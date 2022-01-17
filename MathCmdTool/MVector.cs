using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmdTool
{
    public struct MVector
    {
        public List<double> Components;
        public int NumComponents
        {
            get
            {
                return Components.Count;
            }
        }

        public MVector(List<double> components)
        {
            Components = components;
        }

        public MVector(Ast[] components)
        {
            Components = new List<double>(components.Select(x => x.Value).ToArray());
        }
        public MVector(Ast ast)
        {
            if (ast.Type == AstTypes.VectorLiteral || ast.Type == AstTypes.ListLiteral)
            {
                Components = new List<double>(ast.Contents.Select(x => x.Value).ToArray());
            }
            else
            {
                Components = new List<double>() { ast.Value };
            }
        }
        public MVector(params double[] elements)
            : this(new List<double>(elements))
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<");
            for (int i = 0; i < Components.Count; i++)
            {
                sb.Append(Components[i].ToString());
                if (i + 1 < Components.Count)
                {
                    sb.Append(",");
                }
            }
            sb.Append(">");
            return sb.ToString();
        }
        public Ast[] ToAstArray()
        {
            Ast[] arr = new Ast[Components.Count];
            for (int i = 0; i < Components.Count; i++)
            {
                arr[i] = Ast.CreateNumberAst(Components[i]);
            }
            return arr;
        }

        public MVector SetAllTo(double value)
        {
            List<double> comps = new List<double>();
            for (int i = 0; i < NumComponents; i++)
            {
                comps.Add(value);
            }
            return new MVector(comps);
        }

        public List<double> CopyValues()
        {
            List<double> copy = new List<double>();
            foreach (double d in Components)
            {
                copy.Add(d);
            }
            return copy;
        }

        public static MVector Scale(MVector v, double scalar)
        {
            List<double> comps = new List<double>();
            foreach (double val in v.Components)
            {
                comps.Add(val * scalar);
            }
            return new MVector(comps);
        }

        public static double Magnitude(MVector v)
        {
            double sumSqrs = 0;
            foreach (double val in v.Components)
            {
                sumSqrs += Math.Pow(val, 2);
            }
            return Math.Sqrt(sumSqrs);
        }

        public static double DotProduct(MVector v1, MVector v2)
        {
            if (v1.NumComponents != v2.NumComponents)
            {
                throw new InvalidArgumentsException("Vector lengths must be equal to compute a dot product");
            }
            double prod = 0;
            for (int i = 0; i < v1.NumComponents; i++)
            {
                prod += v1.Components[i] * v2.Components[i];
            }
            return prod;
        }

        public static MVector CrossProduct(MVector v1, MVector v2)
        {
            if (v1.NumComponents != 3 || v2.NumComponents != 3)
            {
                throw new InvalidArgumentsException("Vector lengths must be equal to 3 to compute a dot product");
            }
            double x, y, z;
            x = v1.Components[1] * v2.Components[2] - v2.Components[1] * v1.Components[2];
            y = (v1.Components[0] * v2.Components[2] - v2.Components[0] * v1.Components[2]) * -1;
            z = v1.Components[0] * v2.Components[1] - v2.Components[0] * v1.Components[1];

            if (x == -0)
            {
                x = 0;
            }
            if (y == -0)
            {
                y = 0;
            }
            if (z == -0)
            {
                z = 0;
            }

            return new MVector(x, y, z);
        }

        public static MVector operator +(MVector v) => v;
        public static MVector operator -(MVector v)
        {
            List<double> comps = new List<double>();
            foreach (double val in v.Components)
            {
                comps.Add(-val);
            }
            return new MVector(comps);
        }
        public static MVector operator +(MVector v1, MVector v2)
        {
            if (v1.NumComponents != v2.NumComponents)
            {
                throw new InvalidArgumentsException("Vector lengths must be equal to add/subtract");
            }
            List<double> comps = new List<double>();
            for (int i = 0; i < v1.NumComponents; i++)
            {
                comps.Add(v1.Components[i] + v2.Components[i]);
            }
            return new MVector(comps);
        }
        public static MVector operator -(MVector v1, MVector v2) => v1 + (-v2);

        public static bool operator ==(MVector v1, MVector v2) => v1.Equals(v2);
        public static bool operator !=(MVector v1, MVector v2) => !v1.Equals(v2);

        public override bool Equals(object obj)
        {
            if (!(obj is MVector))
            {
                return false;
            }
            MVector v = (MVector)(obj as MVector?);
            if (v.NumComponents != NumComponents)
            {
                return false;
            }
            for (int i = 0; i < NumComponents; i++)
            {
                if (Components[i] != v.Components[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
