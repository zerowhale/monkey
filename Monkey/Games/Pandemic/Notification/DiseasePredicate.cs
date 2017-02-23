using Monkey.Game.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public struct DiseasePredicate : INoticePredicate
    {

        public DiseasePredicate(DiseaseColor disease)
            : this(disease, 1)
        {
        }

        public DiseasePredicate(DiseaseColor disease, int count)
            :this()
        {
            Disease = disease;
            Count = count;
        }

        public int Count
        {
            get;
            set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public DiseaseColor Disease
        {
            get;
            set;
        }

        public string PredicateType
        {
            get { return this.GetType().Name; }
        }
    }
}