using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class SetIndexOutOfBoundsException : MathCmdException
    {
        public SetIndexOutOfBoundsException(MList set, int index)
            : base(string.Format("Index out of bounds: Set has length {0} but index {1} was provided", MList.Length(set), index))
        {

        }
    }
}
