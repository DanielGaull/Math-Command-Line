using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    abstract class MathCmdException : Exception
    {
        public MathCmdException() : base()
        {
        }
        public MathCmdException(string msg) : base(msg)
        {

        }
    }
}
