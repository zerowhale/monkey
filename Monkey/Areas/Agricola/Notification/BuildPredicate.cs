using BoardgamePlatform.Game.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Notification
{
    public class BuildPredicate : INoticePredicate
    {
        public BuildPredicate(int count, Buildable what)
        {
            Count = count;
            What = what;
        }

        public int Count;

        [JsonConverter(typeof(StringEnumConverter))]
        public Buildable What;

        public string PredicateType
        {
            get { return this.GetType().Name; }
        }
    }
}