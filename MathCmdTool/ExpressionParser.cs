using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCmdTool
{
    static class ExpressionParser
    {
        public const string ADDITION_OP = "+";
        public const string SUBTRACTION_OP = "-";
        public const string MULTIPLICATION_OP = "*";
        public const string DIVISION_OP = "/";
        public const string MOD_OP = "%";
        public const string POW_OP = "^";
        static readonly string[] BINARY_OPERATORS =
        {
            ADDITION_OP, SUBTRACTION_OP, MULTIPLICATION_OP, DIVISION_OP, MOD_OP, POW_OP
        };

        public const string DELEGATE_VAR_SPLITTER = ",";
        public const string DELEGATE_EXPR_STARTER = ":";

        public const string OPEN_SET_BRACKET = "{";
        public const string CLOSE_SET_BRACKET = "}";
        public const string SET_ELEM_SPLITTER_TOKEN = ",";

        public const string OPEN_VECTOR_BRACKET = "<";
        public const string CLOSE_VECTOR_BRACKET = ">";
        public const string VECTOR_ELEM_SPLITTER_TOKEN = ",";

        public const string OPEN_PAREN = "(";
        public const string CLOSE_PAREN = ")";
        public const string OPEN_BRACKET = "[";
        public const string CLOSE_BRACKET = "]";
        public const string STR_WRAPPER = "\"";
        public const string ARG_SPLITTER_TOKEN = ",";
        public const string NEGATIVE_TOKEN = "-";

        static readonly Dictionary<BinaryOperations, int> OPERATOR_PRECEDENCE = new Dictionary<BinaryOperations, int>()
        {
            { BinaryOperations.Addition, 0 },
            { BinaryOperations.Subtraction, 0 },
            { BinaryOperations.Multiplication, 1 },
            { BinaryOperations.Division, 1 },
            { BinaryOperations.Modulus, 1 }
        };

        public static Ast Parse(List<string> tokens)
        {
            if (tokens.Count == 0)
            {
                return null;
            }

            Ast ast = null;

            if (tokens.First() == OPEN_PAREN && GetEndBracketIndex(tokens, 0, OPEN_PAREN, CLOSE_PAREN) == tokens.Count - 1)
            {
                tokens = tokens.GetRange(1, tokens.Count - 2);
            }

            if (tokens.Count == 1)
            {
                // One token, so either a variable or a number
                if (double.TryParse(tokens.First(), out double value))
                {
                    ast = Ast.CreateNumberAst(value);
                }
                else
                {
                    ast = Ast.CreateVariableAst(tokens.First());
                }
            }
            else if (tokens.First() == OPEN_SET_BRACKET &&
                GetEndBracketIndex(tokens, 0, OPEN_SET_BRACKET, CLOSE_SET_BRACKET) == tokens.Count - 1)
            {
                List<List<string>> elems = SplitTokenArguments(tokens.GetRange(1,
                    GetEndBracketIndex(tokens, 0, OPEN_SET_BRACKET, CLOSE_SET_BRACKET) - 1), SET_ELEM_SPLITTER_TOKEN);
                elems.RemoveAll(x => x.Count == 0);
                Ast[] elements = elems.Select(x => Parse(x)).ToArray();
                ast = Ast.CreateListAst(elements);
            }
            else if (tokens.First() == OPEN_VECTOR_BRACKET &&
                GetEndBracketIndex(tokens, 0, OPEN_VECTOR_BRACKET, CLOSE_VECTOR_BRACKET) == tokens.Count - 1)
            {
                List<List<string>> elems = SplitTokenArguments(tokens.GetRange(1,
                    GetEndBracketIndex(tokens, 0, OPEN_VECTOR_BRACKET, CLOSE_VECTOR_BRACKET) - 1), VECTOR_ELEM_SPLITTER_TOKEN);
                Ast[] elements = elems.Select(x => Parse(x)).ToArray();
                ast = Ast.CreateVectorAst(elements);
            }
            else if (tokens.First() == STR_WRAPPER &&
                tokens.IndexOf(STR_WRAPPER, 1) == tokens.Count - 1)
            {
                // String literal, just immediately convert to a set AST
                string str = string.Join(null, tokens.GetRange(1, tokens.IndexOf(STR_WRAPPER, 1) - 1).ToArray());
                Ast[] values = Array.ConvertAll(str.ToCharArray(), x => Ast.CreateNumberAst(x));
                ast = Ast.CreateListAst(values);
            }
            else if (tokens.First() == NEGATIVE_TOKEN)
            {
                // Negative number
                // Simply create an expression of the value being subtracted from 0, then
                // return that
                List<string> newTokens = new List<string>()
                {
                    "(", "0", "-"
                };
                newTokens.AddRange(tokens.GetRange(1, tokens.Count - 1));
                newTokens.Add(")");
                return Parse(newTokens);
            } 
            else if (tokens.First() == OPEN_BRACKET &&
                GetEndBracketIndex(tokens, 2, OPEN_BRACKET, CLOSE_BRACKET) == tokens.Count - 1)
            {
                // Delegate
                List<string> delegateTokens = tokens.GetRange(0, GetEndBracketIndex(tokens, 2, OPEN_BRACKET, CLOSE_BRACKET) + 1);
                return ParseDelegate(delegateTokens);

                //int arrowIndex = str.IndexOf("=>");
                //string varListStr = str.Substring(0, arrowIndex - 1);
                //varsList = varListStr.Split(",");
                //expression = str.Substring(arrowIndex + 1);
            }
            /*Function.AllFunctionTokens.Contains(tokens.First()) && */
            else if (Regex.IsMatch(tokens.First(), @"^[a-zA-Z]+$") && 
                GetEndBracketIndex(tokens, 2, OPEN_PAREN, CLOSE_PAREN) == tokens.Count - 1)
            {
                List<string> funcTokens = tokens.GetRange(0, GetEndBracketIndex(tokens, 2, OPEN_PAREN, CLOSE_PAREN) + 1);
                ast = ParseFunction(funcTokens);
            }
            else
            {
                ast = ParseBinaryExpression(tokens);
            }

            return ast;
        }

        public static Ast ParseFunction(List<string> tokens)
        {
            List<Ast> arguments = new List<Ast>();
            // Split into arguments based on commas
            List<List<string>> argsTokens = SplitTokenArguments(tokens.GetRange(2, tokens.Count - 3), ARG_SPLITTER_TOKEN);
            foreach (List<string> arg in argsTokens)
            {
                arguments.Add(Parse(arg));
            }
                
            return Ast.CreateFunctionAst(tokens.First(), arguments);
        }

        private static List<List<string>> SplitTokenArguments(List<string> tokenArgs, string splitterToken)
        {
            List<List<string>> splitArgs = new List<List<string>>();
            splitArgs.Add(new List<string>());
            for (int i = 0; i < tokenArgs.Count; i++)
            {
                if (tokenArgs[i] == OPEN_PAREN || tokenArgs[i] == OPEN_BRACKET || tokenArgs[i] == OPEN_SET_BRACKET || 
                    tokenArgs[i] == OPEN_VECTOR_BRACKET)
                {
                    // Need to skip over this section so that we don't include arguments of another function in this function
                    string startToken, endToken;
                    if (tokenArgs[i] == OPEN_PAREN)
                    {
                        startToken = OPEN_PAREN;
                        endToken = CLOSE_PAREN;
                    }
                    else if (tokenArgs[i] == OPEN_SET_BRACKET)
                    {
                        startToken = OPEN_SET_BRACKET;
                        endToken = CLOSE_SET_BRACKET;
                    }
                    else if (tokenArgs[i] == OPEN_VECTOR_BRACKET)
                    {
                        startToken = OPEN_VECTOR_BRACKET;
                        endToken = CLOSE_VECTOR_BRACKET;
                    }
                    else
                    {
                        startToken = OPEN_BRACKET;
                        endToken = CLOSE_BRACKET;
                    }
                    int endIndex = GetEndBracketIndex(tokenArgs, i, startToken, endToken);
                    splitArgs.Last().AddRange(tokenArgs.GetRange(i, endIndex - i + 1));
                    i = endIndex;
                }
                else
                {
                    if (tokenArgs[i] == splitterToken)
                    {
                        // We've encountered a comma, so add a new list which will be filled with the next argument
                        splitArgs.Add(new List<string>());
                    }
                    else
                    {
                        splitArgs.Last().Add(tokenArgs[i]);
                    }
                }
            }
            return splitArgs;
        }

        public static Ast ParseBinaryExpression(List<string> tokens)
        {
            List<string> left = new List<string>();
            List<string> right = new List<string>();
            int precedence = 100;
            int opIndex = -1;

            int startI = 0;
            if (tokens.First() == OPEN_PAREN)
            {
                if (GetEndBracketIndex(tokens, 0, OPEN_PAREN, CLOSE_PAREN) == tokens.Count - 1)
                {
                    // Remove parenthesis if they're wrapping this whole expression
                    tokens = tokens.GetRange(1, GetEndBracketIndex(tokens, 0, OPEN_PAREN, CLOSE_PAREN) - 1);
                }
                else
                {
                    startI = GetEndBracketIndex(tokens, 0, OPEN_PAREN, CLOSE_PAREN) + 1;
                }
            }

            for (int i = startI; i < tokens.Count; i++)
            {
                if (tokens[i] == OPEN_PAREN)
                {
                    i = GetEndBracketIndex(tokens, i, OPEN_PAREN, CLOSE_PAREN);
                }
                else if (tokens[i] == OPEN_SET_BRACKET)
                {
                    i = GetEndBracketIndex(tokens, i, OPEN_SET_BRACKET, CLOSE_SET_BRACKET);
                }
                else if (tokens[i] == OPEN_VECTOR_BRACKET)
                {
                    i = GetEndBracketIndex(tokens, i, OPEN_VECTOR_BRACKET, CLOSE_VECTOR_BRACKET);
                }
                else if (tokens[i] == STR_WRAPPER)
                {
                    i = tokens.IndexOf(STR_WRAPPER, 1);
                }

                // If two binary operators are consecutive, ignore the second one because it could be a
                // negative sign
                if (BINARY_OPERATORS.Contains(tokens[i]) && 
                    !(i > 0 && BINARY_OPERATORS.Contains(tokens[i- 1])))
                {
                    BinaryOperations op = StringToOperation(tokens[i]);
                    if (OPERATOR_PRECEDENCE[op] <= precedence)
                    {
                        opIndex = i;
                        precedence = OPERATOR_PRECEDENCE[op];
                    }
                }
            }

            left = new List<string>(tokens.GetRange(0, opIndex));
            right = new List<string>(tokens.GetRange(opIndex + 1, tokens.Count - opIndex - 1));

            return Ast.CreateBinOpAst(Parse(left), Parse(right), StringToOperation(tokens[opIndex]));
        }

        public static bool IsBinOp(List<string> tokens)
        {
            return tokens.Exists(x => BINARY_OPERATORS.Contains(x));
        }

        private static Ast ParseDelegate(List<string> tokens)
        {
            if (tokens.First() == OPEN_BRACKET)
            {
                tokens.RemoveAt(0);
            }
            int colonIndex = tokens.IndexOf(DELEGATE_EXPR_STARTER);
            List<List<string>> varNameTokens = SplitTokenArguments(tokens.GetRange(0, colonIndex), DELEGATE_VAR_SPLITTER);
            string[] varNames = new string[varNameTokens.Count];
            for (int i = 0; i < varNameTokens.Count; i++)
            {
                varNames[i] = string.Join(null, varNameTokens[i].ToArray());
            }
            string expression = string.Join(null, tokens.GetRange(colonIndex + 1, tokens.Count - colonIndex - 2).ToArray());
            return Ast.CreateDelegateAst(varNames, expression);
        }
    
        private static int GetEndBracketIndex(List<string> tokens, int startIndex, string openBracket, string closeBracket)
        {
            int openerCount = 0;
            for (int i = startIndex + 1; i < tokens.Count; i++)
            {
                if (tokens[i] == openBracket)
                {
                    openerCount++;
                }
                else if (tokens[i] == closeBracket)
                {
                    if (openerCount > 0)
                    {
                        openerCount--;
                    }
                    else if (openerCount == 0)
                    {
                        return i;
                    }
                    else if (openerCount < 0)
                    {
                        throw new Exception("what");
                    }
                }
            }
            return -1;
            //throw new InvalidParseException(tokens.Last(), "Punctuation");
        }
        public static BinaryOperations StringToOperation(string s)
        {
            switch (s)
            {
                case ADDITION_OP:
                    return BinaryOperations.Addition;
                case SUBTRACTION_OP:
                    return BinaryOperations.Subtraction;
                case MULTIPLICATION_OP:
                    return BinaryOperations.Multiplication;
                case DIVISION_OP:
                    return BinaryOperations.Division;
                case MOD_OP:
                    return BinaryOperations.Modulus;
            }

            throw new InvalidParseException(s, "Binary Operator");
        }
    }
}
