using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class InvalidArgumentsException : MathCmdException
    {
        public InvalidArgumentsException() : base()
        {
        }
        public InvalidArgumentsException(string msg) : base("Invalid Argument(s): " + msg)
        {

        }
    }
}
