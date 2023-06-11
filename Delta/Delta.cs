using System.Text.Json;

namespace DotNetDelta
{
    public class Delta
    {
        // Declare a private static field named handlers of type Dictionary of key type string and value type generic object T
        private static readonly Dictionary<string, object> Handlers = new();


        private List<Op> _ops;

        public List<Op> Ops
        {
            get => _ops;
            set => _ops = value;
        }

        public Delta()
        {
            Ops = new List<Op>();
        }

        public Delta(List<Op> Ops)
        {
            this.Ops = new List<Op>(Ops);
        }

        public Delta(Delta delta)
        {
            Ops = new List<Op>(delta.Ops);
        }


        public Delta Insert(string text, AttributeMap? attributes = null)
        {
            return Push(Op.Insert(text, attributes));
        }

        public Delta Insert(object embed, AttributeMap? attributes = null)
        {
            return Push(Op.InsertEmbed(embed, attributes));
        }

        public Delta Delete(int length)
        {
            return length <= 0 ? this : Push(Op.Delete(length));
        }

        public Delta Retain(int length, AttributeMap? attributes = null)
        {
            return length <= 0 ? this : Push(Op.Retain(length, attributes));
        }

        public Delta Retain(object retainObject, AttributeMap? attributes = null)
        {
            return Push(Op.RetainObject(retainObject, attributes));
        }


        /// <summary>
        /// Adds an operation to the end of the delta, with the possibility of merging it with the previous operation if it is possible. The following are
        /// the rules for merging:
        /// 1. If the last operation is a delete, and the new operation is a delete, then merge them by adding the lengths.
        /// 2. If the last operation is a delete, and the new operation is an insert, put the insert before the delete.
        /// 3. If the last operation is an insert, and the new operation is an insert, and the attributes are equal, then merge them by concatenating the
        ///    strings.
        /// 4. If the last operation is a retain, and the new operation is a retain, and the attributes are equal, then merge them by adding the lengths.
        /// 5. In any other case, do not merge and just add the new operation to the end.
        /// </summary>
        public Delta Push(Op op)
        {
            int index = Ops.Count;
            Op newOp = Op.Clone(op);
            if (index == 0)
            {
                Ops.Add(newOp);
                return this;
            }

            Op lastOp = Ops[index - 1];

            // Merge delete into previous delete
            if (lastOp.IsDelete() && newOp.IsDelete())
            {
                Ops[index - 1] = Op.Delete(lastOp.delete.Value + newOp.delete.Value);
                return this;
            }

            // Put the insert before the delete
            if (lastOp.IsDelete() && (newOp.IsInsert() || newOp.IsInsertEmbed()))
            {
                index--;
                if (index == 0)
                {
                    Ops.Insert(0, newOp);
                    return this;
                }
            }

            if (newOp.attributes?.Any() != true && lastOp.attributes?.Any() != true ||
                (newOp.attributes?.SequenceEqual(lastOp.attributes ?? new AttributeMap()) ?? lastOp.attributes == null))
            {
                // Merge insert into previous insert
                if (lastOp.IsInsert() && newOp.IsInsert())
                {
                    Ops[index - 1] = Op.Insert((string)lastOp.insert + (string)newOp.insert, lastOp.attributes);
                    return this;
                }

                // Merge retain into previous retain
                if (lastOp.IsRetain() && newOp.IsRetain())
                {
                    Ops[index - 1] = Op.Retain((int)lastOp.retain + (int)newOp.retain, lastOp.attributes);
                    return this;
                }
            }

            if (index == this.Ops.Count)
            {
                Ops.Add(newOp);
            }
            else
            {
                Ops.Insert(index, newOp);
            }

            return this;
        }

        public Delta Chop()
        {
            if (Ops.Count > 0)
            {
                Op lastOp = Ops[Ops.Count - 1];
                if (lastOp.IsRetain() && (lastOp.attributes == null || lastOp.attributes.Count == 0))
                {
                    Ops.RemoveAt(Ops.Count - 1);
                }
            }

            return this;
        }

