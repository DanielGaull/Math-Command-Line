using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmdTool
{
    public struct MathDataValue : ICloneable
    {
        public MathDataTypes Type;
        public double NumberValue;
        private MList listValue;
        public MList ListValue
        {
            get
            {
                switch (Type)
                {
                    case MathDataTypes.Number:
                        return new MList(new MathDataValue() { Type = MathDataTypes.Number, NumberValue = this.NumberValue });
                    case MathDataTypes.List:
                        return listValue;
                    case MathDataTypes.Vector:
                        List<MathDataValue> comps = new List<MathDataValue>();
                        for (int i = 0; i < VectorValue.NumComponents; i++)
                        {
                            comps.Add(new MathDataValue() { Type = MathDataTypes.Number, NumberValue = VectorValue.Components[i] });
                        }
                        return new MList(comps);
                }
                return new MList();
            }
        }
        public MVector VectorValue;

        public MathDataValue(MathDataTypes type, double numValue, MList listValue, MVector vectorValue)
        {
            Type = type;
            this.listValue = new MList();
            switch (type)
            {
                case MathDataTypes.Number:
                    NumberValue = numValue;
                    VectorValue = new MVector(numValue);
                    break;
                case MathDataTypes.List:
                    NumberValue = MList.Length(listValue);
                    this.listValue = listValue;
                    VectorValue = new MVector(listValue.Elements.Select(x => x.NumberValue).ToList());
                    break;
                case MathDataTypes.Vector:
                    NumberValue = vectorValue.NumComponents;
                    VectorValue = vectorValue;
                    break;
                default:
                    NumberValue = 0;
                    VectorValue = new MVector();
                    break;
            }
        }
        public MathDataValue(double numValue)
            : this(MathDataTypes.Number, numValue, new MList(), new MVector())
        {
        }

        public MathDataValue(MList listValue)
            : this(MathDataTypes.List, listValue.Elements.Count, listValue, new MVector())
        {
        }
        public MathDataValue(MVector vectorValue)
            : this(MathDataTypes.Vector, vectorValue.NumComponents, new MList(), vectorValue)
        {
        }
        public MathDataValue(MathDataTypes type)
            : this(type, 0, new MList(), new MVector())
        {
        }

        public MathDataValue(Ast ast)
        {
            listValue = new MList();
            VectorValue = new MVector();
            switch (ast.Type)
            {
                case AstTypes.Number:
                    Type = MathDataTypes.Number;
                    break;
                case AstTypes.ListLiteral:
                    Type = MathDataTypes.List;
                    listValue = new MList(ast.Contents);
                    break;
                case AstTypes.VectorLiteral:
                    Type = MathDataTypes.Vector;
                    VectorValue = new MVector(ast.Contents);
                    break;
                default: 
                    Type = MathDataTypes.Number;
                    break;
            }

            NumberValue = ast.Value;
        }

        public void SetNumberValue(double value)
        {
            NumberValue = value;
        }
        public void SetListValue(MList value)
        {
            listValue = value;
            NumberValue = value.Elements.Count;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MathDataTypes.Number:
                    return NumberValue.ToString();
                case MathDataTypes.List:
                    return ListValue.ToString();
                case MathDataTypes.Vector:
                    return VectorValue.ToString();
            }
            return "<null>";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MathDataValue))
            {
                return false;
            }
            MathDataValue dv = (MathDataValue)obj;
            if (dv.Type == Type)
            {
                switch (Type)
                {
                    case MathDataTypes.Number:
                        return NumberValue == dv.NumberValue;
                    case MathDataTypes.List:
                        return ListValue == dv.ListValue;
                    case MathDataTypes.Vector:
                        return VectorValue == dv.VectorValue;
                }
            }
            return false;
        }

        public static bool operator ==(MathDataValue v1, MathDataValue v2) => v1.Equals(v2);
        public static bool operator !=(MathDataValue v1, MathDataValue v2) => !v1.Equals(v2);

        public object Clone()
        {
            return new MathDataValue(Type, NumberValue, ListValue, VectorValue);
        }
    }

    public enum MathDataTypes
    {
        Number,
        List,
        Vector
    }
}
