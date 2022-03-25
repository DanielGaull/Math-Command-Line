using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmdTool
{
    public struct MList
    {
        public List<MathDataValue> Elements;

        private static ExpressionEvaluation exprEvaluator;

        public MList(List<MathDataValue> elements)
        {
            Elements = elements;
        }
        public MList(Ast ast)
        {
            if (ast.Type == AstTypes.ListLiteral || ast.Type == AstTypes.VectorLiteral)
            {
                Elements = new List<MathDataValue>(ast.Contents.Select(x => new MathDataValue(x)).ToArray());
            }
            else
            {
                Elements = new List<MathDataValue>() { new MathDataValue(ast.Value) };
            }
        }
        public MList(Ast[] elements)
        {
            this.Elements = new List<MathDataValue>(elements.Select(x => new MathDataValue(x)).ToArray());
        }
        public MList(params MathDataValue[] elements)
            : this(new List<MathDataValue>(elements))
        {
        }

        public List<MathDataValue> CopyValues()
        {
            List<MathDataValue> copy = new List<MathDataValue>();
            foreach (MathDataValue d in Elements)
            {
                copy.Add((MathDataValue)d.Clone());
            }
            return copy;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            for (int i = 0; i < Elements.Count; i++)
            {
                sb.Append(Elements[i].ToString());
                if (i + 1 < Elements.Count)
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        public static void Init(ExpressionEvaluation exprEvaluator)
        {
            MList.exprEvaluator = exprEvaluator;
        }

        public static MList Add(MList s, MathDataValue value)
        {
            List<MathDataValue> values = s.CopyValues();
            values.Add(value);
            return new MList(values);
        }
        public static MathDataValue Get(MList s, int index)
        {
            if (index >= s.Elements.Count)
            {
                throw new SetIndexOutOfBoundsException(s, index);
            }
            return s.Elements[index];
        }
        public static MList Union(MList s1, MList s2)
        {
            List<MathDataValue> values = s1.CopyValues();
            values.AddRange(s2.CopyValues());
            return new MList(values);
        }
        public static MList Intersect(MList s1, MList s2)
        {
            List<MathDataValue> values = new List<MathDataValue>();
            for (int i = 0; i < s1.Elements.Count; i++)
            {
                if (s2.Elements.Contains(s1.Elements[i]))
                {
                    values.Add(s1.Elements[i]);
                }
            }
            return new MList(values);
        }
        public static MList Less(MList s1, MList s2)
        {
            List<MathDataValue> values = new List<MathDataValue>();
            for (int i = 0; i < s1.Elements.Count; i++)
            {
                if (!s2.Elements.Contains(s1.Elements[i]))
                {
                    values.Add(s1.Elements[i]);
                }
            }
            return new MList(values);
        }
        public static MList Insert(MList s, int index, MathDataValue element)
        {
            List<MathDataValue> values = s.CopyValues();
            if (index >= values.Count)
            {
                throw new SetIndexOutOfBoundsException(s, index);
            }
            values.Insert(index, element);
            return new MList(values);
        }
        public static MList Remove(MList s, int index)
        {
            List<MathDataValue> values = s.CopyValues();
            if (index >= values.Count)
            {
                throw new SetIndexOutOfBoundsException(s, index);
            }
            values.RemoveAt(index);
            return new MList(values);
        }
        public static int Length(MList s)
        {
            return s.Elements.Count;
        }
        public static int IndexOf(MList s, MathDataValue value)
        {
            return s.Elements.IndexOf(value);
        }
        public static MList ForEach(MList s, Ast del)
        {
            List<MathDataValue> elems = s.CopyValues();
            Dictionary<string, MathDataValue> varVals = new Dictionary<string, MathDataValue>();
            varVals.Add(del.VarNames[0], new MathDataValue(0.0));
            for (int i = 0; i < elems.Count; i++)
            {
                varVals[del.VarNames[0]] = elems[i];
                elems[i] = exprEvaluator(del.Expression, varVals);
            }
            return new MList(elems);
        }
        public static MList Join(MList list)
        {
            List<MathDataValue> elems = list.CopyValues();
            List<MathDataValue> newElems = new List<MathDataValue>();
            foreach (MathDataValue value in elems)
            {
                newElems.AddRange(value.ListValue.CopyValues());
            }
            return new MList(newElems);
        }
        public Ast[] ToAstArray()
        {
            Ast[] arr = new Ast[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
            {
                switch (Elements[i].Type)
                {
                    case MathDataTypes.Number:
                        arr[i] = Ast.CreateNumberAst(Elements[i].NumberValue);
                        break;
                    case MathDataTypes.List:
                        arr[i] = Ast.CreateListAst(Elements[i].ListValue.ToAstArray());
                        break;
                    case MathDataTypes.Vector:
                        arr[i] = Ast.CreateVectorAst(Elements[i].VectorValue.ToAstArray());
                        break;
                }
            }
            return arr;
        }

        public static bool operator ==(MList l1, MList l2) => l1.Equals(l2);
        public static bool operator !=(MList l1, MList l2) => !l1.Equals(l2);

        public override bool Equals(object obj)
        {
            if (!(obj is MList))
            {
                return false;
            }
            MList l = (MList)(obj as MList?);
            if (l.Elements.Count != Elements.Count)
            {
                return false;
            }
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i] != l.Elements[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