        public List<Op> Filter(Func<Op, int, bool> filter)
        {
            return Ops.Where(filter).ToList();
        }

        public void ForEach(Action<Op, int> action)
        {
            for (int i = 0; i < Ops.Count; i++)
            {
                action(Ops[i], i);
            }
        }

        public List<T> Map<T>(Func<Op, int, T> mappingFunction)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < Ops.Count; i++)
            {
                result.Add(mappingFunction(Ops[i], i));
            }

            return result;
        }

        public (List<Op>, List<Op>) Partition(Func<Op, bool> predicate)
        {
            var passed = new List<Op>();
            var failed = new List<Op>();
            foreach (var op in Ops)
            {
                if (predicate(op))
                {
                    passed.Add(op);
                }
                else
                {
                    failed.Add(op);
                }
            }

            return (passed, failed);
        }

        public T Reduce<T>(Func<T, Op, int, T> reducer, T initialValue)
        {
            T result = initialValue;
            for (var i = 0; i < Ops.Count; i++)
            {
                result = reducer(result, Ops[i], i);
            }

            return result;
        }

        public int ChangeLength()
        {
            return Reduce<int>((accumLength, currentOp, index) =>
            {
                if (currentOp.IsInsert())
                {
                    return accumLength + ((string)currentOp.insert).Length;
                }

                if (currentOp.IsDelete())
                {
                    return accumLength - (int)currentOp.delete;
                }

                return accumLength;
            }, 0);
        }

        public int Length()
        {
            return Reduce((accumLength, currentOp, index) => accumLength + Op.Length(currentOp), 0);
        }

        public Delta Slice(int start = 0, int end = int.MaxValue)
        {
            List<Op> newOps = new List<Op>();
            var opIterator = new OpIterator(Ops);
            var index = 0;
            while (index < end && opIterator.HasNext())
            {
                Op nextOp;
                if (index < start)
                {
                    nextOp = opIterator.Next(start - index);
                }
                else
                {
                    nextOp = opIterator.Next(end - index);
                    newOps.Add(nextOp);
                }

                index += Op.Length(nextOp);
            }

            return new Delta(newOps);
        }


        public Delta Compose(Delta other)
        {
            var thisIter = new OpIterator(Ops);
            var otherIter = new OpIterator(other.Ops);
            var ops = new List<Op>();
            var firstOther = otherIter.Peek();

            if (firstOther != null && firstOther.IsRetain() && firstOther.attributes == null)
            {
                int firstLeft = (int)firstOther.retain;
                while (thisIter.PeekType() == OpType.Insert && thisIter.PeekLength() <= firstLeft)
                {
                    firstLeft -= thisIter.PeekLength();
                    ops.Add(thisIter.Next());
                }

                if ((int)firstOther.retain - firstLeft > 0)
                {
                    otherIter.Next((int)firstOther.retain - firstLeft);
                }
            }

            var delta = new Delta(ops);
            while (thisIter.HasNext() || otherIter.HasNext())
            {
                if (otherIter.PeekType() == OpType.Insert)
                {
                    delta.Push(otherIter.Next());
                }
                else if (thisIter.PeekType() == OpType.Delete)
                {
                    delta.Push(thisIter.Next());
                }
                else
                {
                    var length = Math.Min(thisIter.PeekLength(), otherIter.PeekLength());
                    Op thisOp = thisIter.Next(length);
                    var otherOp = otherIter.Next(length);
                    if (otherOp.IsRetain() || otherOp.IsRetainObject())
                    {
                        // TODO: MOST CODE GOES HERE
                        var newOp = new Op();
                        if (thisOp.IsRetain())
                        {
                            newOp = otherOp.IsRetain() ? Op.Retain(length) : Op.RetainObject(otherOp.retain);
                        }
                        else
                        {
                            if (otherOp.IsRetain())
                            {
                                if (thisOp.retain == null)
                                {
                                    newOp = thisOp.IsInsert()
                                        ? Op.Insert((string)thisOp.insert)
                                        : Op.InsertEmbed(thisOp.insert);
                                }
                                else
                                {
                                    newOp = Op.RetainObject(thisOp.retain);
                                }
                            }
                            else
                            {
                                var thisOpType = thisOp.GetOpType();

                                var thisEmbed = thisOpType == OpType.Insert ? thisOp.insert : thisOp.retain;
                                var (embedType, thisData, otherData) =
                                    IEmbedHandler<Dictionary<string, object>>.GetEmbedTypeAndData(
                                        (Dictionary<string, object>)thisEmbed,
                                        (Dictionary<string, object>)otherOp.retain);
                                var handler = GetEmbedHandler<object>(embedType);
                                newOp = thisOpType == OpType.Insert
                                    ? Op.InsertEmbed(handler.Compose(thisData, otherData, false))
                                    : Op.RetainObject(handler.Compose(thisData, otherData, true));
                            }
                        }

                        // Preserve null when composing with a retain, otherwise remove it for inserts
                        var composedAttributes =
                            AttributeMap.Compose(thisOp.attributes, otherOp.attributes, thisOp.IsRetain());
                        if (composedAttributes != null && composedAttributes.Count > 0)
                        {
                            newOp.attributes = composedAttributes;
                        }

                        delta.Push(newOp);

                        // Optimization if rest of other is just retain
                        if (!otherIter.HasNext() && delta.Ops[delta.Ops.Count - 1].Equals(newOp))
                        {
                            var rest = new Delta(thisIter.Rest());
                            return delta.Concat(rest).Chop();
                        }

                        // Other op should be delete, we could be an insert or retain
                        // Insert + delete cancels out
                    }
                    else if (otherOp.IsDelete() &&
                             (thisOp.IsRetain() || (thisOp.IsRetainObject() && thisOp.retain != null)))
                    {
                        delta.Push(otherOp);
                    }
                }
            }

            return delta.Chop();
        }

        private Delta Concat(Delta other)
        {
            var delta = new Delta(new List<Op>(Ops));
            if (other.Ops.Count > 0)
            {
                delta.Push(other.Ops[0]);
                delta.Ops.AddRange(other.Ops.GetRange(1, other.Ops.Count - 1));
            }

            return delta;
        }


        // Generate Equals and GetHashCode methods
        public override bool Equals(object? obj)
        {
            // Ask if obj is not of type Delta
            if (!(obj is Delta otherDelta))
                return false;

            // Check if both dictionaries are null
            if (otherDelta == null)
                return false;

            // Check if dictionaries have the same number of entries
            if (Ops.Count != otherDelta.Ops.Count)
                return false;

            // Check if all keys from dict1 exist in dict2 with the same corresponding values
            return Ops.SequenceEqual(otherDelta.Ops);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ops);
        }

        public override string ToString()
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, options);
        }


        public static void RegisterEmbed<T>(string embedType, IEmbedHandler<T> embedHandler)
        {
            Handlers.Add(embedType, embedHandler);
        }

        public static void UnregisterEmbed(string embedType)
        {
            Handlers.Remove(embedType);
        }

        private static IEmbedHandler<T> GetEmbedHandler<T>(string embedType)
        {
            var embedHandler = (IEmbedHandler<T>)Handlers[embedType];
            if (embedHandler == null)
            {
                throw new Exception($"Embed type {embedType} not supported");
            }

            return embedHandler;
        }
    }

    /// <summary>
    /// Defines the interface for an embed handler. An embed handler is just a class that defines how to compose,
    /// invert, and transform two embeds.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEmbedHandler<T>
    {
        public T Compose(T a, T b, bool keepNull);
        public T Invert(T a, T b);
        public T Transform(T a, T b, bool priority);

        public static (string, object, object) GetEmbedTypeAndData(
            Dictionary<string, object> a,
            Dictionary<string, object> b)
        {
            if (a == null || b == null)
            {
                throw new Exception("Embeds cannot both be null");
            }

            var embedType = a.Keys.First();
            if (b.Keys.First() != embedType)
            {
                throw new Exception("Embed types must match");
            }

            return (embedType, a[embedType], b[embedType]);
        }
    }
}