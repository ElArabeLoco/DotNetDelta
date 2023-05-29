using System.Collections;

namespace DotNetDelta
{

    /// <summary>
    /// An enumerator for an array of Op, with extra utility methods to traverse arrays in meaningful way
    /// </summary>
    public class OpIterator
    {
        
        private int index; // index of the current op
        private int offset; // offset into the current op

        public List<Op> ops { get; private set; } = new List<Op>();

        public OpIterator(List<Op> ops)
        {
            this.ops = ops;
            index = 0;
            offset = 0;
        }


        public bool HasNext()
        {
            return PeekLength() < int.MaxValue;
        }

        /// <summary>
        /// Iterate through the ops array, returning the next op. If length is specified, then the next op will be returned only if it is at least length long.
        /// This allows iterating through the ops array, not only by op, but also by character.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The sum of the two integers.</returns>
        public Op Next(int length = int.MaxValue)
        {
            if (ops.Count == 0 || index >= ops.Count)
            {
                return Op.Retain(int.MaxValue);
            }
            Op nextOp = ops[index];
            int offset = this.offset;
            int opLength = Op.Length(nextOp);
            if (length >= opLength - offset)
            {
                // If the requested length is greater than the total length (or the remainder length) of the current op, then we position ourselves in the beginning of the next op
                length = opLength - offset;
                this.index++;
                this.offset = 0;
            }
            else
            {
                // Otherwise, we just move the offset forward in the current op
                this.offset += length;
            }

            // At this point we have the position of the next op and the length of the next op. Now we have to determine what the next op is.
            if (nextOp.isDelete())
            {
                return Op.Delete(length);
            }
            else
            {
                Op opToReturn = new Op();
                opToReturn.attributes = nextOp.attributes;

                if (nextOp.isRetain())
                {
                    opToReturn.retain = length;
                }
                else if (nextOp.isRetainObject())
                {
                    opToReturn.retain = nextOp.retain;
                }
                else if (nextOp.isInsert())
                {
                    opToReturn.insert = ((string)nextOp.insert).Substring(offset, length);
                }
                else if (nextOp.isInsertEmbed())
                {
                    opToReturn.insert = nextOp.insert;
                }
                return opToReturn;

            }

        }


        public Op? Peek()
        {
            return ops.Count > 0 ? ops[index] : null;
        }


        public int PeekLength()
        {
            if (ops.Count == 0 || index >= ops.Count)
            {
                return int.MaxValue;
            }
            return Op.Length(ops[index]) - offset;
        }

        public string PeekType()
        {
            Op? op = ops.Count > 0 ? ops[index] : null;
            if (op == null)
            {
                return OpType.Retain;
            }
            if (op.insert != null)
            {
                return OpType.Insert;
            }
            if (op.delete != null)
            {
                return OpType.Delete;
            }
            if (op.retain != null)
            {
                return OpType.Retain;
            }
            return OpType.Retain;
        }


        public List<Op> Rest()
        {
            if (!HasNext())
            {
                return new List<Op>();
            }
            else if (this.offset == 0)
            {
                return this.ops.Skip(this.index).ToList();
            }
            int offset = this.offset;
            int index = this.index;
            Op next = Next();
            List<Op> rest = this.ops.Skip(this.index).ToList();
            this.offset = offset;
            this.index = index;
            // return new Op[] { next }.Concat(rest).ToArray();
            List<Op> list = new List<Op>();
            list.Add(next);
            list.AddRange(rest);
            return list;
        }
    }
}
