using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp
{
    public class Variable : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public VariableScope Scope { get; private set; }
        public bool IsMandatory { get; private set; }
        public bool IsUserDefined { get; private set; }
        public bool IsUsedForObsoletingRuns { get; private set; }
        public ReadOnlyCollection<VariableValue> Values { get; private set; }
        public VariableValue DefaultValue { get; private set; }

        #region Links

        private Lazy<Game> game;
        private Lazy<Category> category;
        private Lazy<Level> level;

        public string GameID { get; private set; }
        public Game Game { get { return game.Value; } }
        public string CategoryID { get; private set; }
        public Category Category { get { return category.Value; } }
        public Level Level { get { return level.Value; } }

        #endregion

        private SpeedrunComClient client;

        private Variable() { }

        public VariableValue CreateCustomValue(string customValue)
        {
            if (!IsUserDefined)
                throw new NotSupportedException("This variable doesn't support custom values.");
            
            return VariableValue.CreateCustomValue(client, ID, customValue);
        }

        public static Variable Parse(SpeedrunComClient client, dynamic variableElement)
        {
            var variable = new Variable();

            variable.client = client;

            var properties = variableElement.Properties as IDictionary<string, dynamic>;
            var links = properties["links"] as IEnumerable<dynamic>;

            //Parse Attributes

            variable.ID = variableElement.id as string;
            variable.Name = variableElement.name as string;
            variable.Scope = VariableScope.Parse(client, variableElement.scope) as VariableScope;
            variable.IsMandatory = (bool)(variableElement.mandatory ?? false);
            variable.IsUserDefined = (bool)(properties["user-defined"] ?? false);
            variable.IsUsedForObsoletingRuns = (bool)variableElement.obsoletes;

            if (!(variableElement.values.choices is ArrayList))
            {
                var choiceElements = variableElement.values.choices.Properties as IDictionary<string, dynamic>;
                variable.Values = choiceElements.Select(x => VariableValue.ParseIDPair(client, variable, x) as VariableValue).ToList().AsReadOnly();
            }
            else
            {
                variable.Values = new ReadOnlyCollection<VariableValue>(new VariableValue[0]);
            }

            var valuesProperties = variableElement.values.Properties as IDictionary<string, dynamic>;
            var defaultValue = valuesProperties["default"] as string;
            if (!string.IsNullOrEmpty(defaultValue))
                variable.DefaultValue = variable.Values.FirstOrDefault(x => x.ID == defaultValue);

            //Parse Links

            var gameLink = links.FirstOrDefault(x => x.rel == "game");
            if (gameLink != null)
            {
                var gameUri = gameLink.uri as string;
                variable.GameID = gameUri.Substring(gameUri.LastIndexOf("/") + 1);
                variable.game = new Lazy<Game>(() => client.Games.GetGame(variable.GameID));
            }
            else
            {
                variable.game = new Lazy<Game>(() => null);
            }

            variable.CategoryID = variableElement.category as string;
            if (!string.IsNullOrEmpty(variable.CategoryID))
            {
                variable.category = new Lazy<Category>(() => client.Categories.GetCategory(variable.CategoryID));
            }
            else
            {
                variable.category = new Lazy<Category>(() => null);
            }

            if (!string.IsNullOrEmpty(variable.Scope.LevelID))
            {
                variable.level = new Lazy<Level>(() => client.Levels.GetLevel(variable.Scope.LevelID));
            }
            else
            {
                variable.level = new Lazy<Level>(() => null);
            }

            return variable;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Variable;

            if (other == null)
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
