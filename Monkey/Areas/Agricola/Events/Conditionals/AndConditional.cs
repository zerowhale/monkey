using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Conditionals
{
    public class AndConditional : GameEventConditional
    {
        public AndConditional() : base() { }

        public AndConditional(XElement definition) : base(definition)
        {
            Conditionals = definition.Elements("Conditional").Select(GameEventConditional.Create).ToImmutableArray();
        }

        public override bool IsMet(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer)
        {
            foreach (var condition in Conditionals)
            {
                if (!condition.IsMet(resolvingPlayer, triggeringPlayer))
                    return false;
            }
            return true;
        }

        private ImmutableArray<GameEventConditional> Conditionals { get; set; }
    }
}
