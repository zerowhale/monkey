using Microsoft.AspNet.SignalR;
using Monkey.Games.Agricola;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Data;
using Monkey.Game;
using Microsoft.AspNet.SignalR.Hubs;
using Monkey.Game.Notification;

namespace Monkey.Games.Agricola
{
    public class AgricolaHub : GameHub
    {





        public void CompleteHarvest(HarvestData data)
        {
            IClientGameUpdate update;
            List<GameActionNotice> notices;

            AgricolaGame game;
            AgricolaPlayer player = GetPlayer();
            if (player != null)
            {
                try
                {
                    game = player.Game as AgricolaGame;
                    var result = game.CompleteHarvest(player, data, out update, out notices);
                    if (result)
                    {

                        if (notices != null)
                        {
                            foreach (var notice in notices)
                            {
                                Clients.Group(game.Id.ToString()).message(notice);
                            }
                        }

                        UpdateGameState(game, update);



                    }
                }
                catch (Exception e)
                {
                    Clients.Caller.error(e.StackTrace);
                    throw;
                }
            }
        }

        public void TakeAction(int actionId)
        {
            TakeAction(actionId, null);
        }

        public void TakeAction(int actionId, GameActionData data, int cardId = -1)
        {
            IClientGameUpdate update;
            List<GameActionNotice> notices;

            AgricolaGame game;
            AgricolaPlayer player = GetPlayer();
            if (player != null)
            {
                try
                {
                    if (data == null)
                        data = new GameActionData();
                    data.ActionId = actionId;
                    game = (AgricolaGame)player.Game;
                    var result = cardId > -1
                        ? game.TakeAnytimeAction(player, actionId, cardId, data, out update, out notices)
                        : game.TakeAction(player, actionId, data, out update, out notices);

                    if (result)
                    {

                        if (notices != null)
                        {
                            foreach (var notice in notices)
                            {
                                Clients.Group(game.Id.ToString()).message(notice);
                            }
                        }

                        UpdateGameState(game, update);
                    }
                }
                catch (Exception e)
                {
                    Clients.Caller.error(e.StackTrace);
                    throw;
                }
            }
        }

        public void TakeStartingPlayerAction(int actionId, StartingPlayerActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildingResourcesAction(int actionId, BuildingResourcesActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakePlowAndSowAction(int actionId, PlowAndSowActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildRoomsAndStablesAction(int actionId, BuildRoomsAndStablesActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildRoomOrTravelingPlayersAction(int actionId, BuildRoomOrTravelingPlayersActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildFencesAction(int actionId, BuildFencesActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeAnimalCacheAction(int actionId, AnimalCacheActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeSowAndBakeAction(int actionId, SowAndBakeActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeImprovementAction(int actionId, ImprovementActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeRenovationAction(int actionId, RenovationActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeFamilyGrowthAction(int actionId, FamilyGrowthActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeAnimalChoiceAction(int actionId, AnimalChoiceActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildStableAndBakeAction(int actionId, BuildStableAndBakeActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBakeAction(int actionId, BakeActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeOccupationAction(int actionId, OccupationActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeBuildStableAction(int actionId, BuildStableActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeSelectResourcesAction(int actionId, SelectResourcesActionData resources)
        {
            TakeAction(actionId, resources);
        }

        public void TakeBuildRoomAction(int actionId, BuildRoomData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeAssignAnimalsAction(int actionId, AnimalCacheActionData data)
        {
            TakeAction(actionId, data);
        }

        public void TakePlayerChoiceAction(int actionId, PlayerChoiceData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeCacheExchangeAction(int actionId, CacheExchangeData data)
        {
            TakeAction(actionId, data);
        }

        public void TakeFencePastureAction(int actionId, BuildFencesActionData data)
        {
            TakeAction(actionId, data);
        }

        /// <summary>
        /// Cook Anytime Action handler
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="data"></param>
        public void TakeCookAnytimeAction(int actionId, int cardId, CookActionData data)
        {
            TakeAction(actionId, data, cardId);
        }

        public void TakeBuildRoomAnytimeAction(int actionId, int cardId, BuildRoomData data)
        {
            TakeAction(actionId, data, cardId);
        }




        private void UpdateGameState(AgricolaGame game, IClientGameUpdate update)
        {
            foreach (var player in game.AgricolaPlayers)
            {
                if (!game.FamilyMode)
                {
                    update.MyHand = player.Hand;
                }
                Clients.Client(player.Player.ConnectionId.ToString()).update(update);
            }
        }


        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private AgricolaPlayer GetPlayer()
        {
            return (AgricolaPlayer)gameManager.GetPlayer(Context.ConnectionId).GamePlayer;
        }

    }
}