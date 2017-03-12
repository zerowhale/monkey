using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Cards.Prerequisites;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Actions.AnytimeActions
{
    public abstract class AnytimeAction : GameAction
    {
        public AnytimeAction(int id)
            : base(null, id)
        {

        }

        public AnytimeAction(int id, XElement definition)
            :base(null, id)
        {
            Prerequisites = definition.Descendants("Prerequisite").Select(Prerequisite.Create).ToArray();
            MaxUses = definition.Attribute("MaxUses") == null ? 0 : (int)definition.Attribute("MaxUses");

            this.definition = definition;
        }

        /// <summary>
        /// Factory Creation method
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static AnytimeAction Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            return (AnytimeAction)Activator.CreateInstance(type, definition);
        }


        public override Boolean CanExecute(AgricolaPlayer player, GameActionData data)
        {
            return (MaxUses <= 0 || MaxUses > uses)
                    && meetsPrerequisites(player);
        }

        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            
            uses++;
        }

        public AnytimeAction Clone()
        {
            return AnytimeAction.Create(definition);
        }


        public Prerequisite[] Prerequisites
        {
            get;
            set;
        }

        public int MaxUses
        {
            get;
            set;
        }

        public int Uses
        {
            get { return uses;  }
        }

        [JsonProperty(PropertyName="Type")]
        public string ClassType
        {
            get { return this.GetType().Name; }
        }

        private bool meetsPrerequisites(AgricolaPlayer player)
        {
            if (Prerequisites.Count() > 0)
            {
                foreach (var prereq in Prerequisites)
                {
                    if (!prereq.IsMet(player))
                        return false;
                }
            }
            return true;
        }

        private int uses = 0;

        /// <summary>
        /// A copy of the definition used to create this action
        /// </summary>
        private XElement definition;
    }


}