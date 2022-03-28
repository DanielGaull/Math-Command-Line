using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class MDataTypeParseException : MathCmdException
    {
        public MDataTypeParseException(string line) : base("Error parsing data type: " + line)
        {
        }
        public MDataTypeParseException(string line, string msg) : base("Error parsing data type: " + msg + "\n" + line)
        {
        }
    }
}
