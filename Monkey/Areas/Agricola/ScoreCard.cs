using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public class ScoreCard
    {
        public ScoreCard(AgricolaPlayer player)
        {
            CalculateFieldsScore(player);
            CalculatePasturesScore(player);
            CalculateGrainScore(player);
            CalculateVegetablesScore(player);
            CalculateSheepScore(player);
            CalculateBoarScore(player);
            CalculateCattleScore(player);
            CalculateUnusedSpaceScore(player);
            CalculateRoomsScore(player);
            CalculateFamilyMemberScore(player);
            CalculateFencedStablesScore(player);
            CalculateBeggingScore(player);
            CalculateBonusPoints(player);
        }

        public int Fields
        {
            get;
            private set;
        }

        public int Pastures
        {
            get;
            private set;
        }

        public int Grain
        {
            get;
            private set;
        }

        public int Vegetables
        {
            get;
            private set;
        }

        public int Sheep
        {
            get;
            private set;
        }

        public int Boar
        {
            get;
            private set;
        }

        public int Cattle
        {
            get;
            private set;
        }

        public int UnusedSpace
        {
            get;
            private set;
        }

        public int FencedStables
        {
            get;
            private set;
        }

        public int Rooms
        {
            get;
            private set;
        }

        public int FamilyMembers
        {
            get;
            private set;
        }

        public List<BonusPointsData> BonusPoints
        {
            get;
            private set;
        }

        public int Begging
        {
            get;
            private set;
        }

        public int Total
        {
            get
            {
                var total = Fields +
                    Pastures +
                    Grain +
                    Vegetables +
                    Sheep +
                    Boar +
                    Cattle +
                    UnusedSpace +
                    FencedStables +
                    Rooms +
                    FamilyMembers +
                    Begging +
                    BonusPoints.Select(x => x.Count).Sum();

                return total;
            }
        }
        private void CalculateFieldsScore(AgricolaPlayer player)
        {
            Fields = Curator.CalculateFieldsScore(player);
        }

        private void CalculatePasturesScore(AgricolaPlayer player)
        {
            Pastures = Curator.CalculatePasturesScore(player);
        }

        private void CalculateGrainScore(AgricolaPlayer player)
        {
            Grain = Curator.CalculateGrainScore(player);
        }

        private void CalculateVegetablesScore(AgricolaPlayer player)
        {
            Vegetables = Curator.CalculateVegetablesScore(player);
        }

        private void CalculateSheepScore(AgricolaPlayer player)
        {
            Sheep = Curator.CalculateSheepScore(player);
        }

        private void CalculateBoarScore(AgricolaPlayer player)
        {
            Boar = Curator.CalculateBoarScore(player);
        }

        private void CalculateCattleScore(AgricolaPlayer player)
        {
            Cattle = Curator.CalculateCattleScore(player);
        }

        private void CalculateUnusedSpaceScore(AgricolaPlayer player)
        {
            UnusedSpace = Curator.CalculateUnusedSpaceScore(player);
        }

        private void CalculateFencedStablesScore(AgricolaPlayer player)
        {
            FencedStables = Curator.CalculateFencedStablesScore(player);
        }

        private void CalculateRoomsScore(AgricolaPlayer player)
        {
            Rooms = Curator.CalculateRoomsScore(player);
        }

        private void CalculateFamilyMemberScore(AgricolaPlayer player)
        {
            FamilyMembers = Curator.CalculateFamilyMemberScore(player);
        }

        private void CalculateBeggingScore(AgricolaPlayer player)
        {
            Begging = Curator.CalculateBeggingScore(player);
        }

        /// <summary>
        /// Calculates all bonus points from cards.
        /// </summary>
        /// <param name="player"></param>
        private void CalculateBonusPoints(AgricolaPlayer player)
        {
            BonusPoints = new List<BonusPointsData>();
            foreach (var card in player.OwnedCards)
            {
                if (card is Improvement && (card as Improvement).Points > 0)
                {
                    BonusPoints.Add(new BonusPointsData(card.Name, (card as Improvement).Points));
                }

                if (/*player.Game.IsOver &&*/ card.GameEndPoints != null){
                    foreach(var calc in card.GameEndPoints){
                        string message;
                        var points = calc.GetPoints(player, out message);
                        if (points > 0)
                        {
                            BonusPoints.Add(new BonusPointsData(message, points));
                        }
                        
                    }
                }
            }

            //if (player.Game.IsOver)
            {
                var players = ((AgricolaGame)player.Game).AgricolaPlayers;
                foreach (var p in players)
                {
                    if (p != player)
                    {
                        foreach (var card in p.OwnedCards)
                        {
                            foreach (var calc in card.GameEndPoints)
                            {
                                if (calc.AllPlayers)
                                {
                                    string message;
                                    var points = calc.GetPoints(player, out message);
                                    if (points > 0)
                                    {
                                        BonusPoints.Add(new BonusPointsData(message, points));
                                    }
                        
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}