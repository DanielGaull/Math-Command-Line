using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class MStackOverflowException : MathCmdException
    {
        public MStackOverflowException() : base("Stack Overflow (possibly due to an infinite loop in functions). Stack exceeded the maximum " +
            "amount of parse calls")
        {
        }
    }
}
