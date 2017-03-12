using BoardgamePlatform.Game.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public struct UsedActionsPredicate : INoticePredicate
    {

        public UsedActionsPredicate(int used)
            : this()
        {
            Used = used;
        }

        public int Used
        {
            get;
            set;
        }

        public string PredicateType
        {
            get { return typeof(UsedActionsPredicate).Name; }
        }
    }
}