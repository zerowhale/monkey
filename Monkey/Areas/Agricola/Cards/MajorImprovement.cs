using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    /// <summary>
    /// Representation of cards from the Major Improvement deck
    /// </summary>
    public class MajorImprovement : Improvement
    {
        public MajorImprovement(XElement definition)
            : base(definition, "Major")
        {
        }
    }
}