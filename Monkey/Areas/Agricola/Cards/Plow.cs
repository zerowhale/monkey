using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Collections.Immutable;

namespace Monkey.Games.Agricola.Cards
{
    public class Plow
    {
        public Plow(XElement definition)
        {
            OnActions = ((string)definition.Attribute("OnActions")).Split(',').Select(action => Convert.ToInt32(action)).ToArray().ToImmutableArray<int>();
            MaxUses = (int)definition.Attribute("MaxUses");
            Fields = (int)definition.Attribute("Fields");
        }

        public Plow(ImmutableArray<int> onActions, int maxUses, int numUses, int fields)
        {
            this.OnActions = onActions;
            this.MaxUses = maxUses;
            this.Used = maxUses;
            this.Fields = fields;
        }

        public static Plow Create(XElement definition)
        {
            if (definition != null)
                return new Plow(definition);
            return null;
        }

        /// <summary>
        /// Increases the use count on the plow by 1.
        /// </summary>
        /// <returns>The updated plow object.</returns>
        public Plow Use()
        {
            return new Cards.Plow(this.OnActions, this.MaxUses, this.Used + 1, this.Fields);
        }

        public readonly ImmutableArray<int> OnActions;

        public readonly int MaxUses;

        public readonly int Fields;

        public readonly int Used;

    }
}