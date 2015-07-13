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

        public VariableValue(Variable variable, VariableChoice choice)
        {
            VariableID = variable.ID;
            VariableChoiceID = choice.ID;
            this.variable = new Lazy<Variable>(() => variable);
        }

        public VariableValue(string variableId, string variableChoiceId)
        {
            VariableID = variableId;
            VariableChoiceID = variableChoiceId;
        }

        public static VariableValue Parse(SpeedrunComClient client, KeyValuePair<string, dynamic> valueElement)
        {
            var value = new VariableValue(valueElement.Key, valueElement.Value as string);

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
