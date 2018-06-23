using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Data
{
    public class CacheExchange : ResourceConversion
    {
        public CacheExchange(XElement definition)
            : base(definition)
        {
            OnAction = (string)definition.Attribute("OnAction");
        }

        public static new CacheExchange Create(XElement definition)
        {
            return new CacheExchange(definition);
        }

        public readonly string OnAction;
    }
}