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
    /// <summary>
    /// A ResourceCache represents a quantity of one type of resource
    /// </summary>
    public class ResourceCache: INoticePredicate
    {
        public ResourceCache(Resource type, int count)
        {
            this.Type = type;
            this.Count = count;
        }

        /// <summary>
        /// The predicate type used to identify events related to this
        /// </summary>
        public string PredicateType
        {
            get { return "ResourcePredicate"; }
        }

        /// <summary>
        /// Updates the amount of resource in this cache
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>A new ResourceCache with the updated quantity.</returns>
        public ResourceCache updateCount(int amount)
        {
            return new ResourceCache(this.Type, this.Count + amount);
        }

        /// <summary>
        /// The type of the resource
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly Resource Type;

        /// <summary>
        /// The amount of the resource
        /// </summary>
        public readonly int Count;

    }
}