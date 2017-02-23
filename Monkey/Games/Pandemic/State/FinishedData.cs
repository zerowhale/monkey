using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State
{
    public class FinishedData : GameStateData
    {
        public FinishedData(bool win, GameLossReason lossReason = GameLossReason.None)
        {
            Win = win;
            LossReason = lossReason;
        }

        public bool Win {
            get;
            set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameLossReason LossReason
        {
            get;
            set;
        }

    }
}