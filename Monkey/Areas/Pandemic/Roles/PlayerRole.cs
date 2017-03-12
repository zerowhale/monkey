using BoardgamePlatform.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Roles
{
    public class PlayerRole
    {
        private PlayerRole(PlayerRoleType type, string name, PlayerColor color)
        {
            Type = type;
            Name = name;
            Color = color;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerRoleType Type
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        [JsonIgnore]
        public PlayerColor Color
        {
            get;
            private set;
        }


        public static PlayerRole ContingencyPlanner = new PlayerRole(PlayerRoleType.ContingencyPlanner, "Contingency Planner", PlayerColor.Teal);
        public static PlayerRole Researcher = new PlayerRole(PlayerRoleType.Researcher, "Researcher", PlayerColor.Brown);
        public static PlayerRole Dispatcher = new PlayerRole(PlayerRoleType.Dispatcher, "Dispatcher", PlayerColor.Pink);
        public static PlayerRole QuarantineSpecialist = new PlayerRole(PlayerRoleType.QuarantineSpecialist, "Quarantine Specialist", PlayerColor.Green);
        public static PlayerRole Medic = new PlayerRole(PlayerRoleType.Medic, "Medic", PlayerColor.Orange);
        public static PlayerRole OperationsExpert = new PlayerRole(PlayerRoleType.OperationsExpert, "Operations Expert", PlayerColor.Lime);
        public static PlayerRole Scientist = new PlayerRole(PlayerRoleType.Scientist, "Scientist", PlayerColor.White);
    }
}