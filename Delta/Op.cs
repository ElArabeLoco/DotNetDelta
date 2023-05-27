namespace DotNetDelta
{


    public class Op
    {
        // Only one property out of {insert, delete, retain} will be present
        public object? insert { get; set; }
        public int? delete { get; set; }
        public object? retain { get; set; }
        public AttributeMap? attributes { get; set; }

        public static int Length(Op op)
        {
            if (op.delete.HasValue)
            {
                return op.delete.Value;
            }
            else if (op.retain is int)
            {
                return (int) op.retain;
            }
            else if (op.retain is object && op.retain != null)
            {
                return 1;
            }
            else
            {
                return op.insert is string ? ((string) op.insert).Length : 1;
            }
        }



    }

}