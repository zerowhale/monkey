﻿using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class BuildRoomEvent: GameEvent
    {
        public BuildRoomEvent(XElement definition)
            : base(definition)
        {
            this.count = definition.Attribute("Count") != null ? (int)definition.Attribute("Count") : 1;
        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new BuildRoomAction(player, count, resultingNotices));
        }

        private int count;
    
    }
}