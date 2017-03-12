using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class DelayedResourceCache: ResourceCache
    {
        public DelayedResourceCache(int roundsDelayed, Resource type, int count)
            : base(type, count)
        {
            Delay = roundsDelayed;
        }

        public int Delay
        {
            get;
            private set;
        }

        public bool OnRound
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "{type: " + this.Type.ToString() +", count: " + this.Count +", delay:" + Delay + ", onRound:" + OnRound + "}";
        }
    }
}