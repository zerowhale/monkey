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
        /*
        public AnytimeAction(int actionId)
            : base(null, actionId)
        {

        }*/

        public AnytimeAction(XElement definition, int actionId, int cardId)
            :base(null, actionId)
        {
            Prerequisites = definition.Descendants("Prerequisite").Select(Prerequisite.Create).ToArray();
            MaxUses = definition.Attribute("MaxUses") == null ? 0 : (int)definition.Attribute("MaxUses");
            CardId = cardId;

            this.definition = definition;
        }

        /// <summary>
        /// Factory Creation method
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static AnytimeAction Create(XElement definition, int cardId)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            return (AnytimeAction)Activator.CreateInstance(type, definition, cardId);
        }


        public override Boolean CanExecute(AgricolaPlayer player, GameActionData data)
        {
            Object cardData;
            if(MaxUses > 0 && player.TryGetCardMetadata(CardId, out cardData) && MaxUses <= (int)cardData)
            {
                return false;
            }
            return meetsPrerequisites(player);
        }

        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            if(MaxUses > 0)
            {
                Object cardData;
                if (!player.TryGetCardMetadata(CardId, out cardData))
                    cardData = 0;
                player.SetCardMetadata(CardId, ((int)cardData) + 1);
            }
        }

        public AnytimeAction Clone()
        {
            return AnytimeAction.Create(definition, CardId);
        }

        public readonly int CardId;

        public readonly Prerequisite[] Prerequisites;

        public readonly int MaxUses;

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

        /// <summary>
        /// A copy of the definition used to create this action
        /// </summary>
        private XElement definition;
    }


}