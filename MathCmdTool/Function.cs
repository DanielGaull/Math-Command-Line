using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmdTool
{
    public delegate MathDataValue FunctionEvaluation(Ast[] args);
    public delegate MathDataValue ExpressionEvaluation(string expression, Dictionary<string, MathDataValue> variableAssignments);
    public delegate Ast ExpressionParse(string expression);
    public delegate MathDataValue AstEvaluation(Ast args, Dictionary<string, MathDataValue> variableAssignments);
    public class Function
    {
        //public Functions FunctionType { get; }
        //public int NumArgs { get; } // < 0 arguments means that the function takes unlimited arguments
        public List<FunctionParameter> Parameters { get; private set; }
        public string[] Tokens { get; }
        private FunctionEvaluation evaluation;
        public string Description { get; set; }

        private static List<Function> funcs;
        private static List<string> funcTokens;
        public static List<string> AllFunctionTokens { get { return funcTokens; } }

        private static ExpressionEvaluation exprEvaluator;
        private static ExpressionParse exprParser;
        private static AstEvaluation astEvaluator;

        public Function(List<FunctionParameter> parameters, string[] names, FunctionEvaluation evaluation)
            : this(parameters, names, evaluation, "")
        {
        }
        public Function(List<FunctionParameter> parameters, string[] names, FunctionEvaluation evaluation, string desc)
        {
            Parameters = parameters;
            Tokens = names;
            this.evaluation = evaluation;
            Description = desc;
        }

        public MathDataValue Evaluate(Ast[] args)
        {
            return evaluation(args);
        }

        public static void Init(ExpressionEvaluation exprEvaluator, ExpressionParse exprParser, AstEvaluation astEvaluator)
        {
            Function.exprEvaluator = exprEvaluator;
            Function.exprParser = exprParser;
            Function.astEvaluator = astEvaluator;

            funcs = new List<Function>();

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "sin" }, x => new MathDataValue(Math.Sin(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "cos" }, x => new MathDataValue(Math.Cos(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "tan" }, x => new MathDataValue(Math.Tan(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "csc" }, x => new MathDataValue(1 / Math.Sin(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "sec" }, x => new MathDataValue(1 / Math.Cos(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "cot" }, x => new MathDataValue(1 / Math.Tan(x[0].Value))));

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "asin" }, x => new MathDataValue(1 / Math.Asin(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "acos" }, x => new MathDataValue(1 / Math.Acos(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "atan" }, x => new MathDataValue(1 / Math.Atan(x[0].Value))));

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "abs" }, x => new MathDataValue(Math.Abs(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) }, 
                new string[] { "sign" }, x => new MathDataValue(Math.Sign(x[0].Value)), "Returns 1 if the argument is positive, and " +
                "-1 if the argument is negative"));

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "round" }, x => new MathDataValue(Math.Round(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "floor" }, x => new MathDataValue(Math.Floor(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "ceil" }, x => new MathDataValue(Math.Ceiling(x[0].Value))));

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "exp" }, x => new MathDataValue(Math.Exp(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "ln" }, x => new MathDataValue(Math.Log(x[0].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("argument", FunctionParameterTypes.Number),
                    new FunctionParameter("base", FunctionParameterTypes.Number) },
                new string[] { "log" }, x => new MathDataValue(Math.Log(x[0].Value, x[1].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("base", FunctionParameterTypes.Number),
                    new FunctionParameter("power", FunctionParameterTypes.Number) },
                new string[] { "pow" }, x => new MathDataValue(Math.Pow(x[0].Value, x[1].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("argument", FunctionParameterTypes.Number),
                    new FunctionParameter("root", FunctionParameterTypes.Number) },
                new string[] { "root", "rt" }, x => new MathDataValue(Math.Pow(x[0].Value, 1 / x[1].Value))));
            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "sqrt" }, x => new MathDataValue(Math.Sqrt(x[0].Value))));

            funcs.Add(new Function(new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "fact" }, x =>
                new MathDataValue(Enumerable.Range(1, (int)x[0].Value).Aggregate(1, (p, item) => p * item))));

            funcs.Add(new Function(new List<FunctionParameter>()
                {
                    new FunctionParameter("list", FunctionParameterTypes.List),
                }, new string[] { "max" }, x => new MathDataValue(x.First().Contents.Select(x => x.Value).Max())));
            funcs.Add(new Function(new List<FunctionParameter>()
                {
                    new FunctionParameter("list", FunctionParameterTypes.List),
                }, new string[] { "min" }, x => new MathDataValue(x.First().Contents.Select(x => x.Value).Min())));
            funcs.Add(new Function(new List<FunctionParameter>()
                {
                    new FunctionParameter("list", FunctionParameterTypes.List),
                }, new string[] { "avg", "amean" }, x => new MathDataValue(x.First().Contents.Select(x => x.Value).Average())));

            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("expression", 1),
                new FunctionParameter("start", FunctionParameterTypes.Number),
                new FunctionParameter("max", FunctionParameterTypes.Number)
            }, new string[] { "sum" }, x => new MathDataValue(SummationFunction(x)), "Performs a summation"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("expression", 1),
                new FunctionParameter("lower_bound", FunctionParameterTypes.Number),
                new FunctionParameter("upper_bound", FunctionParameterTypes.Number)
            }, new string[] { "int" }, x => new MathDataValue(Integrate(x)), "Returns the integral of the expression, between lower_bound " +
                "and upper_bound"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("expression", 1),
                new FunctionParameter("variable_value", FunctionParameterTypes.Number)
            }, new string[] { "diff" }, x => DerivativeFunc(x), "Takes the derivative of the provided expression, at the specified value"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("expression", 1),
            }, new string[] { "solve" }, x => new MathDataValue(Solve(new Ast[] { x[0], Ast.CreateNumberAst(0) })),
                "Solves for where the expression is equal to 0 (using an initial guess of 0)"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("expression", 1),
                new FunctionParameter("start_value", FunctionParameterTypes.Number)
            }, new string[] { "solves" }, x => new MathDataValue(Solve(x)), "Solves for where the expression is equal to zero, taking " +
                "in an intial guess"));

            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("element", FunctionParameterTypes.Number)
            }, new string[] { "add" }, x => new MathDataValue(MList.Add(new MList(x[0]), new MathDataValue(x[1]))), 
                "Adds a value to a list and returns the new list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
            }, new string[] { "len", "length" }, x => new MathDataValue(MList.Length(new MList(x[0]))), 
                "Returns the length of the provided list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("index", FunctionParameterTypes.Number)
            }, new string[] { "remove" }, x => new MathDataValue(MList.Remove(new MList(x[0]), (int) x[1].Value)), 
                "Removes the value at the specified index from the list and returns the new list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("index", FunctionParameterTypes.Number)
            }, new string[] { "get" }, x => MList.Get(new MList(x[0]), (int)x[1].Value),
            "Returns the value at the specified index in the provided list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("element", FunctionParameterTypes.Number),
                new FunctionParameter("index", FunctionParameterTypes.Number)
            }, new string[] { "insert" }, x => new MathDataValue(MList.Insert(new MList(x[0]), (int)x[2].Value, new MathDataValue(x[1]))),
            "Inserts the provided value after the specified index in the provided list and returns the new list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list1", FunctionParameterTypes.List),
                new FunctionParameter("list2", FunctionParameterTypes.List)
            }, new string[] { "concat", "union" }, x => new MathDataValue(MList.Union(new MList(x[0]), new MList(x[1]))),
            "Returns a list of all values in each of list1 and list2"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list1", FunctionParameterTypes.List),
                new FunctionParameter("list2", FunctionParameterTypes.List)
            }, new string[] { "intersect" }, x => new MathDataValue(MList.Intersect(new MList(x[0]), new MList(x[1]))), 
            "Returns a list of all values that are contained both in list1 and in list2"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list1", FunctionParameterTypes.List),
                new FunctionParameter("list2", FunctionParameterTypes.List)
            }, new string[] { "less" }, x => new MathDataValue(MList.Less(new MList(x[0]), new MList(x[1]))),
            "Returns a list of all values that are contained in list1 but not in list2"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("element", FunctionParameterTypes.Number)
            }, new string[] { "indexof" }, x => new MathDataValue(MList.IndexOf(new MList(x[0]), new MathDataValue(x[1]))),
            "Returns the first index of the specified value in the provided list, or -1 if it does not appear in the provided list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List),
                new FunctionParameter("delegate", 1)
            }, new string[] { "foreach" }, x => new MathDataValue(MList.ForEach(new MList(x[0]), x[1])),
            "Performs a delegate operation on each value of a list, and returns the new list"));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("list", FunctionParameterTypes.List)
            }, new string[] { "join" }, x => new MathDataValue(MList.Join(new MList(x[0]))),
            "Combines all elements in a list of lists into one list, and returns that new list"));

            funcs.Add(new Function(
                new List<FunctionParameter>() { new FunctionParameter("value", FunctionParameterTypes.Number) },
                new string[] { "toascii" }, x => new MathDataValue(((int)x[0].Value).ToString("X").First()),
                "Converts integers to the ASCII equivelants of the hexadecimal values (ex. 15 -> 'F' -> 70"));

            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("vector1", FunctionParameterTypes.Vector),
                new FunctionParameter("vector2", FunctionParameterTypes.Vector)
            }, new string[] { "dotproduct", "dot" }, x => new MathDataValue(MVector.DotProduct(new MVector(x[0]), new MVector(x[1])))));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("vector1", FunctionParameterTypes.Vector),
                new FunctionParameter("vector2", FunctionParameterTypes.Vector)
            }, new string[] { "crossproduct", "cross" }, x => new MathDataValue(MVector.CrossProduct(new MVector(x[0]), new MVector(x[1])))));
            funcs.Add(new Function(new List<FunctionParameter>()
            {
                new FunctionParameter("vector", FunctionParameterTypes.Vector)
            }, new string[] { "magnitude", "mag" }, x => new MathDataValue(MVector.Magnitude(new MVector(x[0])))));

            // Create list of all function tokens
            funcTokens = new List<string>();
            for (int i = 0; i < funcs.Count; i++)
            {
                funcTokens.AddRange(funcs[i].Tokens);
            }
        }
        private static double SummationFunction(Ast[] args)
        {
            int n = (int)args[1].Value;
            int stop = (int)args[2].Value;
            double sum = 0;
            Dictionary<string, MathDataValue> varAssignments = new Dictionary<string, MathDataValue>();
            varAssignments.Add(args[0].VarNames[0], new MathDataValue(n));
            for (int i = n; i < stop + 1; i++)
            {
                varAssignments[args[0].VarNames[0]] = new MathDataValue(i);
                sum += exprEvaluator(args[0].Expression, varAssignments).NumberValue;
            }
            return sum;
        }

        const int INTEGRAL_INTERVALS = 100000;
        private static double Integrate(Ast[] args)
        {
            double sum = 0;

            double lower = args[1].Value;
            double upper = args[2].Value;
            double deltaVar = (upper - lower) / INTEGRAL_INTERVALS;
            double intVarVal = lower + deltaVar;
            Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
            variables.Add(args[0].VarNames[0], new MathDataValue(intVarVal));
            // We are going to do a trapezoid sum here. Formula is
            // dx/2 * (f(x0) + 2f(x1) + 2f(x2) + 2f(x3) + ... + f(xn))
            // We will sum starting from f(x1) to f(x n-1), then complete the formula at the end
            for (int i = 0; i < INTEGRAL_INTERVALS - 2; i++)
            {
                // Evaluate at this value of the integral variable
                double value = exprEvaluator(args[0].Expression, variables).NumberValue;
                // Multiply by the "dx" and add to the sum
                sum += 2 * value;

                // Increment the delta variable (ex the "dx")
                intVarVal += deltaVar;
                variables[args[0].VarNames[0]] = new MathDataValue(intVarVal);
            }

            variables[args[0].VarNames[0]] = new MathDataValue(lower);
            double fx0 = exprEvaluator(args[0].Expression, variables).NumberValue;
            variables[args[0].VarNames[0]] = new MathDataValue(upper);
            double fxn = exprEvaluator(args[0].Expression, variables).NumberValue;
            return Round(deltaVar / 2 * (fx0 + sum + fxn));
        }

        static readonly double[] DERIVATIVE_DELTA_VARS = new double[]
        {
            0.00001, 0.000001, 0.0000001, 0.000000001
        };
        static MathDataValue DerivativeFunc(Ast[] args)
        {
            Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
            variables.Add(args[0].VarNames[0], new MathDataValue(args[1].Value));
            Ast parsed = exprParser(args[0].Expression);
            bool evaluateAsVector = false;
            if (parsed.Type == AstTypes.VectorLiteral)
            {
                evaluateAsVector = true;
            }
            if (parsed.Type == AstTypes.Function)
            {
                // Determine what type this function returns
                // Evaluate at the default value
                MathDataValue defaultEval = astEvaluator(parsed, variables);
                if (defaultEval.Type == MathDataTypes.Vector)
                {
                    evaluateAsVector = true;
                }
            }
            if (evaluateAsVector)
            {
                return new MathDataValue(DifferentiateVector(args[1].Value, args[0].VarNames[0], exprParser(args[0].Expression)));
            }
            else
            {
                return new MathDataValue(Differentiate(args[1].Value, args[0].VarNames[0], exprParser(args[0].Expression)));
            }
        }
        static MVector DifferentiateVector(double varValue, string diffVar, Ast ast)
        {
            double diffVarVal = varValue;
            Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
            variables.Add(diffVar, new MathDataValue(diffVarVal));
            // Evaluate the derivative with approximation
            MVector diffSum = astEvaluator(ast, variables).VectorValue.SetAllTo(0);
            for (int i = 0; i < DERIVATIVE_DELTA_VARS.Length; i++)
            {
                variables[diffVar] = new MathDataValue(diffVarVal);
                MVector orig = astEvaluator(ast, variables).VectorValue;
                variables[diffVar] = new MathDataValue(diffVarVal + DERIVATIVE_DELTA_VARS[i]);
                MVector changed = astEvaluator(ast, variables).VectorValue;
                diffSum += MVector.Scale(changed - orig, 1 / DERIVATIVE_DELTA_VARS[i]);
            }
            diffSum = MVector.Scale(diffSum, 1.0 / DERIVATIVE_DELTA_VARS.Length);
            for (int i = 0; i < diffSum.NumComponents; i++)
            {
                diffSum.Components[i] = Round(diffSum.Components[i]);
            }
            return diffSum;
        }
        static double Differentiate(double varValue, string diffVar, Ast ast)
        {
            double diffVarVal = varValue;
            Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
            variables.Add(diffVar, new MathDataValue(diffVarVal));
            // Evaluate the derivative with approximation
            double diffSum = 0;
            for (int i = 0; i < DERIVATIVE_DELTA_VARS.Length; i++)
            {
                variables[diffVar] = new MathDataValue(diffVarVal);
                double orig = astEvaluator(ast, variables).NumberValue;
                variables[diffVar] = new MathDataValue(diffVarVal + DERIVATIVE_DELTA_VARS[i]);
                double changed = astEvaluator(ast, variables).NumberValue;
                diffSum += (changed - orig) / DERIVATIVE_DELTA_VARS[i];
            }
            return Round(diffSum / DERIVATIVE_DELTA_VARS.Length);
        }

        const int MAX_NEWTON_METHOD_ITERATIONS = 1000;
        const double MIN_NEWTON_METHOD_DIFF = 0.001;
        static double Solve(Ast[] args)
        {
            double startVal = args[1].Value;
            Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
            variables.Add(args[0].VarNames[0], new MathDataValue(startVal));

            double expVal = exprEvaluator(args[0].Expression, variables).NumberValue;
            double varVal = startVal;
            int iterations = 0;
            // Do Newton's method
            while (Math.Abs(expVal) > MIN_NEWTON_METHOD_DIFF && iterations < MAX_NEWTON_METHOD_ITERATIONS)
            {
                double derivative = Differentiate(varVal, args[0].VarNames[0], exprParser(args[0].Expression));
                    //Differentiate(expression, sVar, varVal.ToString());
                if (derivative == 0)
                {
                    // This guess won't work, so we're going to try and move the variable value and then restart
                    varVal += 1;
                }
                else
                {
                    varVal = varVal - expVal / derivative;
                }
                variables[args[0].VarNames[0]] = new MathDataValue(varVal);
                expVal = exprEvaluator(args[0].Expression, variables).NumberValue;
                iterations++;
            }
            if (iterations >= MAX_NEWTON_METHOD_ITERATIONS)
            {
                return double.NaN;
            }
            return Round(varVal);
        }

        private static double Round(double value)
        {
            value = Math.Round(value, 5);
            if (value == -0)
            {
                return 0;
            }
            return value;
        }
        
        public static Function GetFunction(string token)
        {
            foreach (Function function in funcs)
            {
                if (function.Tokens.Contains(token))
                {
                    return function;
                }
            }
            return null;
        }

        public static IEnumerable<Function> GetAllFunctions()
        {
            return funcs;
        }

        public override string ToString()
        {
            return ToString(false, true);
        }
        public string ToString(bool formatted, bool treatNumAndlistAsVar)
        {
            StringBuilder fmBuilder = new StringBuilder();
            for (int i = 0; i < Tokens.Length; i++)
            {
                if (formatted)
                {
                    fmBuilder.Append("§B");
                }
                fmBuilder.Append(Tokens[i]);
                if (i + 1 < Tokens.Length)
                {
                    if (formatted)
                    {
                        fmBuilder.Append("§F");
                    }
                    fmBuilder.Append("/");
                }
            }
            if (formatted)
            {
                fmBuilder.Append("§F");
            }
            fmBuilder.Append("(");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (formatted)
                {
                    fmBuilder.Append("§A");
                }
                switch (Parameters[i].Type)
                {
                    case FunctionParameterTypes.Delegate:
                        fmBuilder.Append("lambda");
                        if (formatted)
                        {
                            fmBuilder.Append("§7");
                        }
                        fmBuilder.Append("[" + Parameters[i].NumDelegateArgs + "] ");
                        break;
                    case FunctionParameterTypes.Number:
                        if (treatNumAndlistAsVar)
                        {
                            fmBuilder.Append("var ");
                        }
                        else
                        {
                            fmBuilder.Append("number ");
                        }
                        break;
                    case FunctionParameterTypes.List:
                        if (treatNumAndlistAsVar)
                        {
                            fmBuilder.Append("var ");
                        }
                        else
                        {
                            fmBuilder.Append("list ");
                        }
                        break;
                }
                if (formatted)
                {
                    fmBuilder.Append("§F");
                }
                fmBuilder.Append(Parameters[i].Name);
                if (i + 1 < Parameters.Count)
                {
                    fmBuilder.Append(", ");
                }
            }

            fmBuilder.Append(")");
            return fmBuilder.ToString();
        }
    }
    /*public enum Functions
    {
        Sin,
        Cos,
        Tan,
        Csc,
        Sec,
        Cot,

        Floor,
        Ceil,
        Round,

        Abs,
        Sign,

        ArcSin,
        ArcCos,
        ArcTan,

        Exp,
        Log,
        Ln,
        Power,
        Root,
        Sqrt,

        Max,
        Min,
        Average,

        Factorial,

        Sum,
        Integral,
        Derivative,
        Limit,
        Solve,
        SolveS,

        Add,
        Intersect,
        Union,
        Less,
        Get,
        Remove,
        Insert,
        GetLength,
        IndexOf,
        Foreach,

        ToAscii,

        _Custom
    }*/
}
