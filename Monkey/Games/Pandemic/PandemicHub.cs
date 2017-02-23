using Microsoft.AspNet.SignalR;
using Monkey.Games.Agricola;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Monkey.Game;
using Monkey.Games.Pandemic.Cards;


namespace Monkey.Games.Pandemic
{
    public class PandemicHub : GameHub
    {

        public void Pass()
        {
            TakeAction((game, player) => { return game.Pass(player); });
        }

        

        /// <summary>
        /// Handles drive/ferry requests from clients
        /// </summary>
        /// <param name="destination"></param>
        public void Move(City destination)
        {
            TakeAction((game, player) => { return game.Move(player, destination); });
        }

        public void DirectFlight(City destination)
        {
            TakeAction((game, player) => { return game.DirectFlight(player, destination); });
        }

        public void CharterFlight(City destination, City discard)
        {
            TakeAction((game, player) => { return game.CharterFlight(player, destination, discard); });
        }

        public void ShuttleFlight(City destination)
        {
            TakeAction((game, player) => { return game.ShuttleFlight(player, destination); });
        }

        public void BuildResearchStation()
        {
            TakeAction((game, player) => { return game.BuildResearchStation(player); });
        }

        public void TreatDiseases(DiseaseColor[] diseases)
        {
            TakeAction((game, player) => { return game.TreatDiseases(player, diseases); });
        }

        public void DiscoverCure(City[] ids)
        {
            TakeAction((game, player) => { return game.DiscoverCure(player, ids); });
        }

        public void EpidemicInfect()
        {
            TakeAction((game, player) => { return game.EpidemicInfect(player); });
        }

        public void EpidemicIntensify()
        {
            TakeAction((game, player) => { return game.EpidemicIntensify(player); });
        }

        public void Infect()
        {
            TakeAction((game, player) => { return game.Infect(player); });
        }

        public void RequestTrade(string playerName, City city)
        {
            TakeAction((game, player) => { return game.RequestTrade(player, playerName, city); });
        }

        public void AcceptTrade()
        {
            TakeAction((game, player) => { return game.AcceptTrade(player); });
        }
        public void RejectTrade()
        {
            TakeAction((game, player) => { return game.RejectTrade(player); });
        }

        public void PlayEventCard(EventCardType id)
        {
            TakeAction((game, player) => { return game.PlayEventCard(player, id); });
        }

        public void CancelEventCard()
        {
            TakeAction((game, player) => { return game.CancelEventCard(player); });
        }

        public void ExecuteGovernmentGrant(City city)
        {
            TakeAction((game, player) => { return game.ExecuteGovernmentGrant(player, city); });
        }

        public void ExecuteAirLift(string playerName, City city)
        {
            TakeAction((game, player) => { return game.ExecuteAirLift(player, playerName, city); });
        }

        public void ExecuteResilientPopulation(City city)
        {
            TakeAction((game, player) => { return game.ExecuteResilientPopulation(player, city); });
        }

        public void ExecuteForecast(int top, int second, int third, int fourth, int fifth, int sixth)
        {
            TakeAction((game, player) => { return game.ExecuteForecast(player, top, second, third, fourth, fifth, sixth); });
        }

        public void ReorderForecastCards(int top, int second, int third, int fourth, int fifth, int sixth)
        {
            TakeAction((game, player) => { return game.ReorderForecastCards(player, top, second, third, fourth, fifth, sixth); });
        }

        public void Discard(int[] ids)
        {
            TakeAction((game, player) => { return game.Discard(player, ids); });
        }

        public void ExecuteFinalizedResearchStation(City city)
        {
            TakeAction((game, player) => { return game.ExecuteFinalizedResearchStation(player, city); });
        }
        /// <summary>
        /// Executes a game action after doing basic hub authentication,
        /// and sends any updates from the action to the client
        /// </summary>
        /// <param name="action">A delegate to the game action being executed.</param>
        private void TakeAction(Func<PandemicGame, PandemicPlayer, IGameUpdate> action)
        {
            PandemicGame game;
            PandemicPlayer player = GetPlayer();
            if (player != null)
            {
                game = (PandemicGame)player.Game;
                var update = action(game, player);

                if (update != null)
                {
                    UpdateGameState(game, update);
                }
            }
        }

        private void UpdateGameState(PandemicGame game, IGameUpdate update)
        {
            foreach (var player in game.Players)
            {
                Clients.Client(player.Player.ConnectionId.ToString()).update(update);
            }
        }


        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private PandemicPlayer GetPlayer()
        {
            return (PandemicPlayer)gameManager.GetPlayer(Context.ConnectionId).GamePlayer;
        }

    }
}