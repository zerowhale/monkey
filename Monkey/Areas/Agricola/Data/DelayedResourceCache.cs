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

        public DelayedResourceCache(int roundsDelayed, Resource type, int count, bool onRound)
            : base(type, count)
        {
            this.Delay = roundsDelayed;
            this.OnRound = onRound;
        }

        public int Delay { get; }

        public bool OnRound { get; }

        public override string ToString()
        {
            return "{type: " + this.Type.ToString() +", count: " + this.Count +", delay:" + Delay + ", onRound:" + OnRound + "}";
        }
    }
}