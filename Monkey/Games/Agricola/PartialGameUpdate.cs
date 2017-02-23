using Monkey.Game;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.RoundActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public class PartialGameUpdate: IClientGameUpdate
    {

        public PartialGameUpdate(){
        }

        public void AddPlayer(AgricolaPlayer player)
        {
            players.Add(player);
        }

        public void AddAction(GameAction action)
        {
            actions.Add(action);
        }

        public InterruptAction Interrupt
        {
            get;
            set;
        }

        public void AddMajorImprovementOwners(Dictionary<int, string> owners){
            MajorImprovementOwners = owners;
        }

        public Dictionary<int, string> MajorImprovementOwners
        {
            get;
            private set;
        }

        public String ActivePlayerName
        {
            get;
            set;
        }

        public GamePlayer[] Players
        {
            get { return players.Count == 0 ? null : players.ToArray(); }
        }

        public GameAction[] Actions
        {
            get { return actions.Count == 0 ? null : actions.ToArray(); }
        }

        public Dictionary<string, ResourceCache[]>[] ReservedResources
        {
            get;
            set;
        }

        private List<AgricolaPlayer> players = new List<AgricolaPlayer>();
        private List<GameAction> actions = new List<GameAction>();

        public string StartingPlayerName
        {
            get;
            set;
        }

        public object MyHand
        {
            get;
            set;
        }

        public List<Card> DirtyCards
        {
            get;
            set;
        }
    }
}