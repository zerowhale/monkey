using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Utils
{
    public static class XElementExtensions
    {
        public static IEnumerable<XElement> Grandchildren(this XElement @this, String childName, String grandchildName)
        {
            var child = @this.Element(childName);
            if (child != null)
            {
                return child.Elements(grandchildName);
            }
            return Enumerable.Empty<XElement>();
        }
    }
}