using Monkey.Games.Agricola.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    /// <summary>
    /// PlayOccupationTrigger is fired any time an occupation enters play,
    /// regardless of what mechanism puts it in play.
    /// </summary>
    public class PlayOccupationTrigger: GameEventTrigger
    {
        public PlayOccupationTrigger(Card occupation)
            :base()
        {
            Occupation = occupation;
        }

        public PlayOccupationTrigger(XElement definition)
            : base(definition)
        {

        }

        public Card Occupation { get; }
    }
}