using Microsoft.AspNet.SignalR;
using Monkey.Game;
using Monkey.Game.Notification;
using Monkey.Games.Pandemic.Cards;
using Monkey.Games.Pandemic.ClientState;
using Monkey.Games.Pandemic.Notification;
using Monkey.Games.Pandemic.Roles;
using Monkey.Games.Pandemic.State;
using Monkey.Games.Pandemic.State.Interrupts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic
{
    public class PandemicGame: GameBase<PandemicHub>, IGameUpdate
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PandemicGame(string name, string viewPath, int maxPlayers, Player[] players, Dictionary<string, object> props)
            : base(name, viewPath, maxPlayers, players, props)
        {

            int difficulty;
            if (int.TryParse(props["Difficulty"].ToString(), out difficulty))
                this.difficulty = difficulty;

            if (logger.IsInfoEnabled)
                logger.InfoFormat("Pandemic Game Initialized. Name: [{0}] dificulty: [{1}]", name, this.difficulty);

            OutbreakCount = 0;

            var diseases = Enum.GetValues(typeof(DiseaseColor)).Cast<DiseaseColor>();
            foreach (var disease in diseases)
            {
                cures[disease] = false;
                eradicatedDiseases[disease] = false;
            }


            InitializePlayers(players);
            InitializeInfectionDeck();
            InitializePlayerDeck();
            DetermineStartingPlayer();

            InitializeInfectedCities();

            FinalizeInitialization();

            partialUpdate = new PartialGameUpdate();
        }

        /// <summary>
        /// Sends the full start-game object to a specific player
        /// </summary>
        /// <param name="player"></param>
        public override void SendPlayerGameStart(GamePlayer player)
        {
            if (!players.Contains(player))
                throw new InvalidOperationException("Player is not part of this game");

            HubContext.Clients.Client(player.Player.ConnectionId.ToString()).startingGame(this, new Dictionary<string, object>());
        }

        /// <summary>
        /// Moves a player the destination city if it's adjacent to the players current location.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="city"></param>
        public IGameUpdate Pass(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Pass Turn [{0}].", player.Name);

            if (IsActivePlayer(player)
                && gameState == GameState.PlayerMove
                && interrupt == null)
            {
                var remainingActions = this.activePlayerActionsRemaining;
                Broadcast(player.Name, NoticeVerb.PassTurn, new UsedActionsPredicate(remainingActions));

                DebitPlayerAction(remainingActions);
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }
        
        /// <summary>
        /// Moves a player the destination city if it's adjacent to the players current location.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="city"></param>
        public IGameUpdate Move(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Move [{0}] to [{1}].", player.Name, city);

            if (IsActivePlayer(player)
                && gameState == GameState.PlayerMove
                && interrupt == null
                && Map.AreNeighbors(player.Location, city))
            {
                Broadcast(player.Name, NoticeVerb.Move, new IdPredicate((int)city));

                MovePlayer(player, city);
                DebitPlayerAction();

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        /// <summary>
        /// Moves a player to the destination city if that player has the destination city card
        /// in their hand.  The player discards that card.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="city"></param>
        public IGameUpdate DirectFlight(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Direct Flight [{0}] to [{1}].", player.Name, city);

            if (IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null
                && player.HandIds.Contains((int)city))
            {
                Broadcast(player.Name, NoticeVerb.DirectFlight, new IdPredicate((int)city));

                DiscardCard(player, city);
                MovePlayer(player, city);
                DebitPlayerAction();

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        /// <summary>
        /// Moves a player to the destination city if the player is holding the city card
        /// of their current location.  The player discards that card.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public IGameUpdate CharterFlight(PandemicPlayer player, City city, City discard)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Direct Flight [{0}] to [{1}] discarding [{2}].", player.Name, city, discard);

            if(IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null
                && (player.Location == discard || player.Role == PlayerRole.OperationsExpert)
                && player.HandIds.Contains((int)discard))
            {
                Broadcast(player.Name, NoticeVerb.CharterFlight,
                    new IdPredicate((int)city),
                    new CardPredicate((int)discard));

                DiscardCard(player, discard);
                MovePlayer(player, city);
                DebitPlayerAction();

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        /// <summary>
        /// Moves the player to the destination city if that city and the players current city 
        /// have research stations.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public IGameUpdate ShuttleFlight(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Shuttle Flight [{0}] to [{1}].", player.Name, city);

            if (IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null
                && Map.IsShuttleAvailable(player.Location, (City)city))
            {
                Broadcast(player.Name, NoticeVerb.ShuttleFlight, new IdPredicate((int)city));

                MovePlayer(player, city);
                DebitPlayerAction();

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        /// <summary>
        /// Constructs a research station at the players current city if they are holding
        /// the card of that city, and that city does not already have a research station.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public IGameUpdate BuildResearchStation(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Build Research Station [{0}] at [{1}].", player.Name, player.Location);

            if (IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null
                && (player.HandIds.Contains((int)player.Location) || (player.Role == PlayerRole.OperationsExpert))
                && !Map.HasResearchStation(player.Location))
            {
                var predicate = new BuildResearchStationPredicate((int)player.Location);
                if (player.Role != PlayerRole.OperationsExpert)
                    DiscardCard(player, player.Location);
                else
                    predicate.OperationsExpert = true;

                FinalizeResearchStation(player, player.Location, true);
                Broadcast(player.Name, NoticeVerb.Build, predicate);

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        /// <summary>
        /// Treats diseases at the players current city.  
        /// </summary>
        /// <param name="player">The player treating the diseases</param>
        /// <param name="colors">A list of disease colors to treat.  Colors should be listed multiple times if multiple treatments are being done on the same color.</param>
        /// <returns></returns>
        public IGameUpdate TreatDiseases(PandemicPlayer player, DiseaseColor[] diseases)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Treat Disease [{0}] in [{1}]. Diseases: [{2}].", player.Name, player.Location, String.Join(",", diseases));

            if (IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null)
            {
                var originalDiseases = diseases.ToArray();

                if (player.Role == PlayerRole.Medic)
                    diseases = diseases.Distinct().ToArray();
                else if(cures[DiseaseColor.Red] || cures[DiseaseColor.Black] || cures[DiseaseColor.Blue] || cures[DiseaseColor.Yellow])
                    diseases = ReduceDiseaseListByCures(diseases);


                if (CanTreatDiseases(player, diseases))
                {

                    var treated = Map.TreatDiseases(player.Location, diseases, Cures, player.Role == PlayerRole.Medic);
                    var predicates = new List<INoticePredicate>();
                    predicates.Add(new UsedActionsPredicate(diseases.Length));
                    predicates.Add(new IdPredicate((int)player.Location));
                    foreach(var kvp in treated)
                    {
                        if(kvp.Value > 0)
                        {
                            predicates.Add(new DiseasePredicate(kvp.Key, kvp.Value));
                        }
                    }

                    Broadcast(player.Name, NoticeVerb.Treat, predicates.ToArray());

                    UpdateEradicatedDiseases(diseases, player);
                   
                    DebitPlayerAction(diseases.Length);
                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        public IGameUpdate DiscoverCure(PandemicPlayer player, City[] cities)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Discover Cure [{0}] in [{1}]. Cards: [{2}].", player.Name, player.Location, String.Join(",", cities));

            if (IsActivePlayer(player) 
                && gameState == GameState.PlayerMove
                && interrupt == null
                && CanDiscoverCure(player, cities))
            {
                var curedColor = CureDisease(player, cities);
                DebitPlayerAction();

                Broadcast(player.Name, NoticeVerb.Cure, new DiseasePredicate(curedColor), new IdListPredicate(cities.Cast<int>().ToArray()));

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate UseContingencyPlanner(PandemicPlayer player, EventCardType cardId)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Use Contingency Planner [{0}] retreiving [{1}].", player.Name, cardId);

            if (IsActivePlayer(player)
                && gameState == GameState.PlayerMove
                && interrupt == null
                && Enum.IsDefined(typeof(EventCardType), cardId)
                && playerDiscardPile.HasCard((int)cardId))
            {

                DebitPlayerAction();

                //Broadcast(player.Name, NoticeVerb.Cure, new DiseasePredicate(curedColor), new IdListPredicate(cities.Cast<int>().ToArray()));

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        
        /// <summary>
        /// Draws the bottom card from the epidemic deck,
        /// infects that city, and adds the card to the discard pile.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="update"></param>
        public IGameUpdate EpidemicInfect(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Epidemic Infect [{0}].", player.Name);

            if (IsActivePlayer(player) 
                && gameState == GameState.Epidemic
                && interrupt == null
                && ((EpidemicData)_gameStateData).Step == EpidemicStep.Infect)
            {
                var card = DrawInfectionCard(true);
                var city = (card as CityCard).City;
                InfectCity(city, 3);

                if (gameState != GameState.Finished)
                {
                    var data = this._gameStateData as EpidemicData;
                    data.Step = EpidemicStep.Intensify;
                    this.gameStateData = data;
                }

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        public IGameUpdate EpidemicIntensify(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Epidemic Intensify [{0}].", player.Name);

            if (IsActivePlayer(player) 
                && gameState == GameState.Epidemic
                && interrupt == null
                && ((EpidemicData)_gameStateData).Step == EpidemicStep.Intensify)
            {
                infectionDiscardPile.Shuffle();
                infectionDeck.AddDeck(infectionDiscardPile);
                
                partialUpdate.EpidemicIntensified = true;
                
                var data = this._gameStateData as EpidemicData;
                if (data.RemainingEpidemics > 0)
                {
                    StartEpidemicStage(data.RemainingEpidemics);
                }
                else
                {
                    StartInfectStage();
                }
                
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        public IGameUpdate Infect(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Infect [{0}].", player.Name);

            if (IsActivePlayer(player)
                && interrupt == null
                && gameState == GameState.Infect)
            {
                var card = DrawInfectionCard();
                var city = (card as CityCard).City;
                InfectCity(city);

                if (gameState != GameState.Finished)
                {
                    var data = this._gameStateData as InfectData;
                    data.Step++;
                    if (data.Step >= data.TotalSteps)
                    {
                        StartNextTurn();
                    }
                    else
                        this.gameStateData = data;
                }
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        /// <summary>
        /// Requests to trade a city card to or from another player.
        /// </summary>
        /// <param name="player">The player initiating the trade</param>
        /// <param name="otherPlayerName">The player with whome the initiating player wishes to trade.</param>
        /// <param name="city">The proposed card for trade.</param>
        /// <returns></returns>
        public IGameUpdate RequestTrade(PandemicPlayer player, string otherPlayerName, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Action Request Trade [{0}] requesting trade with [{1}] at [{2}].", player.Name, otherPlayerName, city);

            if (IsActivePlayer(player)
                && interrupt == null
                && gameState == GameState.PlayerMove
                && PlayersByName.ContainsKey(otherPlayerName)
                && otherPlayerName != player.Name)
            {
                var player2 = PlayersByName[otherPlayerName];
                var cardFromResearcher = false;
                ICard card = null;
                if(player.HandIds.Contains((int)city)){
                    card = player.GetCardById((int)city);
                    if (player.Role == PlayerRole.Researcher)
                        cardFromResearcher = true;
                }
                else
                {
                    card = player2.GetCardById((int)city);
                    if (player2.Role == PlayerRole.Researcher)
                        cardFromResearcher = true;
                }

                if (card != null
                    && (card.Id == (int)player.Location || cardFromResearcher))
                {
                    Interrupt = new TradeInterrupt(player, player2, card as CityCard);
                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate AcceptTrade(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Accept Trade [{0}].", player.Name);

            if (interrupt is TradeInterrupt)
            {
                var tradeData = interrupt as TradeInterrupt;
                if (player == tradeData.Player1)
                {
                    tradeData.Player1Confirmed = true;
                }
                    
                if(player == tradeData.Player2)
                {
                    tradeData.Player2Confirmed = true;
                }

                if (tradeData.Player1Confirmed && tradeData.Player2Confirmed)
                {
                    if (tradeData.Player1.Hand.Contains(tradeData.Card))
                    {
                        tradeData.Player1.RemoveCardFromHand(tradeData.Card);
                        tradeData.Player2.AddCardToHand(tradeData.Card);
                    }
                    else
                    {
                        tradeData.Player2.RemoveCardFromHand(tradeData.Card);
                        tradeData.Player1.AddCardToHand(tradeData.Card);
                    }

                    CheckPlayersHandSize();
                    DebitPlayerAction();

                    interrupt = null;
                    partialUpdate.TradeAccepted = true;
                    CheckPlayersHandSize();
                }
                else
                {
                    Interrupt = tradeData;
                }

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate RejectTrade(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Accept Trade [{0}].", player.Name);

            if (interrupt is TradeInterrupt)
            {
                var tradeData = interrupt as TradeInterrupt;
                if (player == tradeData.Player1 || player == tradeData.Player2) 
                {
                    interrupt = null;
                    partialUpdate.TradeAccepted = false;
                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null; 
        }

        /// <summary>
        /// Client request to discard cards down to hand size of 7
        /// </summary>
        /// <param name="player">The player discarding.</param>
        /// <param name="ids">List of card ids to discard.</param>
        /// <returns></returns>
        public IGameUpdate Discard(PandemicPlayer player, int[] ids)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Discard [{0}] cards [{1}].", player.Name, String.Join(",", ids));

            var discardInterrupt = interrupt as DiscardInterrupt;
            if( discardInterrupt != null 
                && discardInterrupt.Player == player
                && discardInterrupt.Count == ids.Length)
            {
                if (ids.Intersect(player.HandIds).Count() == ids.Length)
                {
                    foreach (var id in ids)
                    {
                        DiscardCard(player, id);
                    }

                    partialUpdate.DiscardComplete = true;
                    interrupt = null;

                    Broadcast(player.Name, NoticeVerb.ForcedDiscard, new IdListPredicate(ids));

                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        public IGameUpdate PlayEventCard(PandemicPlayer player, EventCardType cardId)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Play Event Card [{0}] card ", player.Name, cardId);

            var discardInterrupt = interrupt as DiscardInterrupt;

            if (Enum.IsDefined(typeof(EventCardType), cardId)
                && (interrupt == null || (discardInterrupt != null && discardInterrupt.Player == player))
                && player.HasCard((int)cardId))
            {
                if (cardId == EventCardType.ResilientPopulation && infectionDiscardPile.Count == 0)
                    return null;

                var eventCardInterrupt = new EventCardInterrupt(player, cardId);
                Interrupt = eventCardInterrupt;

                if (cardId == EventCardType.OneQuietNight)
                    return ExecuteOneQuietNightEvent(player);

                if (cardId == EventCardType.Forecast)
                {
                    var cards = infectionDeck.Peek(6);
                    eventCardInterrupt.Data = new ForecastEventData(cards);
                }

                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate CancelEventCard(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Cancel Event Card [{0}] card ", player.Name);

            var eventCardInterrupt = interrupt as EventCardInterrupt;

            if (eventCardInterrupt != null
                && eventCardInterrupt.Player == player)
            {
                Interrupt = null;
                CheckPlayersHandSize();
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate ExecuteGovernmentGrant(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute Government Grant [{0}] in [{1}]", player.Name, city);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if (eventCardInterrupt != null
                && eventCardInterrupt.Player == player 
                && eventCardInterrupt.CardId == EventCardType.GovernmentGrant 
                && !Map.HasResearchStation(city))
            {
                DiscardCard(player, (int)EventCardType.GovernmentGrant);
                Interrupt = null;
                FinalizeResearchStation(player, city, false);
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }


        public IGameUpdate ExecuteAirLift(PandemicPlayer player, string targetPlayerName, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute Air Left [{0}] on player [{1}] to [{2}]", player.Name, targetPlayerName, city);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if (eventCardInterrupt != null
                && eventCardInterrupt.Player == player
                && eventCardInterrupt.CardId == EventCardType.AirLift
                && PlayersByName.ContainsKey(targetPlayerName))
            {
                var targetPlayer = PlayersByName[targetPlayerName];
                if (targetPlayer.Location == city)
                    return null;

                MovePlayer(targetPlayer, city);

                DiscardCard(player, (int)EventCardType.AirLift);
                Interrupt = null;
                CheckPlayersHandSize();
                var update = GetUpdate();
                update.AddPlayer(player.GetPartialUpdate());
                return update;
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate ExecuteResilientPopulation(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute Resilient Population [{0}] at [{1}]", player.Name, city);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if(eventCardInterrupt != null 
                && eventCardInterrupt.Player == player
                && eventCardInterrupt.CardId == EventCardType.ResilientPopulation
                && infectionDiscardPile.HasCard((int)city))
            {
                infectionDiscardPile.RemoveCardById((int)city);
                partialUpdate.ExiledCardFromInfection = (int)city;
                DiscardCard(player, (int)EventCardType.ResilientPopulation);
                Interrupt = null;
                CheckPlayersHandSize();
                return GetUpdate();
            }
            

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate ExecuteForecast(PandemicPlayer player, int top, int second, int third, int fourth, int fifth, int sixth)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute Forecast [{0}] cards [{1},{2},{3},{4},{5},{6}]", player.Name, top, second, third, fourth, fifth, sixth);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if (eventCardInterrupt != null 
                && eventCardInterrupt.Player == player
                && eventCardInterrupt.CardId == EventCardType.Forecast)
            {
                var ids = new int[] { top, second, third, fourth, fifth, sixth };
                var data = eventCardInterrupt.Data as ForecastEventData;
                if (data.TryReorder(ids))
                {
                    var cards = data.Cards;
                    ICard nothing;
                    for(var i=0;i<6;i++)
                        infectionDeck.Draw(out nothing);

                    for (var i = cards.Length-1; i >= 0;i-- )
                    {
                        infectionDeck.AddCard(cards[i]);
                    }

                    partialUpdate.ForecastComplete = data.CardIds;

                    DiscardCard(player, (int)EventCardType.Forecast);

                    Interrupt = null;
                    CheckPlayersHandSize();
                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        public IGameUpdate ReorderForecastCards(PandemicPlayer player, int top, int second, int third, int fourth, int fifth, int sixth)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Reorder Forecast [{0}] Order: [{1},{2},{3},{4},{5},{6}]", player.Name, top, second, third, fourth, fifth,sixth);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if (eventCardInterrupt != null
                && eventCardInterrupt.Player == player
                && eventCardInterrupt.CardId == EventCardType.Forecast)
            {
                var data = eventCardInterrupt.Data as ForecastEventData;
                var ids = new int[] { top, second, third, fourth, fifth, sixth };
                if (data.TryReorder(ids))
                {
                    partialUpdate.ForecastCardReorder = ids;
                    return GetUpdate();
                }
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }



        public IGameUpdate ExecuteFinalizedResearchStation(PandemicPlayer player, City removalLocation)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute Finalize Research Station at [{0}]", removalLocation);

            var moveStationInterrupt = interrupt as MoveResearchStationInterrupt;
            if (moveStationInterrupt != null
                && moveStationInterrupt.Player == player
                && Map.HasResearchStation(removalLocation))
            {
                Map.RemoveResearchStation(removalLocation);
                Map.BuildResearchStation(moveStationInterrupt.NewLocation);
                Interrupt = null;
                if (moveStationInterrupt.DebitAction)
                    DebitPlayerAction();
                CheckPlayersHandSize();
                return GetUpdate();
            }

            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");
            return null;
        }

        /// <summary>
        /// Draws the top card of the infection deck and adds it to the 
        /// infection discard pile.
        /// </summary>
        /// <returns>The card that was drawn.</returns>
        private ICard DrawInfectionCard(bool fromBottom = false)
        {
            if (logger.IsInfoEnabled)
                logger.Info("DrawInfectionCard");

            ICard card;
            if (fromBottom)
                infectionDeck.DrawFromBottom(out card);
            else
                infectionDeck.Draw(out card);
            partialUpdate.DrawnInfectionCard = card;
            infectionDiscardPile.AddCard(card);

            Broadcast(ActivePlayer, NoticeVerb.DrawInfectionCard, new IdPredicate(card.Id));

            return card;
        }

        /// <summary>
        /// Attempts to infect the city with the disease of the cities color.
        /// Prior to actually infecting the city a city immunity list is built based
        /// on player roles, which will be factored into potential outbreaks.
        /// </summary>
        /// <param name="city"></param>
        /// <param name="count"></param>
        private void InfectCity(City city, int count = 1)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Infect City: [{0}], [{1}]", city, count );

            PandemicPlayer medic, quarantineSpecialist;

            var cityColor = Map.GetCityColor(city);
            var immune = new List<City>();
            if (cures[cityColor]){
                if (eradicatedDiseases[cityColor])
                {
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("  [{0}] is Eradicated.", cityColor);

                    return;
                }

                if(TryGetPlayerOfRole(PlayerRole.Medic, out medic))
                {
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("  Medic found at [{0}], making location immune.", medic.Location);

                        immune.Add(medic.Location);
                }
            }

            if(TryGetPlayerOfRole(PlayerRole.QuarantineSpecialist, out quarantineSpecialist))
            {
                var neighbors = Map.GetNeighbors(quarantineSpecialist.Location);

                if (logger.IsDebugEnabled)
                    logger.DebugFormat("  Quarantine Specialist found at [{0}], making [{1}] immune.", quarantineSpecialist.Location, String.Join(",", neighbors));

                immune.Add(quarantineSpecialist.Location);
                immune.AddRange(neighbors);
            }

            if (!immune.Contains(city))
            {
                var outbreaks = Map.AddDiseasesToCity(city, count, immune, this);
                if (outbreaks > 0)
                {
                    OutbreakCount += outbreaks;
                }

                if(OutbreakCount >= OUTBREAKS_TO_LOSE)
                {
                    gameState = GameState.Finished;
                    gameStateData = new FinishedData(false, GameLossReason.Outbreaks);
                    return;
                }

                DiseaseColor? color;
                if (Map.DepletedDisease(out color))
                {
                    gameState = GameState.Finished;
                    GameLossReason gameLossReason;
                    switch (color.Value) {
                        case DiseaseColor.Red:
                            gameLossReason = GameLossReason.NoRed;
                            break;
                        case DiseaseColor.Black:
                            gameLossReason = GameLossReason.NoBlack;
                            break;
                        case DiseaseColor.Blue:
                            gameLossReason = GameLossReason.NoBlue;
                            break;
                        default:
                            gameLossReason = GameLossReason.NoYellow;
                            break;
                    }

                    gameStateData = new FinishedData(false, gameLossReason);
                }
            }
        }

        /// <summary>
        /// Packages a notice up and broadcasts it to all game players.
        /// </summary>
        /// <param name="obj">What or who the message is about.</param>
        /// <param name="verb">What action the object is taking.</param>
        /// <param name="predicates">Additional information about what happened.</param>
        public void Broadcast(Object obj, NoticeVerb verb, params INoticePredicate[] predicates)
        {
            Broadcast(new ActionNotice(obj, verb, predicates));
        }

        /// <summary>
        /// The game title.
        /// </summary>
        public override string Title
        {
            get { return "Pandemic"; }
        }

        /// <summary>
        /// Returns the list of players as GamePlayers
        /// </summary>
        public override GamePlayer[] Players
        {
            get { return (GamePlayer[])players.Clone(); }
        }

        /// <summary>
        /// The player who's turn it is currently.
        /// Used for client JSON update.
        /// </summary>
        public string ActivePlayer
        {
            get { return players[activePlayerIndex].Name; }
        }

        /// <summary>
        /// The active players remaining actions.
        /// Used for client JSON update.
        /// </summary>
        public int ActionsRemaining
        {
            get { return activePlayerActionsRemaining;  }
        }

        /// <summary>
        /// Number of outbreaks that have occurred
        /// </summary>
        public int OutbreakCount
        {
            get
            {
                return outbreakCount;
            }
            private set
            {
                if (logger.IsInfoEnabled)
                    logger.InfoFormat("Outbreak Count updated to [{0}].", value);

                outbreakCount = value;
                if(partialUpdate != null)
                    partialUpdate.OutbreakCount = outbreakCount;
            }
        }

        /// <summary>
        /// The index of the infection rate marker.
        /// </summary>
        public int InfectionRateCount
        {
            get
            {
                return infectionRateCount;
            }
            private set
            {
                if (logger.IsInfoEnabled)
                    logger.InfoFormat("Infection Rate updated to [{0}].", value);

                infectionRateCount = value;
                if(partialUpdate != null)
                    this.partialUpdate.InfectionRateCount = value;
            }
        }
        
        /// <summary>
        /// Number of cards in the player deck.
        /// </summary>
        public int PlayerDeckCount
        {
            get { return playerDeck.Count; }
        }

        /// <summary>
        /// The current game state.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GameState? State
        {
            get
            {
                return _gameState;
            }
        }

        /// <summary>
        /// Any state specific data associated with the game state.
        /// </summary>
        public GameStateData StateData
        {
            get
            {
                return _gameStateData;
            }
        }

        /// <summary>
        /// Number of cards in the infection deck
        /// </summary>
        public int InfectionDeckCount
        {
            get { return infectionDeck.Count; }
        }

        /// <summary>
        /// Returns a list of card IDs that are in the infection discard pile.
        /// </summary>
        public int[] InfectionDiscardPileIds
        {
            get { return infectionDiscardPile.GetDeckList().Select(x => x.Id).ToArray(); }
        }

        /// <summary>
        /// Returns a list of card Ids that are in the player discard pile.
        /// </summary>
        public int[] PlayerDiscardPileIds
        {
            get { return playerDiscardPile.GetDeckList().Select(x => x.Id).ToArray(); }
        }




 

        /// <summary>
        /// The interrupt state of the game.  
        /// Adds the interrupt to the game update.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public InterruptState Interrupt
        {
            get
            {
                return interrupt;
            }

            private set
            {
                if (logger.IsInfoEnabled)
                    logger.InfoFormat("Interrupt set to object of type [{0}].", value == null ? "null" : value.Type);

                interrupt = value;
                partialUpdate.Interrupt = value;
            }
        }

        /// <summary>
        /// Returns the state of cured diseases 
        /// </summary>
        public bool[] Cures
        {
            get
            {
                _cures[0] = cures[(DiseaseColor)0];
                _cures[1] = cures[(DiseaseColor)1];
                _cures[2] = cures[(DiseaseColor)2];
                _cures[3] = cures[(DiseaseColor)3];
                return _cures;
            }
        }

        /// <summary>
        /// Returns the state of the eradicated diseases
        /// </summary>
        public DiseaseColor[] EradicatedDiseases
        {
            get
            {
                return eradicatedDiseases.Where(x => x.Value).Select(x => x.Key).ToArray();
            }
        }

        /// <summary>
        /// The number of ourbreaks it takes to lose the game
        /// </summary>
        public const int OUTBREAKS_TO_LOSE = 8;

        /// <summary>
        /// The maximum number of research stations that can be on the board at once.
        /// </summary>
        public const int MAX_RESEARCH_STATION_COUNT = 6;

        /// <summary>
        /// The number of existing diseases on a city that will cause an outbreak
        /// when a new disease is added.
        /// </summary>
        public const int DISEASE_COUNT_FOR_OUTBREAK = 3;

        /// <summary>
        /// The number of actions a player can take per turn by default.
        /// </summary>
        public const int ACTIONS_PER_TURN = 4;

        /// <summary>
        /// The city node map.
        /// </summary>
        public Map Map = new Map();



        private IGameUpdate ExecuteOneQuietNightEvent(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Interrupt Execute One Quiet Night [{0}]", player.Name);

            var eventCardInterrupt = interrupt as EventCardInterrupt;
            if (eventCardInterrupt != null
                && eventCardInterrupt.Player == player
                && eventCardInterrupt.CardId == EventCardType.OneQuietNight)
            {
                DiscardCard(player, (int)EventCardType.OneQuietNight);

                if (gameState == GameState.Infect)
                {
                    StartNextTurn();
                }
                else
                {
                    oneQuietNight = true;
                }
                Interrupt = null;
                CheckPlayersHandSize();
                return GetUpdate();
            }


            if (logger.IsInfoEnabled)
                logger.Info("  Rejected: Invalid Request.");

            return null;
        }

        private void Broadcast(Object obj)
        {
            HubContext.Clients.Group(Id.ToString()).message(obj);
        }

        /// <summary>
        /// Checks if a player can cure a disease.
        /// </summary>
        /// <param name="player">The player curing the disease.</param>
        /// <param name="cityIds">The list of cards being used to cure the disease.</param>
        /// <returns></returns>
        private bool CanDiscoverCure(PandemicPlayer player, City[] cityIds)
        {
            var cardsNeeded = player.Role == PlayerRole.Scientist ? 4 : 5;
            
            if (cityIds.Distinct().Count() == cardsNeeded
                && Map.HasResearchStation(player.Location))
            {
                var hand = player.HandIds;
                DiseaseColor? color = null;
                foreach (var id in cityIds)
                {
                    if (!Enum.IsDefined(typeof(City), (int)id) || !hand.Contains((int)id))
                        return false;

                    var cityColor = Map.GetCityColor(id);
                    if (color == null)
                    {
                        color = cityColor;
                        if (cures[color.Value])
                            return false;
                    }
                    else if (color.Value != cityColor)
                        return false;

                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Moves a player to a city without any validation.
        /// </summary>
        /// <param name="player">The player to move.</param>
        /// <param name="city">The city to move to.</param>
        private void MovePlayer(PandemicPlayer player, City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Move Player [{0}] to [{1}].", player.Name, city);

            player.Location = city;

            if (player.Role == PlayerRole.Medic)
            {
                CheckMedicPassiveTreat(player);
            }

        }



        /// <summary>
        /// Checks if any new diseases has been eradicated and updates the data accordingly
        /// </summary>
        /// <param name="diseases">The list of disease colors to check.</param>
        /// <param name="player">The player who recently treated diseases (for accreditation.)</param>
        private void UpdateEradicatedDiseases(DiseaseColor[] diseases, PandemicPlayer player)
        {
            foreach (var disease in diseases)
            {
                if (cures[disease] && !eradicatedDiseases[disease] && Map.IsEradicated(disease))
                {
                    if (logger.IsInfoEnabled)
                        logger.InfoFormat("  [{0}] Disease has been eradicated!");
                    eradicatedDiseases[disease] = true;
                    partialUpdate.AddEradicatedDisease(disease);

                    Broadcast(player.Name, NoticeVerb.Eradicate, new DiseasePredicate(disease));
                }
            }
        }


        /// <summary>
        /// Cures a disease and removes the moves the cards used from the players hand to
        /// the Player Discard Pile.  This function does no validation.
        /// </summary>
        /// <param name="player">The player curing the disease.</param>
        /// <param name="cards">The cards being discarded to cure the disease.</param>
        private DiseaseColor CureDisease(PandemicPlayer player, City[] cards)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Cure Diseases by player [{0}] with cards [{1}].", player.Name, String.Join(",", cards));

            var color = Map.GetCityColor(cards[0]);;
            cures[color] = true;
            foreach(var city in cards){
                DiscardCard(player, city);
            }

            PandemicPlayer medic;
            if(TryGetPlayerOfRole(PlayerRole.Medic, out medic)){
                CheckMedicPassiveTreat(medic);
            }

            CheckForWin();

            partialUpdate.Cure = color;
            return color;
        }

        /// <summary>
        /// Checks if all 4 diseases have been cured, and if so sets the game to finished.
        /// </summary>
        private void CheckForWin()
        {
            foreach(var c in cures.Values)
            {
                if (!c)
                    return;
            }

            gameState = GameState.Finished;
            gameStateData = new FinishedData(true);

        }

        /// <summary>
        /// Removes any cured diseases from the medics location 
        /// without costing any action points.
        /// </summary>
        /// <param name="medic"></param>
        private void CheckMedicPassiveTreat(PandemicPlayer medic)
        {
            if(medic.Role != PlayerRole.Medic)
            {
                logger.Error("Attempted to use medic passive Treat by non medic.");
                return;
            }

            if (logger.IsInfoEnabled)
                logger.InfoFormat("  Player is Medic, checking cured diseases at node.");

            var city = medic.Location;

            foreach (var kvp in cures)
            {
                if (kvp.Value)
                {
                    var count = Map.TreatAllDiseasesOfType(city, kvp.Key);
                    if (count > 0)
                    {
                        Broadcast(medic.Name, NoticeVerb.MedicTreat, 
                            new IdPredicate((int)medic.Location),
                            new DiseasePredicate(kvp.Key, count));

                        UpdateEradicatedDiseases(new DiseaseColor[] { kvp.Key }, medic);
                    }
                }
            }
        }

        private ICard DiscardCard(PandemicPlayer player, City city)
        {
            return DiscardCard(player, (int)city);
        }

        /// <summary>
        /// Discards the card for a city from a players hand and adds it to the Player Discard Pile.
        /// </summary>
        /// <param name="player">The player discarding the card.</param>
        /// <param name="city"></param>
        /// <returns></returns>
        private ICard DiscardCard(PandemicPlayer player, int id)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Discard Card by [{0}], card [{1}].", player.Name, id);

            var card = player.RemoveCardFromHandById(id);
            if (card == null)
            {
                var err = new InvalidOperationException("A card must be in a players hand to be discarded. Player:" + player.Name + ", card:" + id);
                logger.Error(err);
                throw err;
            }
            playerDiscardPile.AddCard(card);
            return card;
        }

        /// <summary>
        /// Checks if a player has the required actions to treat a list of diseases.
        /// </summary>
        /// <param name="player">The player treating the diseases.</param>
        /// <param name="diseases">The requested list of diseases to treat, multiple treatments of one color will appear in the list individually.</param>
        /// <returns></returns>
        private bool CanTreatDiseases(PandemicPlayer player, DiseaseColor[] diseases)
        {
            return diseases.Length <= activePlayerActionsRemaining && Map.HasDiseases(player.Location, diseases);
        }

        private void StartInfectStage()
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Start Infect Stage.");

            if (!oneQuietNight)
            {
                gameState = GameState.Infect;

                var infectData = new InfectData();
                infectData.TotalSteps = this.GetInfectionRate();
                infectData.Step = 0;
                gameStateData = infectData;
            }
            else
            {
                logger.InfoFormat("  One Quiet Night in effect. Skipping.");
                oneQuietNight = false;
                StartNextTurn();
            }
        }

        private void StartEpidemicStage(int totalEpidemics)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Start Epidemic Stage.  Number of epidemics: [{0}].", totalEpidemics);

            gameState = GameState.Epidemic;
            InfectionRateCount++;

            var epidemicData = new EpidemicData();
            epidemicData.Step = EpidemicStep.Infect;
            epidemicData.RemainingEpidemics = totalEpidemics - 1;
            gameStateData = epidemicData;
        }

        private bool IsActivePlayer(PandemicPlayer player)
        {
            return players[activePlayerIndex] == player;
        }

        /// <summary>
        /// Subtracts a player action and checks if the game state has changed.
        /// </summary>
        private void DebitPlayerAction(int count = 1)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Debit Player Action [{0}] by [{1}].", ActivePlayer, count);

            if (activePlayerActionsRemaining - count < 0)
            {
                var err = new InvalidOperationException("Attempted to debit a player action when the player has no actions remaining.");
                if (logger.IsErrorEnabled)
                    logger.Error(err);
                throw err;
            }

            ActivePlayerActionsRemaining -= count;
            if (ActivePlayerActionsRemaining == 0)
            {
                var epidemics = DrawPlayerCards();

                if (epidemics > 0)
                    StartEpidemicStage(epidemics);
                else
                    StartInfectStage();
            }
        }

        /// <summary>
        /// Draws 2 player cards, adds non epidemic cards to the players hand, and 
        /// counts the number of epidemic cards revealed.
        /// </summary>
        /// <returns>The number of epidemic cards drawn.</returns>
        private int DrawPlayerCards(){
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Draw Player Cards.");

            var player = this.players[activePlayerIndex];
            ICard card1, card2;

            if (playerDeck.Count < 2)
            {
                gameState = GameState.Finished;
                gameStateData = new FinishedData(false, GameLossReason.DeckEmpty);

                if (playerDeck.Count == 1) {
                    playerDeck.Draw(out card1);
                    partialUpdate.AddDrawnPlayerCard(card1);

                    if (!(card1 is EpidemicCard))
                        player.AddCardToHand(card1);
                }
                return 0;
            }

            playerDeck.Draw(out card1);
            playerDeck.Draw(out card2);

            partialUpdate.AddDrawnPlayerCard(card1);
            partialUpdate.AddDrawnPlayerCard(card2);

            var epidemics = 0;

            if (!(card1 is EpidemicCard))
                player.AddCardToHand(card1);
            else
                epidemics++;

            if (!(card2 is EpidemicCard))
                player.AddCardToHand(card2);
            else
                epidemics++;

            CheckPlayersHandSize();

            return epidemics;
        }

        /// <summary>
        /// If the player has more than 7 cards in hand this initiates a discard interrupt.
        /// </summary>
        private void CheckPlayersHandSize()
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Check Player Hand Size.");

            foreach (var player in players)
            {
                if (player.HandSize > 7)
                {
                    Interrupt = new DiscardInterrupt(player, player.HandSize - 7);
                    return;
                }
            }
        }

        /// <summary>
        /// First checks to see if 6 research stations already exist.
        /// If not, the station is immediately build.  If 6 already exist,
        /// a MoveResearchStationInterrupt is initialized and the player needs 
        /// to determine which research center will be moved.
        /// Once this point has been reached this can not be cancelled, and a 
        /// research station to remove must be chosen.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="location"></param>
        private void FinalizeResearchStation(PandemicPlayer player, City location, bool debitAction)
        {
            if (Map.GetResearchStationCount() < MAX_RESEARCH_STATION_COUNT)
            {
                if (debitAction)
                    DebitPlayerAction();
                Map.BuildResearchStation(location);
                CheckPlayersHandSize();
            }
            else
            {
                Interrupt = new MoveResearchStationInterrupt(player, location, debitAction);
            }
        }

        /// <summary>
        /// Gets a partial game update object with only the required fields populated.
        /// </summary>
        /// <returns></returns>
        private PartialGameUpdate GetPartialUpdate()
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("GetPartialUpdate.");

            var update = partialUpdate;
            partialUpdate = new PartialGameUpdate();
            return update;
        }

        /// <summary>
        /// Attempts to find a player of the specific role.
        /// If a player is found it will passed out through the out parameter.
        /// </summary>
        /// <param name="role">The role to look for.</param>
        /// <param name="player">When this method returns, contains the player with the specified role, or null if no player has that role. This parameter is passed uninitialized; any value originally supplied in <paramref name="player"/> will be overwritten.</param>
        /// <returns></returns>
        private bool TryGetPlayerOfRole(PlayerRole role, out PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Try Get Player Of Role [{0}].", role.Name);

            if (PlayersByRole.ContainsKey(role))
            {
                player = PlayersByRole[role];
                if (logger.IsInfoEnabled)
                    logger.InfoFormat("  Found {0} [{1}].", role.Name, player.Name);

                return true;
            }
            player = null;
            return false;
        }

        /// <summary>
        /// Gets the game partial update object and populates it for sending to the client.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private PartialGameUpdate GetUpdate()
        {
            if (logger.IsInfoEnabled)
                logger.Info("Get Update");

            var gameUpdate = GetPartialUpdate();
            foreach (var player in players)
            {
                if (player.HasUpdate())
                    gameUpdate.AddPlayer(player.GetPartialUpdate());
            }

            var mapUpdate = GetMapUpdate();
            if (mapUpdate != null)
                gameUpdate.Map = mapUpdate;

            return gameUpdate;
        }

        /// <summary>
        /// Gets the map partial update
        /// </summary>
        /// <returns>The map update or null if the update is empty</returns>
        private PartialMapUpdate GetMapUpdate()
        {
            var mapUpdate = Map.GetPartialUpdate();
            if (!mapUpdate.IsEmpty())
                return mapUpdate;
            return null;
        }


        /// <summary>
        /// Gets the number of infection cards that are used each round
        /// based on the infection rate count.
        /// </summary>
        /// <returns></returns>
        private int GetInfectionRate()
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Get Infection Rate");

            switch(InfectionRateCount){
                case 0:
                case 1:
                case 2:
                    return 2;
                case 3:
                case 4:
                    return 3;
                default:
                    return 4;
            }
        }

        /// <summary>
        /// Sets up the Pandemic player objects
        /// </summary>
        /// <param name="players"></param>
        private void InitializePlayers(Player[] players){
            this.players = new PandemicPlayer[players.Length];

            var playerRoles = new List<PlayerRole>();
            playerRoles.Add(PlayerRole.ContingencyPlanner);
            //playerRoles.Add(PlayerRole.Dispatcher);
            //playerRoles.Add(PlayerRole.Medic);
            //playerRoles.Add(PlayerRole.OperationsExpert);
            //playerRoles.Add(PlayerRole.QuarantineSpecialist);
            //playerRoles.Add(PlayerRole.Researcher);
            //playerRoles.Add(PlayerRole.Scientist);

            var random = new Random();

            for(var i=0;i<players.Length;i++){
                var role = playerRoles[random.Next(0, playerRoles.Count)];
                playerRoles.Remove(role);

                players[i].GamePlayer = new PandemicPlayer(this, players[i], role);
                this.players[i] = (PandemicPlayer)players[i].GamePlayer;
                PlayersByRole[role] = this.players[i];
                PlayersByName[players[i].Name] = this.players[i];
            }
        }

        /// <summary>
        /// Builds and randomizes the infection deck
        /// </summary>
        private void InitializeInfectionDeck()
        {
            var cities = Enum.GetValues(typeof(City)).Cast<City>();
            foreach(var city in cities){
                infectionDeck.AddCard(new CityCard(Map.GetNode(city).Name, city));
            }
            infectionDeck.Shuffle();
        }

        /// <summary>
        /// Builds the player deck of cards
        /// </summary>
        private void InitializePlayerDeck()
        {
            var cities = Enum.GetValues(typeof(City)).Cast<City>();
            //var i = 0;
            foreach (var city in cities)
            {
                //if (i++ > 1)
                  //  break;
                playerDeck.AddCard(new CityCard(Map.GetNode(city).Name, city));
            }

            playerDeck.AddCard(new EventCard("Government Grant", EventCardType.GovernmentGrant));
            playerDeck.AddCard(new EventCard("Air Lift", EventCardType.AirLift));

            playerDeck.Shuffle();
            playerDeck.AddCard(new EventCard("One Quiet Night", EventCardType.OneQuietNight));
            playerDeck.AddCard(new EventCard("Forecast", EventCardType.Forecast));
            playerDeck.AddCard(new EventCard("Resilient Population", EventCardType.ResilientPopulation));

            ICard card;
            var toDeal = players.Length == 4 ? 2 : (players.Length == 3 ? 3 : 4);
            for (var dealing = 0; dealing < toDeal; dealing++)
            {
                foreach (var player in players)
                {
                    playerDeck.Draw(out card);
                    player.AddCardToHand(card);
                }
            }


            (playerDeck as PlayerDeck).ShuffleInEpidemicCards(difficulty);

        }

        /// <summary>
        /// Determines who the first player is based on which player
        /// has the highest population city in hand
        /// </summary>
        private void DetermineStartingPlayer(){
            var highestPopulation = 0;
            var highestPopulationPlayer = players[0];
            for (var p = 0; p < players.Length; p++)
            {
                var player = players[p];
                foreach (var card in player.Hand)
                {
                    if (card is CityCard)
                    {
                        var cityCard = card as CityCard;
                        var pop = Map.GetCityPopulation(cityCard.City);
                        if(pop > highestPopulation){
                            highestPopulation = pop;
                            highestPopulationPlayer = player;
                        }
                    }
                }
            }

            StartTurn(highestPopulationPlayer);
        }

        private PandemicPlayer GetNextPlayer()
        {
            var i = activePlayerIndex + 1;
            if (i >= players.Length)
                i = 0;
            return players[i];
        }

        /// <summary>
        /// Starts a players turn
        /// </summary>
        /// <param name="player"></param>
        private void StartTurn(PandemicPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Start turn [{0}].", player.Name);

            _gameStateData = null;
            gameState = GameState.PlayerMove;
            ActivePlayerIndex = Array.IndexOf(players, player);
            ActivePlayerActionsRemaining = ACTIONS_PER_TURN;


        }

        /// <summary>
        /// Starts a new turn for the player next in order.
        /// </summary>
        private void StartNextTurn()
        {
            if (logger.IsInfoEnabled)
                logger.Info("Start next Turn");

            StartTurn(GetNextPlayer());
        }


        /// <summary>
        /// Starts the game by drawing 9 infection cards and infecting
        /// those 9 cities with 3/2/1 diseases of the appropriate type.
        /// </summary>
        private void InitializeInfectedCities()
        {
            ICard card;
            for (var i = 3; i > 0; i--)
            {
                for(var c = 0;c<3;c++){
                    infectionDeck.Draw(out card);
                    var cityCard = card as CityCard;

                    Map.AddDiseasesToCity(cityCard.City, i, null, this);

                    infectionDiscardPile.AddCard(card);
                }
            }
        }

        /// <summary>
        /// Any final initialization that needs to be done.  
        /// </summary>
        private void FinalizeInitialization()
        {
            // Clear out the partial updates so they aren't sent on the first action of the game
            foreach (var player in players)
                player.GetPartialUpdate();

            Map.GetPartialUpdate();
        }

        /// <summary>
        /// Takes a list of disease colors and reduces the count of each color to 1
        /// if that disease is cured.
        /// </summary>
        /// <param name="diseases"></param>
        /// <returns></returns>
        private DiseaseColor[] ReduceDiseaseListByCures(DiseaseColor[] diseases)
        {
            int black = 0,
                red = 0,
                yellow = 0,
                blue = 0;

            foreach (var disease in diseases)
            {
                switch (disease)
                {
                    case DiseaseColor.Black:
                        black++;
                        break;

                    case DiseaseColor.Blue:
                        blue++;
                        break;

                    case DiseaseColor.Red:
                        red++;
                        break;

                    case DiseaseColor.Yellow:
                        yellow++;
                        break;
                }
            }

            if ((cures[DiseaseColor.Black] && black > 0)
                || (cures[DiseaseColor.Red] && red > 0)
                || (cures[DiseaseColor.Yellow] && yellow > 0)
                || (cures[DiseaseColor.Blue] && blue > 0))
            {
                var updatedDiseases = new List<DiseaseColor>();

                AddDiseasesToList(updatedDiseases, DiseaseColor.Red, red);
                AddDiseasesToList(updatedDiseases, DiseaseColor.Yellow, yellow);
                AddDiseasesToList(updatedDiseases, DiseaseColor.Blue, blue);
                AddDiseasesToList(updatedDiseases, DiseaseColor.Black, black);

                diseases = updatedDiseases.ToArray();

            }

            return diseases;
        }

        /// <summary>
        /// Given a list and a number of a certain color of a disease, adds either one or the 
        /// number provided of the given disease type to the list, based on whether that 
        /// disease is cured or not.
        /// </summary>
        /// <param name="diseases">The list to add diseases to.</param>
        /// <param name="color">The disease color.</param>
        /// <param name="colorCount">The number of diseases to add if the disease is not cured.</param>
        private void AddDiseasesToList(List<DiseaseColor> diseases, DiseaseColor color, int colorCount)
        {
            if (colorCount > 0)
            {
                if (cures[color])
                    colorCount = 1;

                for (var i = 0; i < colorCount; i++)
                    diseases.Add(color);
            }
        }


        /// <summary>
        /// Pandemic is broken down into 4 stages:
        /// PlayerMove - The game is waiting for the active player to issue an action
        /// Infect - The game is waiting for the active player to draw from the infect deck.
        /// Epidemic - The game is waiting for the active player to step through the epidemic steps.
        /// Finished - The game is over.
        /// </summary>
        private GameState gameState
        {
            get{
                return _gameState;
            }
            set
            {
                // Once the game is over the game state can no longer be changed.
                if (_gameState == GameState.Finished)
                    return;

                _gameState = value;
                if(partialUpdate != null)
                    partialUpdate.State = value;
            }
        }

        private GameStateData gameStateData
        {
            get
            {
                return _gameStateData;
            }
            set
            {
                if (_gameState == GameState.Finished
                    && !(value is FinishedData))
                    return;

                _gameStateData = value;
                if (partialUpdate != null)
                    partialUpdate.StateData = value;
            }
        }

        /// <summary>
        /// Index of the active player.
        /// Updates the Game Partail Update object when set.
        /// </summary>
        private int ActivePlayerIndex{
            get
            {
                return activePlayerIndex;
            }
            set
            {
                activePlayerIndex = value;
                if (partialUpdate != null)
                    partialUpdate.ActivePlayer = ActivePlayer;
            }
        }

        /// <summary>
        /// Number of actions remaining for the active player.
        /// Updates the Game Partial Update object when set.
        /// </summary>
        private int ActivePlayerActionsRemaining
        {
            get
            {
                return activePlayerActionsRemaining;
            }

            set{
                activePlayerActionsRemaining = value;
                if (partialUpdate != null)
                    partialUpdate.ActionsRemaining = value;
            }
        }



        /// <summary>
        /// List of the players
        /// </summary>
        private PandemicPlayer[] players;


        private Deck infectionDeck = new Deck();
        private Deck infectionDiscardPile = new Deck();

        private Deck playerDeck = new PlayerDeck();
        private Deck playerDiscardPile = new Deck();

        /// <summary>
        /// The number of epidemic cards to use
        /// </summary>
        private int difficulty = 4;

        private int activePlayerIndex;
        private int activePlayerActionsRemaining = 0;
        private int infectionRateCount = 0;
        private int outbreakCount = 0;

        private PartialGameUpdate partialUpdate;
        private GameState _gameState = GameState.PlayerMove;
        private GameStateData _gameStateData;

        private Dictionary<PlayerRole, PandemicPlayer> PlayersByRole = new Dictionary<PlayerRole, PandemicPlayer>();
        private Dictionary<string, PandemicPlayer> PlayersByName = new Dictionary<string, PandemicPlayer>();

        private Dictionary<DiseaseColor, bool> cures = new Dictionary<DiseaseColor, bool>();
        private Dictionary<DiseaseColor, bool> eradicatedDiseases = new Dictionary<DiseaseColor, bool>();

        /// <summary>
        /// Used to return requested cure values from the game
        /// </summary>
        private bool[] _cures = new bool[] { false, false, false, false };

        private bool oneQuietNight = false;

        private InterruptState interrupt = null;
    }
}