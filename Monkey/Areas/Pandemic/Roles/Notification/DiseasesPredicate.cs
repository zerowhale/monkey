using BoardgamePlatform.Game.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public struct DiseasesPredicate : INoticePredicate
    {

        public DiseasesPredicate(DiseaseColor[] diseases)
            : this()
        {
            Diseases = diseases;
        }

        public DiseaseColor[] Diseases
        {
            get;
            set;
        }

        public string PredicateType
        {
            get { return typeof(DiseasesPredicate).Name; }
        }
    }
}