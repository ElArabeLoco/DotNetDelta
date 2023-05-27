namespace DotNetDelta
{

    public class Delta
    {
        
        public List<Op> Ops { get; set; }

        public Delta()
        {
            this.Ops = new List<Op>();
        }
        public Delta(Op[] Ops)
        {
            this.Ops = new List<Op>(Ops);
        }

        public Delta(Delta delta)
        {
            this.Ops = new List<Op>(delta.Ops);
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
            Op newOp = Op.clone(op);
            if (index == 0)
            {
                System.Console.WriteLine("Adding first op");
                Ops.Add(newOp);
                return this;
            }

            Op lastOp = Ops[index - 1];

            // Merge delete into previous delete
            if (lastOp.isDelete() && newOp.isDelete())
            {
                Ops[index - 1] = Op.Delete(lastOp.delete.Value + newOp.delete.Value);
                return this;
            }
            
            // Put the insert before the delete
            if (lastOp.isDelete() && (newOp.isInsert() || newOp.isInsertEmbed()))
            {
                index--;
                if (index == 0)
                {
                    Ops.Insert(0, newOp);
                    return this;
                }
            }

            if (newOp.attributes?.SequenceEqual(lastOp.attributes) ?? lastOp.attributes == null)
            {   
                System.Console.WriteLine("Both attributes are equal");
                // Merge insert into previous insert
                if (lastOp.isInsert() && newOp.isInsert())
                {
                    System.Console.WriteLine("Merging inserts");
                    Ops[index - 1] = Op.Insert((string) lastOp.insert + (string) newOp.insert, lastOp.attributes);
                    return this;
                }

                // Merge retain into previous retain
                if (lastOp.isRetain() && newOp.isRetain())
                {
                    Ops[index - 1] = Op.Retain((int) lastOp.retain + (int) newOp.retain, lastOp.attributes);
                    return this;
                }
            }

            if (index == this.Ops.Count)
            {
                System.Console.WriteLine("Adding op to end");
                Ops.Add(newOp);
            }
            else
            {
                System.Console.WriteLine("Inserting op at index " + index);
                Ops.Insert(index, newOp);
            }
            return this;

        }



    }
}