using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    public class Plow
    {
        public Plow(XElement definition)
        {
            OnActions = ((string)definition.Attribute("OnActions")).Split(',').Select(action => Convert.ToInt32(action)).ToArray();
            MaxUses = (int)definition.Attribute("MaxUses");
            Fields = (int)definition.Attribute("Fields");
        }

        public static Plow Create(XElement definition)
        {
            if (definition != null)
                return new Plow(definition);
            return null;
        }

        public void Use()
        {
            this.Used++;
        }

        public int[] OnActions
        {
            get;
            private set;
        }

        public int MaxUses
        {
            get;
            private set;
        }

        public int Fields
        {
            get;
            private set;
        }

        public int Used
        {
            get;
            private set;
        }

    }
}