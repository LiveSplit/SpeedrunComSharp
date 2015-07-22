using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class VariableValue : IElementWithID
    {
        public string ID { get; private set; }

        public string VariableID { get; private set; }
        

        #region Links

        internal Lazy<Variable> variable;
        internal Lazy<string> value;

        public Variable Variable { get { return variable.Value; } }
        public string Value { get { return value.Value; } }
        public string Name { get { return Variable.Name; } }

        #endregion

        private VariableValue() { }

        public static VariableValue ParseValueDescriptor(SpeedrunComClient client, KeyValuePair<string, dynamic> valueElement)
        {
            var value = new VariableValue();

            value.VariableID = valueElement.Key;
            value.ID = valueElement.Value as string;

            //Parse Links

            value.variable = new Lazy<Variable>(() => client.Variables.GetVariable(value.VariableID));
            value.value = new Lazy<string>(() => value.Variable.Choices.FirstOrDefault(x => x.ID == value.ID).Value);

            return value;
        }

        public static VariableValue ParseIDPair(SpeedrunComClient client, Variable variable, KeyValuePair<string, dynamic> valueElement)
        {
            var value = new VariableValue();

            value.VariableID = variable.ID;
            value.ID = valueElement.Key as string;

            //Parse Links

            value.variable = new Lazy<Variable>(() => variable);

            var valueName = valueElement.Value as string;
            value.value = new Lazy<string>(() => valueName);

            return value;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as VariableValue;

            if (other == null)
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
