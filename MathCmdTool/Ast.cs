using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmdTool
{
    public class Ast
    {
        public AstTypes Type { get; private set; }

        // For Number ASTs; the value it has (if a variable is used, then the tokenizer will pull that variable out)
        public double Value;
        // For binary operation ASTs; each side of the equation
        public Ast Left;
        public Ast Right;
        public BinaryOperations Operation;
        // For function ASTs
        public string FunctionName;
        public List<Ast> Arguments;
        // For variable ASTs
        public string VarName;
        // For delegates
        public string[] VarNames;
        public string Expression;
        // For sets
        public Ast[] Contents;

        public override string ToString()
        {
            switch (Type)
            {
                case AstTypes.Number:
                    return Value.ToString();
                case AstTypes.BinaryOperation:
                    string op = null;
                    switch (Operation) 
                    {
                        case BinaryOperations.Addition:
                            op = "+";
                            break;
                        case BinaryOperations.Subtraction:
                            op = "-";
                            break;
                        case BinaryOperations.Multiplication:
                            op = "*";
                            break;
                        case BinaryOperations.Division:
                            op = "/";
                            break;
                        case BinaryOperations.Modulus:
                            op = "%";
                            break;
                    }
                    if (op != null)
                    {
                        return Left.ToString() + " " + op + " " + Right.ToString();
                    }
                    return "Invalid Binary Operation AST";
                case AstTypes.Function:
                    string funcStr = "Function: " + FunctionName + "(";
                    for (int i = 0; i < Arguments.Count; i++)
                    {
                        funcStr += Arguments[i].ToString();
                        if (i + 1 < Arguments.Count)
                        {
                            funcStr += ",";
                        }
                    }
                    return funcStr + ")";
                case AstTypes.Variable:
                    return "Var: " + VarName;
                case AstTypes.ListLiteral:
                    string elemStr = "";
                    for (int i = 0; i < Contents.Length; i++)
                    {
                        elemStr += Contents[i].ToString();
                        if (i + 1 < Contents.Length)
                        {
                            elemStr += ",";
                        }
                    }
                    return "{" + elemStr + "}";
                case AstTypes.VectorLiteral:
                    string compStr = "";
                    for (int i = 0; i < Contents.Length; i++)
                    {
                        compStr += Contents[i].ToString();
                        if (i + 1 < Contents.Length)
                        {
                            compStr += ",";
                        }
                    }
                    return "<" + compStr + ">";
                case AstTypes.Delegate:
                    string delStr = "[";
                    for (int i = 0; i < VarNames.Length; i++)
                    {
                        delStr += VarNames[i];
                        if (i + 1 < VarNames.Length)
                        {
                            delStr += ",";
                        }
                    }
                    return delStr + ":" + Expression + "]";
            }
            return "Invalid AST Type";
        }
        public static Ast CreateNumberAst(double value)
        {
            return new Ast() { Type = AstTypes.Number, Value = value };
        }
        public static Ast CreateBinOpAst(Ast left, Ast right, BinaryOperations operation)
        {
            return new Ast() { Type = AstTypes.BinaryOperation, Left = left, Right = right, Operation = operation };
        }
        public static Ast CreateFunctionAst(string functionName, List<Ast> arguments)
        {
            return new Ast() { Type = AstTypes.Function, FunctionName = functionName, Arguments = arguments };
        }
        public static Ast CreateVariableAst(string varName)
        {
            return new Ast() { Type = AstTypes.Variable, VarName = varName };
        }
        public static Ast CreateListAst(Ast[] contents)
        {
            return new Ast() { Type = AstTypes.ListLiteral, Contents = contents };
        }
        public static Ast CreateDelegateAst(string[] varNames, string expression)
        {
            return new Ast() { Type = AstTypes.Delegate, VarNames = varNames, Expression = expression };
        }
        public static Ast CreateVectorAst(Ast[] contents)
        {
            return new Ast() { Type = AstTypes.VectorLiteral, Contents = contents };
        }

        public Ast Clone()
        {
            switch (Type)
            {
                case AstTypes.Number: return CreateNumberAst(Value);
                case AstTypes.Variable: return CreateVariableAst(VarName);
                case AstTypes.BinaryOperation: return CreateBinOpAst(Left.Clone(), Right.Clone(), Operation);
                case AstTypes.Function: return CreateFunctionAst(FunctionName, new List<Ast>(Arguments.Select(x => x.Clone()).ToList()));
                case AstTypes.ListLiteral: return CreateListAst(Contents.Select(x => x.Clone()).ToArray());
                case AstTypes.VectorLiteral: return CreateVectorAst(Contents.Select(x => x.Clone()).ToArray());
                case AstTypes.Delegate: return CreateDelegateAst((string[])VarNames.Clone(), Expression);
            }
            return null;
        }
    }

    public enum AstTypes
    {
        Number,
        BinaryOperation,
        Function,
        Variable,
        Delegate,
        ListLiteral,
        VectorLiteral
    }
    public enum BinaryOperations
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus
    }
}
