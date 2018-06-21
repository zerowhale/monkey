using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class ResourceCache: INoticePredicate
    {
        public ResourceCache(Resource type, int count)
        {
            this.Type = type;
            this.Count = count;
        }

        public ResourceCache Clone()
        {
            return new ResourceCache(Type, Count);
        }

        public string PredicateType
        {
            get { return "ResourcePredicate"; }
        }

        public ResourceCache updateCount(int amount)
        {
            return new ResourceCache(this.Type, this.Count + amount);
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public readonly Resource Type;

        public readonly int Count;

    }
}