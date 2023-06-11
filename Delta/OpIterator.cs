using System.Collections;

namespace DotNetDelta
{

    /// <summary>
    /// An enumerator for an array of Op, with extra utility methods to traverse arrays in meaningful way
    /// </summary>
    public class OpIterator
    {
        
        private int _index; // index of the current op
        private int _offset; // offset into the current op

        private List<Op> ops { get; }

        public OpIterator(List<Op> ops)
        {
            this.ops = ops;
            _index = 0;
            _offset = 0;
        }


        public bool HasNext()
        {
            return PeekLength() < int.MaxValue;
        }

        /// <summary>
        /// Iterate through the ops array, returning the next op. If length is specified, then the next op will be returned only if it is at least length long.
        /// This allows iterating through the ops array, not only by op, but also by character.
        /// </summary>
        /// <param name="length">The character offset for the op that we need</param>
        /// <returns>The sum of the two integers.</returns>
        public Op Next(int length = int.MaxValue)
        {
            if (ops.Count == 0 || _index >= ops.Count)
            {
                return Op.Retain(int.MaxValue);
            }
            Op nextOp = ops[_index];
            int offset = _offset;
            int opLength = Op.Length(nextOp);
            if (length >= opLength - offset)
            {
                // If the requested length is greater than the total length (or the remainder length) of the current op, then we position ourselves in the beginning of the next op
                length = opLength - offset;
                _index++;
                _offset = 0;
            }
            else
            {
                // Otherwise, we just move the offset forward in the current op
                _offset += length;
            }

            // At this point we have the position of the next op and the length of the next op. Now we have to determine what the next op is.
            if (nextOp.IsDelete())
            {
                return Op.Delete(length);
            }
            else
            {
                Op opToReturn = new Op();
                opToReturn.attributes = nextOp.attributes;

                if (nextOp.IsRetain())
                {
                    opToReturn.retain = length;
                }
                else if (nextOp.IsRetainObject())
                {
                    opToReturn.retain = nextOp.retain;
                }
                else if (nextOp.IsInsert())
                {
                    opToReturn.insert = ((string)nextOp.insert).Substring(offset, length);
                }
                else if (nextOp.IsInsertEmbed())
                {
                    opToReturn.insert = nextOp.insert;
                }
                return opToReturn;

            }

        }


        public Op? Peek()
        {
            return ops.Count > 0 ? ops[_index] : null;
        }


        public int PeekLength()
        {
            if (ops.Count == 0 || _index >= ops.Count)
            {
                return int.MaxValue;
            }
            return Op.Length(ops[_index]) - _offset;
        }

        public string PeekType()
        {
            Op? op = ops.Count == 0 || _index >= ops.Count ? null : ops[_index];
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
            return OpType.Retain;
        }


        public List<Op> Rest()
        {
            if (!HasNext())
            {
                return new List<Op>();
            }

            if (_offset == 0)
            {
                return ops.Skip(_index).ToList();
            }
            int offset = _offset;
            int index = _index;
            Op next = Next();
            List<Op> rest = ops.Skip(_index).ToList();
            _offset = offset;
            _index = index;
            // return new Op[] { next }.Concat(rest).ToArray();
            List<Op> list = new List<Op>();
            list.Add(next);
            list.AddRange(rest);
            return list;
        }
    }
}
