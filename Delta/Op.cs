using System.Collections.Generic;
namespace DotNetDelta
{


    public class Op
    {
        private static IEqualityComparer<object> comparer = EqualityComparer<object>.Default;
        private static IEqualityComparer<Dictionary<string, object>> dictComparer = EqualityComparer<Dictionary<string, object>>.Default;
        
        // Only one property out of {insert, delete, retain} will be present
        public object? insert { get; set; }
        public int? delete { get; set; }
        public object? retain { get; set; }
        public AttributeMap? attributes { get; set; }


        public bool isInsert()
        {
            return insert is string && insert != null;
        }

        public bool isInsertEmbed()
        {
            return insert is object && insert != null;
        }

        public bool isDelete()
        {
            return delete is int && delete != null;
        }

        public bool isRetain()
        {
            return retain is int && retain != null;
        }

        public bool isRetainObject()
        {
            return retain is object && retain != null;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
                

            Op other = (Op)obj;
            if (this.isInsert() && other.isInsert() ||
            this.isInsertEmbed() && other.isInsertEmbed())
            {
                return comparer.Equals(this.insert, other.insert) && dictComparer.Equals(this.attributes, other.attributes);
            }
            if (this.isRetain() && other.isRetain() ||
            this.isRetainObject() && other.isRetainObject())
            {
                return comparer.Equals(this.retain, other.retain) && dictComparer.Equals(this.attributes, other.attributes);
            }
            if (this.isDelete() && other.isDelete())
            {
                return comparer.Equals(this.delete, other.delete);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (this.isInsert() || this.isInsertEmbed())
            {
                return comparer.GetHashCode(this.insert) ^ dictComparer.GetHashCode(this.attributes);
            }
            if (this.isRetain() || this.isRetainObject())
            {
                return comparer.GetHashCode(this.retain) ^ dictComparer.GetHashCode(this.attributes);
            }
            if (this.isDelete())
            {
                return comparer.GetHashCode(this.delete);
            }
            return 0;
        }

        public override string ToString()
        {
            if (this.isInsert())
            {
                return "{insert: '" + this.insert + "' , attributes: " + this.attributes + "}";
            }
            else if (this.isDelete())
            {
                return "{delete: " + this.delete + "}";
            }
            else if (this.isRetain())
            {
                return "{retain: " + this.retain + ", attributes: " + this.attributes + "}";
            }
            else if (this.isRetainObject())
            {
                return "{retain: {" + this.retain + "}, attributes: " + this.attributes + "}";
            }
            else
            {
                return "InvalidOp";
            }
        }

        public static Op Insert(string insert, AttributeMap? attributes = null)
        {
            Op op = new Op();
            op.insert = insert;
            op.attributes = attributes;
            return op;
        }

        public static Op InsertEmbed(object embed, AttributeMap? attributes = null)
        {
            Op op = new Op();
            op.insert = embed;
            op.attributes = attributes;
            return op;
        }

        public static Op Delete(int length)
        {
            Op op = new Op();
            op.delete = length;
            return op;
        }

        public static Op Retain(int length, AttributeMap? attributes = null)
        {
            Op op = new Op();
            op.retain = length;
            op.attributes = attributes;
            return op;
        }

        public static Op RetainObject(object retainObject, AttributeMap? attributes = null)
        {
            Op op = new Op();
            op.retain = retainObject;
            op.attributes = attributes;
            return op;
        }

        public static int Length(Op op)
        {
            if (op.delete.HasValue)
            {
                return op.delete.Value;
            }
            else if (op.retain is int)
            {
                return (int)op.retain;
            }
            else if (op.retain is object && op.retain != null)
            {
                return 1;
            }
            else
            {
                return op.insert is string ? ((string)op.insert).Length : 1;
            }
        }

        public static Op clone(Op op)
        {
            Op newOp = new Op();
            if (op.isInsert())
            {
                newOp.insert = op.insert;
            }
            else if (op.isInsertEmbed())
            {
                // Clone Object with a shallow copy
                // TODO: Actually clone the object
                newOp.insert = op.insert;

            }
            else if (op.isDelete())
            {
                newOp.delete = op.delete;
            }
            else if (op.isRetain())
            {
                newOp.retain = op.retain;
            }
            else if (op.isRetainObject())
            {
                // Clone Object with a shallow copy
                // TODO: Actually clone the object
                newOp.retain = op.retain;
            }
            if (op.attributes != null)
            {
                // Clone Dictionary
                newOp.attributes = new AttributeMap(op.attributes);
            }
            return newOp;
        }



    }

}