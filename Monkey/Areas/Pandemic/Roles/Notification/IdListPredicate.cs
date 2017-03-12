using BoardgamePlatform.Game.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public struct IdListPredicate : INoticePredicate
    {

        public IdListPredicate(int[] ids)
            : this()
        {
            Ids = ids;
        }

        public int[] Ids
        {
            get;
            set;
        }

        public string PredicateType
        {
            get { return typeof(IdListPredicate).Name; }
        }
    }
}