using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class InvalidExpressionException : MathCmdException
    {
        public InvalidExpressionException() : base()
        {
        }
        public InvalidExpressionException(string msg) : base("Invalid Expression: " + msg)
        {

        }
    }
}
