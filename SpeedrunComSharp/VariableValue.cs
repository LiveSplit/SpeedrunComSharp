using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class VariableValue
    {
        public string VariableID { get; private set; }
        public string VariableChoiceID { get; private set; }

        #region Links

        private Lazy<Variable> variable;

        public Variable Variable { get { return variable.Value; } }
        public string Value { get { return Variable.Choices.FirstOrDefault(x => x.ID == VariableChoiceID).Value; } }
        public string Name { get { return Variable.Name; } }

        #endregion

        private VariableValue() { }

        public static VariableValue Parse(SpeedrunComClient client, KeyValuePair<string, dynamic> valueElement)
        {
            var value = new VariableValue();

            //Parse Attributes

            value.VariableID = valueElement.Key;
            value.VariableChoiceID = valueElement.Value as string;

            //Parse Links

            value.variable = new Lazy<Variable>(() => client.Variables.GetVariable(value.VariableID));

            return value;
        }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
