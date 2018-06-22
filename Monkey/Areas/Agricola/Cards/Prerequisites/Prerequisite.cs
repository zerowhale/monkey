using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{

    /// <summary>
    /// A prerequisite is a requirement that must be met to be able to play
    /// a card. 
    /// </summary>
    public abstract class Prerequisite
    {
        public Prerequisite(XElement definition)
        {
        }

        public static Prerequisite Create(XElement options)
        {
            var cls = (string)options.Attribute("Class");
            var type = System.Type.GetType(cls);

            return (Prerequisite)Activator.CreateInstance(type, options);
        }

        public abstract bool IsMet(AgricolaPlayer player);

        public string Type
        {
            get { return this.GetType().Name; }
        }

    }
}