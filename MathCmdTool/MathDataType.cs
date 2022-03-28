using System;
using System.Collections.Generic;
using System.Text;

namespace MathCmdTool
{
    class MathDataType
    {
        public string Name { get; private set; }
        public List<DataValue> DataValues { get; private set; }
        public string Format { get; set; }

        public MathDataType()
        {

        }

        public static MathDataType ParseFromDTLine(string line)
        {
            // Parses a data type line to be a math data type. Assumes that we removed the prefix that defines the line type (the .d)

            // Split the line by spaces
            // It goes [name] [data values/restrictions] [string format]
            string[] args = line.Split(' ');

            if (args.Length != 3)
            {
                // Obviously wrong number of args
                throw new MDataTypeParseException("There should be 3 arguments ([name] [data values] [string format])", line);
            }

            // We can just directly pull these
            string name = args[0]; // Name doesn't need any verification
            string dataValuesOneString = args[1];
            string format = args[2];

            // Need to store the data values and make sure they're the correct format
            string[] dataValuesStrings = dataValuesOneString.Split(',');


            // Need to make sure the format provided uses valid characters and only uses the data values


            return null;
        }

        public struct DataValue
        {
            public DataValueTypes Type;
            public double NumberValue;
            public MList ListValue;
            public string Name;

            public DataValue(string name, double num)
            {
                Type = DataValueTypes.Number;
                Name = name;
                ListValue = new MList();
                NumberValue = num;
            }

            public DataValue(string name, MList list)
            {
                Type = DataValueTypes.List;
                Name = name;
                ListValue = list;
                NumberValue = 0;
            }
        }
        public enum DataValueTypes
        {
            Number, List
        }
    }
}
