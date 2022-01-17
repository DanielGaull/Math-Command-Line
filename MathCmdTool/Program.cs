using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCmdTool
{
    class Program
    {
        static readonly string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"gaull_cmd_tools\math");
        static readonly string varsPath = Path.Combine(basePath, "vars.txt");
        static readonly string customFuncsPath = Path.Combine(basePath, "functions.txt");

        static Dictionary<string, MathDataValue> variables = new Dictionary<string, MathDataValue>();
        static readonly Dictionary<string, double> CONSTANTS = new Dictionary<string, double>()
        {
            { "PI", Math.PI },
            { "E", Math.E },
            { "INF", double.PositiveInfinity }
        };
        const string TIME_CONSTANT = "MILLIS";
        static Dictionary<string, Function> customFunctions = new Dictionary<string, Function>();

        static int parseCallsInFrame = 0;

        const string EXPRESSION_REGEX = @"([*()\/,\%\[\]:\{\}<>" + "\"" + @"]|(?<!E)[\+\-])";
        const string PREVIOUS_VALUE_VAR_NAME = "ans";
        static readonly char[] ALLOWED_VAR_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_".ToCharArray();

        const string HELP_STRING = "§a== Commands ==§f\ndelvar [variable name]\tDeletes a variable. Providing '~' as the argument deletes " +
            "all variables\nfunction [name] [comma-separated arguments] [function expression]\t Creates a custom function. " +
            "(for more on functions, see the Functions section below)\n" +
            "delf [function name]\tDeletes a function. Providing '~' as the argument deletes all functions\n" +
            "display [list]\tConverts each element of a list to a character and displays it. (See the Display section below)\n" +
            "vars\tLists all variables\nconstss\tLists all constants\nfunctions\tLists all functions\n\ncolors\tLists all colors\n" +
            "Simply input a math expression to evaluate it" +
            "\nTo declare and assign variables, simply use §7[variable name]=[value or expression]§f\n" +
            "The variable \"" + PREVIOUS_VALUE_VAR_NAME + "\" is used to store the result of the previous evaluation\n" +
            "\n§B== Data Values ==§F\nThis tool has 3 data types: §Anumbers§F, §Alists§F, and §avectors§f. §ALists§F are lists of any other" +
            "data type (§alists§f can contain §anumbers§f, §avectors§f, and even other §alists§f), and can contain " +
            "duplicate values. To define a §alist§F, type {} around the comma-separated value list (ex. §7list={1,2,3}§f). " +
            "§aNumbers§f are simply all real numbers, including the special values of infinity (INF), negative infinity, and NaN. " +
            "Data types have a sort of trinity, meaning each can be interpreted as the other. If a §alist§f or §avector§f is treated as a §anumber§f, " +
            "then its length will be used (ex. §7{1,2,3}+1§f evaluates to §74§f). If a §anumber§f is treated as a §alist§f or §avector§f, " +
            "then it will be interpreted as a §alist§f or §avector§f in which that §anumber§f is the only element (ex. §7union(1,2)§f evaluates to " +
            "§7{1,2}§f). §aVectors§f and §alists§f interpreted as each other will simply convert their elements to the other (ex." +
            "§7union(<1,2>,<3,4>)§f evaluates to §7{1,2,3,4§f and §7dot({1,2},{3,4})§f evaluates to §711§f)" +
            "\n\n§b== Operators ==§f\n+ Adds two §anumbers§f or §avectors§f\n- Subtracts two §anumbers§f or §avectors§f\n" +
            "* Multiplies two §anumbers§f or a §avector§f by a scalar §anumber§f" +
            "\n/ Divides two §anumbers§f or a §avector§f by a scalar §anumber§f\n% Takes the modulus of two §anumbers§f" +
            "\n\n§b== Functions ==§f\nFunctions are similar to mathematical functions. They take in a number of arguments " +
            "and perform some sort of calculation to output a result. Functions can take in §anumbers§f/§alists§f/§avectors§f (sometimes simply " +
            "referred to collectively as a \"var\" argument, due to the trinity of §anumbers§f and §alists§f). To define a function, use " +
            "the function command as such:\n\tExample: §7function simpleadd x,y x+y§f\n\n§b== Delegates ==§f\n" +
            "Additionally, function arguments can include the §adelegate§f type, which is a sort of function in itself. §aDelegates§f, " +
            "like functions, take in arguments, but these can only be var arguments, and §adelegates§f cannot take §adelegates§f as " +
            "arguments. Essentially, §adelegates§f are a way to pass an expression into a function. The form of a §adelegate§f is \n\t" +
            "§7[(comma-separated variable list):(expression)]\n\t§fExample: §7[x,y:x*y+2]\nWhen defining function parameters, " +
            "use parenthesis to specify an argument as a delegate, as such:\n\tExample: §7function rundelegate x,delegate[1] delegate(x)\n" +
            "§fAs shown, to use a delegate in a custom function, treat the delegate as a function.\n\n§b== Display Command ==§f\n" +
            "This command will convert the elements of a list to ASCII characters, and then display them as regular output. Using 128 as " +
            "a character will interpret the character as '§§'. The '§§' character is used for special color codes. There are 16 colors, " +
            "and a color will be chosen based on the character after the '§§' interpreted as a hex symbol. Example: \"§a§§aHello§f\" is cyan. " +
            "A list of colors can be seen with the 'colors' command. Typing a string using quotes (\") will interpret that string as a list, " +
            "which can be combined with other lists to form strings.\n\tExample: §7display union(\"Hello \", \"there\") => Hello there§f\n" +
            "\n\n§b== Other Help/Examples ==\n§f" +
            "This section contains additional examples of uses of the tool.\nCalculating a derivative of x^2 at x=11: Use the " +
            "diff function\n\t§7diff([x:pow(x,2)],11) => 22§f\nCalculating the integral of e^-x from 1 to 10: Use the int " +
            "function\n\t§7int([x:exp(-x)],1,10) => 0.36783403740358456\nSolving for where x^2-5x+4 = 0, with an initial guess of " +
            "x=0.5: Use the solves function, which finds where an expression is equal to 0 given an intial guess for the variable\n\t" +
            "§7solves([x:pow(x,2)-5*x+4],0.5) => 0.999999479600333\n§fCreate a list that increments each value of an input list by 1: " +
            "Use the foreach function\n\t§7foreach({1,2,3},[x:x+1]) => {2,3,4}\nDisplay \"Hello\" in a psuedo-random color: Use " +
            "the MILLIS variable, mod operator, and toascii\n\t§7display union({128,toascii(MILLIS%16)},\"Hello\")\n§f" +
            "Defining a vector-valued function, r(t), and defining a function dr(t) to be its derivative: Use vectors and the diff function" +
            ", which can take in and output vectors\n\t§7function r t <sin(t),cos(t),t>\n\tfunction dr t diff([x:r(x)],t)" +
            "\n\tdr(0) => <1,0,1>§f";
        const string COLORS_STRING = "§f  == Colors ==\n\t" +
           "§§0 Black (§0===§f)\n\t§§1 Dark Blue (§1===§f)\n\t§§2 Dark Green (§2===§f)\n\t§§3 Dark Cyan (§3===§f)\n\t" +
           "§§4 Dark Red (§4===§f)\n\t§§5 Dark Magenta (§5===§f)\n\t§§6 Dark Yellow (§6===§f)\n\t§§7 Gray (§7===§f)\n\t" +
           "§§8 Dark Gray (§8===§f)\n\t§§9 Blue (§9===§f)\n\t§§A Green (§a===§f)\n\t§§B Cyan (§b===§f)\n\t§§C Red (§c===§f)\n\t" +
           "§§D Magenta (§d===§f)\n\t§§E Yellow (§e===§f)\n\t§§F White (§f===§f)";

        static int Main(string[] args)
        {
            bool infiniteMode = false;
            if (args.Length <= 0)
            {
                infiniteMode = true;
            }

            Function.Init(EvaluateFunctionExpression, GetAst, EvaluateAstAndSetVars);
            MList.Init(EvaluateFunctionExpression);
            LoadVariables();
            LoadCustomFunctions();

            if (infiniteMode)
            {
                do
                {
                    Console.Write("> ");
                    string line = Console.ReadLine();
                    List<string> argList = new List<string>() { "" };
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == ' ')
                        {
                            argList.Add("");
                        }
                        else if (line[i] == '"')
                        {
                            int end = line.IndexOf('"', i + 1);
                            if (end < 0)
                            {
                                break;
                            }
                            argList[argList.Count - 1] += line.Substring(i, end - i + 1);
                            i = end;
                        }
                        else
                        {
                            argList[argList.Count - 1] += line[i];
                        }
                    }
                    RunCmd(argList.ToArray());
                }
                while (true);
            }
            else
            {
                return RunCmd(args);
            }
        }

        static int RunCmd(string[] args)
        {
            parseCallsInFrame = 0;
            if (args[0] == "help")
            {
                OutputMessage(HELP_STRING);
            }
            else if (args[0] == "colors")
            {
                OutputMessage(COLORS_STRING);
            }
            else if (args[0] == "vars")
            {
                foreach (var entry in variables)
                {
                    OutputMessage(entry.Key + ": " + entry.Value);
                }
            }
            else if (args[0] == "consts")
            {
                foreach (var entry in CONSTANTS)
                {
                    OutputMessage(entry.Key + ": " + entry.Value);
                }
                OutputMessage(TIME_CONSTANT + ": " + GetMillis());
            }
            else if (args[0] == "display")
            {
                MathDataValue value = new MathDataValue();
                try
                {
                    value = EvaluateExpression(args[1]);
                }
                catch (MathCmdException ex)
                {
                    OutputError(ex.Message);
                    return -1;
                }
                catch (Exception ex)
                {
                    OutputError("Invalid Expression");
                    return -1;
                }
                string msg = "";
                if (value.Type == MathDataTypes.List)
                {
                    for (int i = 0; i < value.ListValue.Elements.Count; i++)
                    {
                        if (value.ListValue.Elements[i].NumberValue == 128)
                        {
                            msg += '§';
                        }
                        else
                        {
                            msg += (char)value.ListValue.Elements[i].NumberValue;
                        }
                    }
                    OutputMessage(msg);
                }
                else
                {
                    Console.WriteLine((char)value.NumberValue);
                }
            }
            else if (args[0].Contains("="))
            {
                #region Assign Var
                // Variable assignment
                string varName = args[0].Substring(0, args[0].IndexOf("="));
                if (Array.Exists(varName.ToCharArray(), x => !ALLOWED_VAR_CHARS.Contains(x)))
                {
                    // Illegal variable name; bad characters
                    OutputError("Invalid variable name: Variables can only contain alphabetical characters and underscores");
                    return -1;
                }
                if (IsReservedValue(varName))
                {
                    // Illegal variable name; used by a constant
                    OutputError("Invalid variable name: \"" + varName + "\" is a reserved name for a constant");
                    return -1;
                }
                string expression = args[0].Substring(args[0].IndexOf("=") + 1);
                //EvaluateAndAssignVariable(varName, expression);
                Ast ast = GetAst(expression);
                MathDataValue varVal;
                try
                {
                    varVal = EvaluateAst(ast);
                }
                catch (MathCmdException ex)
                {
                    OutputError(ex.Message);
                    return -1;
                }
                AssignVariable(varName, varVal);

                OutputMessage("Set \"" + varName + "\" to " + varVal + ".");
                #endregion
            }
            else if (args[0] == "delvar")
            {
                #region Del Var
                // Variable deletion
                string varName = args[1];
                if (varName == "~")
                {
                    // Delete all variables
                    File.Delete(varsPath);
                    OutputMessage("Deleted all variables");
                    variables.Clear();
                    return 0;
                }
                if (!variables.ContainsKey(varName))
                {
                    OutputError("The variable \"" + varName + "\" does not exist!");
                    return -1;
                }

                string[] lines = File.ReadAllLines(varsPath);
                using (StreamWriter writer = new StreamWriter(varsPath))
                {
                    foreach (string line in lines)
                    {
                        if (line.Substring(0, line.IndexOf(" ")) != varName)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                variables.Remove(varName);

                OutputMessage("Deleted \"" + varName + "\".");
                #endregion
            }
            else if (args[0] == "delf")
            {
                #region Del Func
                // Function deletion
                string funcName = args[1];
                if (funcName == "~")
                {
                    // Delete all functions
                    File.Delete(customFuncsPath);
                    OutputMessage("Deleted all functions");
                    customFunctions.Clear();
                    return 0;
                }
                if (!customFunctions.ContainsKey(funcName))
                {
                    OutputError("The function \"" + funcName + "\" does not exist!");
                    return -1;
                }

                string[] lines = File.ReadAllLines(customFuncsPath);
                using (StreamWriter writer = new StreamWriter(customFuncsPath))
                {
                    foreach (string line in lines)
                    {
                        if (line.Substring(0, line.IndexOf(" ")) != funcName)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                customFunctions.Remove(funcName);

                OutputMessage("Deleted \"" + funcName + "\".");
                #endregion
            }
            else if (args[0] == "functions")
            {
                #region List Functions
                StringBuilder fmBuilder = new StringBuilder();
                fmBuilder.AppendLine("Pre-Defined Functions");
                foreach (Function function in Function.GetAllFunctions())
                {
                    string line = "\t" + function.ToString(true, false);
                    if (function.Description != null)
                    {
                        line += ": " + function.Description;
                    }
                    fmBuilder.AppendLine(line);
                }
                fmBuilder.AppendLine("User-Defined Functions");
                foreach (KeyValuePair<string, Function> kv in customFunctions)
                {
                    fmBuilder.AppendLine("\t" + kv.Value.ToString(true, true) + ": §7" + kv.Value.Description + "§f");
                }
                OutputMessage(fmBuilder.ToString());
                #endregion
            }
            else if (args[0] == "function")
            {
                #region Def Function
                if (args.Length != 4)
                {
                    OutputError("Correct usage: function (function name) (comma-separated arguments) (expression)");
                    return -1;
                }

                string funcName = args[1];
                string argString = args[2];
                string expression = args[3];
                if (!Regex.IsMatch(funcName, @"^[a-zA-Z]+$"))
                {
                    OutputError("You can only use alphabetical characters in function names.");
                    return -1;
                }
                else if (IsReservedValue(funcName))
                {
                    OutputError("Cannot use " + funcName + " as a function name because " +
                                        "it is already a reserved value.");
                    return -1;
                }
                string[] argNames = argString.Split(',');
                foreach (string argName in argNames)
                {
                    if (IsReservedValue(argName))
                    {
                        OutputError("Cannot use " + argName + " as a function argument because " +
                                        "it is already a reserved value.");
                        return -1;
                    }
                }
                CreateFunction(funcName, argString, expression, true);
                OutputMessage("Defined function " + funcName + "(" + argString + ")=" + expression);
                #endregion
            }
            else
            {
                // Simply evaluate the expression
                #region Evaluate Expr
                try
                {
                    MathDataValue result = EvaluateExpression(args[0]);
                    OutputMessage(result.ToString());
                    AssignVariable(PREVIOUS_VALUE_VAR_NAME, result);
                }
                catch (MathCmdException ex)
                {
                    OutputError(ex.Message);
                    return -1;
                }
                catch (Exception ex)
                {
                    OutputError("Invalid Expression");
                    return -1;
                }
                #endregion
            }

            return 0;
        }

        static void CreateFunction(string name, string args, string expression, bool addToFile)
        {
            List<FunctionParameter> parameters = new List<FunctionParameter>();
            string[] argNames = args.Split(',');
            for (int i = 0; i < argNames.Length; i++)
            {
                if (argNames[i].Contains("("))
                {
                    int openParenIndex = argNames[i].IndexOf("(");
                    int closeParenIndex = argNames[i].IndexOf(")");
                    string argName = argNames[i].Substring(0, openParenIndex);
                    int numArgs = int.Parse(argNames[i].Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1));
                    parameters.Add(new FunctionParameter(argName, numArgs));
                }
                else
                {
                    parameters.Add(new FunctionParameter(argNames[i], FunctionParameterTypes.Number));
                }
            }

            Function newFunc = new Function(parameters, new string[] { name },
                    x => EvaluateCustomFunction(args.Split(','), expression, x), expression);
            if (customFunctions.Where(kv => kv.Key == name).Count() > 0)
            {
                // Function exists
                customFunctions[name] = newFunc;
                if (addToFile)
                {
                    string[] lines = File.ReadAllLines(customFuncsPath);
                    using (StreamWriter writer = new StreamWriter(customFuncsPath))
                    {
                        foreach (string line in lines)
                        {
                            if (line.Substring(0, line.IndexOf(" ")) == name)
                            {
                                writer.WriteLine(name + " " + args + " " + expression);
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                }
            }
            else
            {
                customFunctions.Add(name, newFunc);
                if (addToFile)
                {
                    File.AppendAllText(customFuncsPath, name + " " + args + " " + expression + Environment.NewLine);
                }
            }
        }

        static void AssignVariable(string variable, MathDataValue value)
        {
            if (variables.ContainsKey(variable))
            {
                variables[variable] = value;
                string[] lines = File.ReadAllLines(varsPath);
                using (StreamWriter writer = new StreamWriter(varsPath))
                {
                    foreach (string line in lines)
                    {
                        if (line.Substring(0, line.IndexOf(" ")) == variable)
                        {
                            writer.WriteLine(variable + " " + value.ToString());
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            else
            {
                variables.Add(variable, value);
                File.AppendAllText(varsPath, variable + " " + value + Environment.NewLine);
            }
        }
        static void AssignVariableInDict(string variable, MathDataValue value)
        {
            if (variables.ContainsKey(variable))
            {
                variables[variable] = value;
            }
            else
            {
                variables.Add(variable, value);
            }
        }

        static Ast GetAst(string expression)
        {
            // Loads variables and performs math on numbers; beefiest part of the program
            List<string> tokens = new List<string>(Regex.Split(expression, EXPRESSION_REGEX));
            tokens.RemoveAll(x => x.Length <= 0);

            // Need to generate more of a tree using these tokens
            Ast ast = ExpressionParser.Parse(tokens);

            return ast;
        }
        static MathDataValue EvaluateExpression(string expression)
        {
            Ast ast = GetAst(expression);
            return EvaluateAst(ast);
        }
        static MathDataValue EvaluateAst(Ast originalAst)
        {
            parseCallsInFrame++;
            if (parseCallsInFrame > 512)
            {
                throw new MStackOverflowException();
            }
            Ast ast = originalAst.Clone();
            double value = 0;
            MList setValue = new MList();
            MVector vectorValue = new MVector();
            MathDataTypes type = MathDataTypes.Number;
            switch (ast.Type)
            {
                #region Number
                case AstTypes.Number:

                    type = MathDataTypes.Number;
                    value = ast.Value;
                    break;
                #endregion
                #region Variable
                case AstTypes.Variable:
                    if (variables.ContainsKey(ast.VarName))
                    {
                        //type = MathDataTypes.Number;
                        //value = variables[ast.VarName].NumberValue;
                        return variables[ast.VarName];
                    }
                    else if (CONSTANTS.ContainsKey(ast.VarName))
                    {
                        type = MathDataTypes.Number;
                        value = CONSTANTS[ast.VarName];
                    }
                    else if (ast.VarName == TIME_CONSTANT)
                    {
                        type = MathDataTypes.Number;
                        value = GetMillis();
                    }
                    else
                    {
                        throw new InvalidExpressionException("\"" + ast.VarName + "\" is not a valid variable.");
                    }
                    break;
                #endregion
                #region Set Literal
                case AstTypes.ListLiteral:
                    type = MathDataTypes.List;
                    setValue = new MList(ast.Contents.Select(x => EvaluateAst(x)).ToList());
                    break;
                #endregion
                #region Vector Literal
                case AstTypes.VectorLiteral:
                    type = MathDataTypes.Vector;
                    vectorValue = new MVector(ast.Contents.Select(x => EvaluateAst(x).NumberValue).ToList());
                    break;
                #endregion
                #region Binary Operation
                case AstTypes.BinaryOperation:
                    MathDataValue evLeft = EvaluateAst(ast.Left);
                    MathDataValue evRight = EvaluateAst(ast.Right);
                    double left = evLeft.NumberValue;
                    double right = evRight.NumberValue;
                    MVector leftV = evLeft.VectorValue;
                    MVector rightV = evRight.VectorValue;
                    type = MathDataTypes.Number;
                    switch (ast.Operation)
                    {
                        case BinaryOperations.Addition:
                            if (evLeft.Type == MathDataTypes.Vector && evRight.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = leftV + rightV;
                            }
                            value = left + right;
                            break;
                        case BinaryOperations.Subtraction:
                            if (evLeft.Type == MathDataTypes.Vector && evRight.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = leftV - rightV;
                            }
                            value = left - right;
                            break;
                        case BinaryOperations.Multiplication:
                            if (evLeft.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = MVector.Scale(leftV, right);
                            }
                            else if (evRight.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = MVector.Scale(rightV, left);
                            }
                            value = left * right;
                            break;
                        case BinaryOperations.Division:
                            if (evLeft.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = MVector.Scale(leftV, 1 / right);
                            }
                            else if (evRight.Type == MathDataTypes.Vector)
                            {
                                type = MathDataTypes.Vector;
                                vectorValue = MVector.Scale(rightV, 1 / left);
                            }
                            value = left / right;
                            break;
                        case BinaryOperations.Modulus:
                            value = left % right;
                            break;
                        //case BinaryOperations.Exponent:
                        //    return Math.Pow(left, right);
                        default:
                            throw new InvalidExpressionException("Invalid operation \"" + ast.Operation + "\".");
                    }
                    break;
                #endregion
                #region Function
                case AstTypes.Function:
                    //List<double> args = new List<double>();
                    //foreach (Ast arg in ast.Arguments)
                    //{
                    //    args.Add(EvaluateAst(arg));
                    //}
                    Function function = GetFunction(ast.FunctionName);

                    // Verify that arguments are of the right type
                    if (ast.Arguments.Count != function.Parameters.Count)
                    {
                        throw new InvalidArgumentsException("\"" + function.Tokens[0] + "\" requires " +
                            function.Parameters.Count + " argument(s).");
                    }
                    for (int i = 0; i < ast.Arguments.Count; i++)
                    {
                        FunctionParameterTypes ptype = function.Parameters[i].Type;
                        bool invalidArg = false;
                        switch (ptype)
                        {
                            case FunctionParameterTypes.Delegate:
                                invalidArg = (ast.Arguments[i].Type != AstTypes.Delegate);
                                if (function.Parameters[i].NumDelegateArgs != ast.Arguments[i].VarNames.Length)
                                {
                                    throw new InvalidArgumentsException("Argument " + (i + 1) + " expected " +
                                        function.Parameters[i].NumDelegateArgs + " arguments in delegate but found " +
                                        ast.Arguments[i].VarNames.Length);
                                }
                                break;
                            case FunctionParameterTypes.List:
                            case FunctionParameterTypes.Vector:
                            case FunctionParameterTypes.Number:
                                invalidArg = (ast.Arguments[i].Type == AstTypes.Delegate);
                                break;
                        }

                        if (invalidArg)
                        {
                            throw new InvalidArgumentsException("Expected arg " + (i + 1) +
                                " to be " + ptype.ToString() + " but found " + ast.Arguments[i].Type.ToString() + ".");
                        }

                        // Convert non-delegate ASTs to numbers/sets
                        if (ast.Arguments[i].Type == AstTypes.Delegate)
                        {
                            // Make sure variable names are valid
                            foreach (string varName in ast.Arguments[i].VarNames)
                            {
                                if (IsReservedValue(varName))
                                {
                                    throw new InvalidArgumentsException("Cannot use " + varName + " as a delegate argument because " +
                                        "it is already a reserved value.");
                                }
                            }
                        }
                        else
                        {
                            MathDataValue result = EvaluateAst(ast.Arguments[i]);
                            if (result.Type == MathDataTypes.Number)
                            {
                                ast.Arguments[i] = Ast.CreateNumberAst(result.NumberValue);
                            }
                            else if (result.Type == MathDataTypes.Vector)
                            {
                                ast.Arguments[i] = Ast.CreateVectorAst(result.VectorValue.ToAstArray());
                            }
                            else
                            {
                                ast.Arguments[i] = Ast.CreateListAst(result.ListValue.ToAstArray());
                            }
                        }
                    }

                    return function.Evaluate(ast.Arguments.ToArray());
                    #endregion
            }

            return new MathDataValue(type, value, setValue, vectorValue);
        }

        static double GetMillis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        static Function GetFunction(string name)
        {
            Function function = Function.GetFunction(name);
            if (function == null)
            {
                if (customFunctions.ContainsKey(name))
                {
                    return customFunctions[name];
                }
                else
                {
                    throw new InvalidExpressionException("\"" + name + "\" is not a pre-defined " +
                        "or user-defined function.");
                }
            }
            return function;
        }

        //static double Integrate(string expression, string varName, string lowerExp, string upperExp)
        //{
        //    double sum = 0;

        //    double lower = EvaluateExpression(lowerExp);
        //    double upper = EvaluateExpression(upperExp);
        //    double deltaVar = (upper - lower) / INTEGRAL_INTERVALS;
        //    double intVarVal = lower + deltaVar;
        //    if (!variables.ContainsKey(varName))
        //    {
        //        variables.Add(varName, intVarVal);
        //    }
        //    // We are going to do a trapezoid sum here. Formula is
        //    // dx/2 * (f(x0) + 2f(x1) + 2f(x2) + 2f(x3) + ... + f(xn))
        //    // We will sum starting from f(x1) to f(x n-1), then complete the formula at the end
        //    else
        //    {
        //        variables[varName] = intVarVal;
        //    }
        //    for (int i = 0; i < INTEGRAL_INTERVALS - 2; i++)
        //    {
        //        // Evaluate at this value of the integral variable
        //        double value = EvaluateExpression(expression);
        //        // Multiply by the "dx" and add to the sum
        //        sum += 2 * value;

        //        // Increment the delta variable (ex the "dx")
        //        intVarVal += deltaVar;
        //        variables[varName] = intVarVal;
        //    }

        //    variables[varName] = lower;
        //    double fx0 = EvaluateExpression(expression);
        //    variables[varName] = upper;
        //    double fxn = EvaluateExpression(expression);
        //    return deltaVar / 2 * (fx0 + sum + fxn);
        //}
        //static double TakeLimit(string expression, string limVar, string limVarValExpression)
        //{
        //    double limVarVal = EvaluateExpression(limVarValExpression);
        //    if (variables.ContainsKey(limVar))
        //    {
        //        variables[limVar] = limVarVal;
        //    }
        //    else
        //    {
        //        variables.Add(limVar, limVarVal);
        //    }
        //    // First try to just plug the variable in
        //    double result = EvaluateExpression(expression);
        //    if (!double.IsNaN(result) && !double.IsInfinity(result))
        //    {
        //        return result;
        //    }
        //    // Evaluate the limit with approximation
        //    variables[limVar] = limVarVal - LIMIT_DELTA_VAR;
        //    double oneLower = EvaluateExpression(expression);
        //    variables[limVar] = limVarVal + LIMIT_DELTA_VAR;
        //    double oneHigher = EvaluateExpression(expression);
        //    if (Math.Abs(oneLower - oneHigher) <= MIN_LIM_DIFF)
        //    {
        //        // Limit converges
        //        double limit = (oneLower + oneHigher) / 2;
        //        if (limit >= 1 / Math.Pow(LIMIT_DELTA_VAR, 2) - 100)
        //        {
        //            // limit is within 100 of 1/dv^2, so very big number
        //            return double.PositiveInfinity;
        //        }
        //        else if (limit <= -1 / Math.Pow(LIMIT_DELTA_VAR, 2) + 100)
        //        {
        //            // limit is within 100 of -1/dv^2, so very small (a "big negative") number
        //            return double.NegativeInfinity;
        //        }
        //        return limit;
        //    }
        //    else
        //    {
        //        // Limit diverges
        //        return double.NaN;
        //    }
        //}
        //static double Differentiate(string expression, string diffVar, string diffVarValExpression)
        //{
        //    double diffVarVal = EvaluateExpression(diffVarValExpression);
        //    if (variables.ContainsKey(diffVar))
        //    {
        //        variables[diffVar] = diffVarVal;
        //    }
        //    else
        //    {
        //        variables.Add(diffVar, diffVarVal);
        //    }
        //    // Evaluate the derivative with approximation
        //    double diffSum = 0;
        //    for (int i = 0; i < DERIVATIVE_DELTA_VARS.Length; i++)
        //    {
        //        variables[diffVar] = diffVarVal;
        //        double orig = EvaluateExpression(expression);
        //        variables[diffVar] = diffVarVal + DERIVATIVE_DELTA_VARS[i];
        //        double changed = EvaluateExpression(expression);
        //        diffSum += (changed - orig) / DERIVATIVE_DELTA_VARS[i];
        //    }
        //    return Math.Round(diffSum / DERIVATIVE_DELTA_VARS.Length, 5);
        //}
        //static double Solve(string expression, string sVar, double startVal)
        //{
        //    if (variables.ContainsKey(sVar))
        //    {
        //        variables[sVar] = startVal;
        //    }
        //    else
        //    {
        //        variables.Add(sVar, startVal);
        //    }

        //    double expVal = EvaluateExpression(expression);
        //    double varVal = startVal;
        //    int iterations = 0;
        //    // Do Newton's method
        //    while (Math.Abs(expVal) > MIN_NEWTON_METHOD_DIFF && iterations < MAX_NEWTON_METHOD_ITERATIONS)
        //    {
        //        double derivative = 0;// Differentiate(expression, sVar, varVal.ToString());
        //        if (derivative == 0)
        //        {
        //            // This guess won't work, so we're going to try and move the variable value and then restart
        //            varVal += 1;
        //        }
        //        else
        //        {
        //            varVal = varVal - expVal / derivative;
        //        }
        //        variables[sVar] = varVal;
        //        expVal = EvaluateExpression(expression);
        //        iterations++;
        //    }
        //    if (iterations >= MAX_NEWTON_METHOD_ITERATIONS)
        //    {
        //        return double.NaN;
        //    }
        //    return varVal;
        //}

        static void LoadVariables()
        {
            StreamReader reader;
            if (!File.Exists(varsPath))
            {
                reader = new StreamReader(File.Create(varsPath));
            }
            else
            {
                reader = new StreamReader(File.OpenRead(varsPath));
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    string[] tokens = line.Split(' ');
                    string varName = tokens[0];
                    variables.Add(varName, EvaluateExpression(tokens[1]));
                    //if (tokens[1].StartsWith(ExpressionParser.OPEN_SET_BRACKET))
                    //{

                    //}
                    //else
                    //{
                    //    variables.Add(varName, new MathDataValue(double.Parse(tokens[1])));
                    //}
                }
                catch (Exception e)
                {
                    OutputError("An error occurred while loading variables: " + e.Message + "\n\n" + e.StackTrace);
                    continue;
                }
            }
            reader.Close();
        }

        static void LoadCustomFunctions()
        {
            StreamReader reader;
            if (!File.Exists(customFuncsPath))
            {
                reader = new StreamReader(File.Create(customFuncsPath));
            }
            else
            {
                reader = new StreamReader(File.OpenRead(customFuncsPath));
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    string[] tokens = line.Split(' ');
                    string funcName = tokens[0];
                    string args = tokens[1];
                    string expression = tokens[2];

                    CreateFunction(funcName, args, expression, false);
                    //customFunctions.Add(funcName, new Function(Functions._Custom, args.Length, new string[] { funcName },
                    //    x => EvaluateCustomFunction(args, expression, x)));
                }
                catch (Exception e)
                {
                    OutputError("An error occurred while loading functions: " + e.Message + "\n\n" + e.StackTrace);
                    continue;
                }
            }
            reader.Close();
        }

        static MathDataValue EvaluateCustomFunction(string[] funcVarArgs, string funcExpression, Ast[] actualArgs)
        {
            // Store the current custom functions and variables dicts, so we can restore them later
            Dictionary<string, Function> prevFunctions = customFunctions.ToDictionary(entry => entry.Key, entry => entry.Value);
            Dictionary<string, MathDataValue> prevVariables = variables.ToDictionary(entry => entry.Key,
                entry => (MathDataValue)entry.Value.Clone());

            for (int i = 0; i < funcVarArgs.Length; i++)
            {
                if (actualArgs[i].Type == AstTypes.Number)
                {
                    AssignVariableInDict(funcVarArgs[i], new MathDataValue(actualArgs[i].Value));
                }
                else if (actualArgs[i].Type == AstTypes.ListLiteral)
                {
                    AssignVariableInDict(funcVarArgs[i], new MathDataValue(
                        new MList(actualArgs[i].Contents.Select(x => EvaluateAst(x)).ToArray())));
                }
                else if (actualArgs[i].Type == AstTypes.Delegate)
                {
                    List<FunctionParameter> parameters = new List<FunctionParameter>();
                    string[] strArgs = new string[actualArgs[i].VarNames.Length];
                    for (int j = 0; j < actualArgs[i].VarNames.Length; j++)
                    {
                        parameters.Add(new FunctionParameter(actualArgs[i].VarNames[j], FunctionParameterTypes.Number));
                        strArgs[j] = actualArgs[i].VarNames[j];
                    }
                    string delegateName = funcVarArgs[i].Substring(0, funcVarArgs[i].IndexOf('('));
                    string expression = actualArgs[i].Expression;
                    Function newFunction = new Function(parameters, new string[] { delegateName },
                        x => EvaluateCustomFunction(strArgs, expression, x));
                    if (customFunctions.ContainsKey(delegateName))
                    {
                        customFunctions[delegateName] = newFunction;
                    }
                    else
                    {
                        customFunctions.Add(delegateName, newFunction);
                    }
                }
            }
            MathDataValue result = EvaluateExpression(funcExpression);
            customFunctions = prevFunctions;
            variables = prevVariables;
            return result;
        }
        static MathDataValue EvaluateFunctionExpression(string expression, Dictionary<string, MathDataValue> varAssignments)
        {
            foreach (KeyValuePair<string, MathDataValue> kv in varAssignments)
            {
                AssignVariableInDict(kv.Key, kv.Value);
            }
            return EvaluateExpression(expression);
        }
        static MathDataValue EvaluateAstAndSetVars(Ast ast, Dictionary<string, MathDataValue> varAssignments)
        {
            foreach (KeyValuePair<string, MathDataValue> kv in varAssignments)
            {
                AssignVariableInDict(kv.Key, kv.Value);
            }
            return EvaluateAst(ast);
        }

        static void OutputError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("\t" + error);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        static void OutputMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\t");
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == '§')
                {
                    if (i + 1 < msg.Length)
                    {
                        if (msg[i + 1] == '§')
                        {
                            // Have "§§", simply display a single "§"
                            Console.Write("§");
                        }
                        else
                        {
                            ConsoleColor next = ConsoleColor.White;
                            try
                            {
                                int hexVal = int.Parse(msg[i + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                                next = (ConsoleColor)hexVal;
                            }
                            catch (Exception ex)
                            {
                                // Ignore this exception, since we will simply set the color to white in the case of illegal input
                            }
                            Console.ForegroundColor = next;
                        }
                        i++;
                    }
                }
                else
                {
                    Console.Write(msg[i]);
                }
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static bool IsReservedValue(string name)
        {
            return (CONSTANTS.ContainsKey(name) || name == TIME_CONSTANT);
        }
    }
}
