using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Data
{
    public class CacheExchange : ResourceConversion
    {
        public CacheExchange(XElement definition, int Id)
            : base(definition, Id)
        {
            OnAction = (string)definition.Attribute("OnAction");
        }

        public static new CacheExchange Create(XElement definition, int Id)
        {
            return new CacheExchange(definition, Id);
        }

        public string OnAction { get; }
    }
}