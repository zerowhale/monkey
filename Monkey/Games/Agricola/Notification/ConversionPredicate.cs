using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Notification
{
    public class ConversionPredicate : INoticePredicate
    {
        
        public ConversionPredicate(ResourceCache input, ResourceCache output){
            Input = input;
            Output = output;
        }

        public string PredicateType
        {
            get { return this.GetType().Name; }
        }

        public ResourceCache Input
        {
            get;
            private set;
        }

        public ResourceCache Output
        {
            get;
            private set;
        }
    }
}