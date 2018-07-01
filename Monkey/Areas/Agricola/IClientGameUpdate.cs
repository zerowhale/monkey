using BoardgamePlatform.Game;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.RoundActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public interface IClientGameUpdate
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        GamePlayer[] Players
        {
            get;
        }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        String ActivePlayerName
        {
            get;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        GameAction[] Actions
        {
            get;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Dictionary<int, string> MajorImprovementOwners
        {
            get;
        }

        InterruptAction Interrupt
        {
            get;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Dictionary<String, ResourceCache[]>[] ReservedResources
        {
            get;
        }

        [JsonProperty("StartingPlayer", NullValueHandling = NullValueHandling.Ignore)]
        String StartingPlayerName
        {
            get;
        }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        Object MyHand
        {
            get;
            set;
        }
    }
}