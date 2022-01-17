using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    public struct FunctionParameter
    {
        public FunctionParameterTypes Type;
        public string Name;
        public int NumDelegateArgs;

        public FunctionParameter(string name, FunctionParameterTypes type)
        {
            Name = name;
            Type = type;
            NumDelegateArgs = 0;
        }
        public FunctionParameter(string name, int numDelegateArgs)
        {
            Name = name;
            Type = FunctionParameterTypes.Delegate;
            NumDelegateArgs = numDelegateArgs;
        }
    }

    public enum FunctionParameterTypes
    {
        Number,
        Delegate,
        List,
        Vector
    }
}
