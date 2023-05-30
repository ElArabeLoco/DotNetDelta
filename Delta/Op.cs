using System.Collections.Generic;
namespace DotNetDelta
{


    public class Op
    {
        private static readonly IEqualityComparer<object> comparer = EqualityComparer<object>.Default;
        private static readonly IEqualityComparer<Dictionary<string, object>> dictComparer = EqualityComparer<Dictionary<string, object>>.Default;
        
        // Only one property out of {insert, delete, retain} will be present
        public object? insert { get; set; }
        public int? delete { get; set; }
        public object? retain { get; set; }
        public AttributeMap? attributes { get; set; }


        public bool IsInsert()
        {
            return insert is string && insert != null;
        }

        public bool IsInsertEmbed()
        {
            return insert is not string && insert != null;
        }

        public bool IsDelete()
        {
            return delete is not null;
        }

        public bool IsRetain()
        {
            return retain is int && retain != null;
        }

        public bool IsRetainObject()
        {
            return retain is not string && retain != null;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
                

            Op other = (Op)obj;
            if (this.IsInsert() && other.IsInsert())
            {
                return comparer.Equals(this.insert, other.insert) && dictComparer.Equals(this.attributes, other.attributes);
            }
            if (this.IsInsertEmbed() && other.IsInsertEmbed())
            {
                return embedsAreEqual((Dictionary<string, object>) this.insert, (Dictionary<string, object>) other.insert) && dictComparer.Equals(this.attributes, other.attributes);
            }
            if (this.IsRetain() && other.IsRetain() ||
            this.IsRetainObject() && other.IsRetainObject())
            {
                return comparer.Equals(this.retain, other.retain) && dictComparer.Equals(this.attributes, other.attributes);
            }
            if (this.IsDelete() && other.IsDelete())
            {
                return comparer.Equals(this.delete, other.delete);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (this.IsInsert() || this.IsInsertEmbed())
            {
                return comparer.GetHashCode(this.insert) ^ dictComparer.GetHashCode(this.attributes);
            }
            if (this.IsRetain() || this.IsRetainObject())
            {
                return comparer.GetHashCode(this.retain) ^ dictComparer.GetHashCode(this.attributes);
            }
            if (this.IsDelete())
            {
                return comparer.GetHashCode(this.delete);
            }
            return 0;
        }

        private bool embedsAreEqual(Dictionary<string, object>? dict1, Dictionary<string, object>? dict2)
        {
                
            // Check if both dictionaries are null
            if (dict1 == null && dict2 == null)
                return true;

            // Check if either dictionary is null
            if (dict1 == null || dict2 == null)
                return false;

            // Check if dictionaries have the same number of entries
            if (dict1.Count != dict2.Count)
                return false;

            // Check if all keys from dict1 exist in dict2 with the same corresponding values
            return dict1.All(kv => dict2.TryGetValue(kv.Key, out object value) && EqualityComparer<object>.Default.Equals(kv.Value, value));
    
        }

        public override string ToString()
        {
            if (this.IsInsert())
            {
                return "{insert: '" + this.insert + "' , attributes: " + this.attributes + "}";
            }
            if (this.IsInsertEmbed())
            {
                return "{insert: {" + this.insert + "} , attributes: " + this.attributes + "}";
            }
            else if (this.IsDelete())
            {
                return "{delete: " + this.delete + "}";
            }
            else if (this.IsRetain())
            {
                return "{retain: " + this.retain + ", attributes: " + this.attributes + "}";
            }
            else if (this.IsRetainObject())
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

            if (op.retain is int)
            {
                return (int)op.retain;
            }

            if (op.retain is object && op.retain != null)
            {
                return 1;
            }

            return op.insert is string ? ((string)op.insert).Length : 1;
        }

        public static Op Clone(Op op)
        {
            Op newOp = new Op();
            if (op.IsInsert())
            {
                newOp.insert = op.insert;
            }
            else if (op.IsInsertEmbed())
            {
                // Clone Object with a shallow copy
                // TODO: Actually clone the object
                newOp.insert = op.insert;

            }
            else if (op.IsDelete())
            {
                newOp.delete = op.delete;
            }
            else if (op.IsRetain())
            {
                newOp.retain = op.retain;
            }
            else if (op.IsRetainObject())
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

    public static class OpType 
    {
        // Declare a static constant of type string
        public const string Insert = "insert";
        public const string Delete = "delete";
        public const string Retain = "retain";
    }

}