using System.Collections;

namespace DotNetDelta {

    class OpEnumerator : IEnumerator<Op>
    {
        private Op[] ops;
        private int index;
        private int offset;

        public OpEnumerator(Op[] ops)
        {
            this.ops = ops;
            index = 0;
            offset = 0;
        }

        public Op Current => ops[index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            if (index < ops.Length)
            {
                index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}