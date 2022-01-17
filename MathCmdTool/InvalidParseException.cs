using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class InvalidParseException : MathCmdException
    {
        public InvalidParseException(string value, string expectedValueType)
            : base(string.Format("Invalid Parse Attempt: Cannot parse {0} to {1}.", value, expectedValueType))
        {

        }
    }
}
