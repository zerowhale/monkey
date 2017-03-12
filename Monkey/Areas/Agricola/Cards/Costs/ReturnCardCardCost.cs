using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Costs
{
    public class ReturnCardCardCost: CardCost
    {
        public ReturnCardCardCost(XElement definition)
        {
            Text = (string)definition.Attribute("Text");
            Ids = ((string)definition.Attribute("Ids")).Split(',').Select(Int32.Parse).ToArray();
        }

        public int[] Ids
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            private set;
        }

    }
}