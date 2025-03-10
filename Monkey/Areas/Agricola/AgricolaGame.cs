﻿using BoardgamePlatform.Game;
using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.RoundActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using State = System.Collections.Immutable.ImmutableDictionary<string, object>;
using RoundActionState = System.Collections.Immutable.ImmutableDictionary<string, object>;
using WebGrease.Css.Extensions;

namespace Monkey.Games.Agricola
{
    public class AgricolaGame : GameBase<AgricolaHub>, IClientGameUpdate
    {

        public AgricolaGame(string name, string viewPath, int maxPlayers, Player[] players, Dictionary<string, object> props)
            :base(name, viewPath, maxPlayers, players, props)
        {

            if (players.Length > maxPlayers)
                throw new ArgumentException("Agricola supports no more than 5 players.");
            InitializeState();
            this.players = new AgricolaPlayer[players.Length];
            for (var i = 0; i < players.Length; i++)
            {
                players[i].GamePlayer = new AgricolaPlayer(this, players[i]);
                this.players[i] = (AgricolaPlayer)players[i].GamePlayer;
            }
            Random rng = new Random();
            StartingPlayerIndex = rng.Next(0, this.players.Length);

            for (var i = 0; i < this.players.Length; i++)
            {
                var player = this.players[i];

                player.AddResource(Resource.Food, i == StartingPlayerIndex ? 2 : 3);
                player.AddFamilyMember();
                player.AddFamilyMember();
                player.AddRoom(0, 1);
                player.AddRoom(0, 2);
                player.UpdateScoreCard();
            }

            bool familyMode;
            if(bool.TryParse(props["FamilyMode"].ToString(), out familyMode))
                FamilyMode = familyMode;

            Mode = GameMode.Work;

            roundsDeck = ShuffleRoundsDeck().ToImmutableArray();
            BuildActionList();

            if (!FamilyMode)
            {
                minorImprovementsDeck = Curator.MinorImprovementsDeck;
                occupationsDeck = Curator.OccupationsDeck;
            }
            masterDeck = LoadDecks().ToImmutableDictionary();

            if (!FamilyMode)
            {
                DealPlayerHands();
            }

            StartNextRound(new List < GameActionNotice >());

        }


        public Card[] GetMasterDeck()
        {
            return masterDeck.Values.ToArray();
        }

        /// <summary>
        /// Executes the requested anytime action id.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="actionId"></param>
        /// <param name="cardId"></param>
        /// <param name="param"></param>
        /// <param name="update"></param>
        /// <param name="notices"></param>
        /// <returns></returns>
        public bool TakeAnytimeAction(AgricolaPlayer player, Int32 actionId, int cardId, GameActionData param, out IClientGameUpdate update, out List<GameActionNotice> notices)
        {
            if (player.OwnedCardIds.Contains(cardId))
            {
                var card = GetCard(cardId);
                if (card.AnytimeAction?.Id == actionId)
                {
                    var action = card.AnytimeAction;
                    GameAction outAction;
                    if (action.TryExecute(player, param, out outAction))
                    {
                        UpdateScorecards();
                        notices = action.ResultingNotices;
                        CheckForInterrupt();

                        update = BuildPartialUpdate(player, action);
                        return true;
                    }
                }
            }
            update = null;
            notices = null;
            return false;
        }

        public bool TakeRoundAction(AgricolaPlayer player, Int32 actionId, GameActionData param, out IClientGameUpdate update, out List<GameActionNotice> notices)
        {
            if (Interrupt != null)
                return TakeInterruptAction(player, actionId, param, out update, out notices);

            var action = roundActions[roundActionIndices[actionId]];
            if (action != null)
            {
                if (Mode == GameMode.Work
                    && players[ActivePlayerIndex] == player)
                {
                    GameAction updatedAction;
                    action.SetState(RoundActionStates[action.Id]);
                    if (action.TryExecute(player, param, out updatedAction))
                    {
                        SetRoundActionState(updatedAction as RoundAction);
                        // This should no longer be necessary
                        roundActions = roundActions.SetItem(roundActionIndices[actionId], (RoundAction)updatedAction);

                        UpdateScorecards();
                        if (!CheckForInterrupt())
                        {
                            if (NextActivePlayer(action.ResultingNotices))
                            {
                                update = BuildPartialUpdate(player, action);
                            }
                            else
                            {
                                update = StartNextRound(action.ResultingNotices);
                            }
                        }
                        else
                        {
                            update = BuildPartialUpdate(player, action);
                        }
                        notices = action.ResultingNotices;

                        return true;
                    }
                }

            }
            update = null;
            notices = null;
            return false;
        }

        private PartialGameUpdate BuildPartialUpdate(AgricolaPlayer player, GameAction action)
        {
            var update = new PartialGameUpdate();
            ((PartialGameUpdate)update).ActivePlayerName = players[ActivePlayerIndex].Name;

            if(action is RoundAction)
            {

            }

            ((PartialGameUpdate)update).AddAction(action);
            foreach(var otherPlayers in player.Game.Players) 
                ((PartialGameUpdate)update).AddPlayer((AgricolaPlayer)otherPlayers);
            ((PartialGameUpdate)update).Interrupt = Interrupt;

            if (action is ImprovementAction || action is RenovationAction)
                ((PartialGameUpdate)update).AddMajorImprovementOwners(MajorImprovementOwners);

            if (newDelayedResourcesAdded)
            {
                newDelayedResourcesAdded = false;
                ((PartialGameUpdate)update).ReservedResources = ReservedResources;
            }

            if (action is StartingPlayerAction)
                ((PartialGameUpdate)update).StartingPlayerName = StartingPlayerName;

            return update;
        }

        private bool TakeInterruptAction(AgricolaPlayer player, Int32 actionId, GameActionData param, out IClientGameUpdate update, out List<GameActionNotice> notices)
        {
            if (player == Interrupt.Player)
            {
                var interrupt = this.Interrupt;
                GameAction action = interrupt;
                if (action != null)
                {
                    GameAction updatedAction;
                    if (action.TryExecute(player, param, out updatedAction))
                    {
                        this.Interrupt = null;
                        if (!CheckForInterrupt() && this.Mode == GameMode.Work )
                        {
                            if (NextActivePlayer(action.ResultingNotices))
                            {
                                update = BuildPartialUpdate(player, action);
                            }
                            else
                            {
                                update = StartNextRound(action.ResultingNotices);
                            }
                        }
                        else
                        {
                            update = BuildPartialUpdate(player, action);
                        }
                        notices = action.ResultingNotices;

                        return true;
                    }
                }

            }
            update = null;
            notices = null;
            return false;
        }

        /// <summary>
        /// Checks if the player can complete their harvest with the specified information
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CanCompleteHarvest(AgricolaPlayer player, HarvestData data)
        {
            if (!player.CanFeedFamily(data.FeedResources))
                return false;

            var sheepUsedAsFood = 0;
            var boarUsedAsFood = 0;
            var cattleUsedAsFood = 0;

            var availableConversions = Curator.GetHarvestFoodValues(player);
            foreach (var conversion in data.FeedResources)
            {
                var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == conversion.Id
                    && x.InType == conversion.InType && x.InAmount == conversion.InAmount
                    && x.OutType == conversion.OutType);
                if (conversionDefinition == null)
                    return false;

                if (conversionDefinition.InLimit.HasValue && conversionDefinition.InLimit.Value < conversion.Count / conversion.InAmount)
                    return false;

                if (conversionDefinition.InType == Resource.Sheep)
                    sheepUsedAsFood += conversion.Count * conversionDefinition.InAmount;
                else if (conversionDefinition.InType == Resource.Boar)
                    boarUsedAsFood += conversion.Count * conversionDefinition.InAmount;
                else if (conversionDefinition.InType == Resource.Cattle)
                    cattleUsedAsFood += conversion.Count * conversionDefinition.InAmount;
            }

            // Calculate how many animals are left over after feeding during harvest
            // We aren't concerned with cooked animals yet, as the only animals that can be passed in as cooked
            // are the baby animals
            var sheepAfterFeeding = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Sheep) - sheepUsedAsFood;
            var boarAfterFeeding = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Boar) - boarUsedAsFood;
            var cattleAfterFeeding = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Cattle) - cattleUsedAsFood;

            data.AnimalData.Free[AnimalResource.Sheep] += sheepUsedAsFood;
            data.AnimalData.Free[AnimalResource.Boar] += boarUsedAsFood;
            data.AnimalData.Free[AnimalResource.Cattle] += cattleUsedAsFood;

            // Figure out which animals will breed
            var newAnimals = new Dictionary<AnimalResource, int>();
            newAnimals[AnimalResource.Sheep] = sheepAfterFeeding >= 2 ? 1 : 0;
            newAnimals[AnimalResource.Boar] = boarAfterFeeding >= 2 ? 1 : 0;
            newAnimals[AnimalResource.Cattle] = cattleAfterFeeding >= 2 ? 1 : 0;
           
            if (!ActionService.CanAssignAnimals(player, data.AnimalData, newAnimals))
                return false;

            return true;
        }


        public bool CompleteHarvest(AgricolaPlayer player, HarvestData data, out IClientGameUpdate update, out List<GameActionNotice> notices)
        {
            if (Mode == GameMode.Harvest && player.Harvesting && CanCompleteHarvest(player, data))
            {
                var conversions = data.FeedResources;
                var feedNotice = new List<INoticePredicate>();
                int begAmount;
                player.FeedFamily(data.FeedResources, feedNotice, out begAmount);

                notices = new List<GameActionNotice>();
                ActionService.AssignAnimals(player, data.AnimalData, notices);

                UpdateScorecards();

                if(begAmount > 0)
                    feedNotice.Add(new IdPredicate(begAmount));

                notices.Add(new GameActionNotice(player.Name, NoticeVerb.Fed.ToString(), feedNotice));
                if (IsHarvestComplete())
                {
                    update = StartNextRound(notices);
                }
                else
                {
                    update = new PartialGameUpdate();
                    ((PartialGameUpdate)update).AddPlayer(player);
                }
                return true;
            }

            update = null;
            notices = null;
            return false;
        }


        /// <summary>
        /// Updates the active player to the next player in order
        /// who still has a family member at home.
        /// </summary>
        /// <returns>True if a player has a family member left, false if not.</returns>
        public bool NextActivePlayer(List<GameActionNotice> resultingNotices)
        {
            var oldActivePlayer = ActivePlayerIndex;
            do
            {
                ActivePlayerIndex = NextPlayerIndex;
                if (players[ActivePlayerIndex].FamilyAtHome > 0)
                {
                    resultingNotices.Add(new GameActionNotice(players[ActivePlayerIndex].Name, NoticeVerb.Turn.ToString()));
                    return true;
                }

            }
            while (oldActivePlayer != ActivePlayerIndex);
            return false;
        }

        public IClientGameUpdate StartNextRound(List<GameActionNotice> resultingNotices)
        {

            if (this.Mode == GameMode.Work 
                && (CurrentRound == 4 || CurrentRound == 7 || CurrentRound == 9 || CurrentRound == 11 || CurrentRound == 13 || CurrentRound == 14))
            {
                this.BeginHarvest(resultingNotices);
            }
            else
            {
                if (CurrentRound != 14)
                {
                    familyOnRounds = ImmutableDictionary<int, ImmutableList<AgricolaPlayer>>.Empty;

                    this.Mode = GameMode.Work;
                    CurrentRound++;

                    ActivePlayerIndex = StartingPlayerIndex;
                    resultingNotices.Add(new GameActionNotice(ActivePlayerName, NoticeVerb.Turn.ToString()));
                    foreach (var player in players)
                    {
                        player.ReturnFamilyHome();
                    }

                    AddRoundAction(resultingNotices);
                    UpdateActions();
                }
                else
                {
                    Mode = GameMode.Over;
                    UpdateScorecards();
                }
            }
            
            CheckForInterrupt();
            return this;
        }


        /// <summary>
        /// Assigns a major improvement to a player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player"></param>
        public void AssignMajorImprovement(int id, AgricolaPlayer player)
        {

            if (!majorImprovementOwners.Keys.Contains(id))
                throw new ArgumentException("Invalid major improvement id:" + id);

            var oldPlayer = majorImprovementOwners[id];
            if (oldPlayer != null)
            {
                oldPlayer.RemoveMajorImprovement(id);
            }

            majorImprovementOwners = majorImprovementOwners.SetItem(id, player);
            if(player != null)
                player.AddMajorImprovement(id);
        }

        /// <summary>
        /// Returns a card to its original source.  Major Improvements return to the major improvement deck,
        /// minor improvements and occupations are returned to their owners hand.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        public void ReturnCard(AgricolaPlayer player, int id)
        {
            if (!player.OwnsCard(id))
                return;

            var card = GetCard(id);
            if (card is MajorImprovement)
            {
                AssignMajorImprovement(id, null);
                player.RemoveMajorImprovement(id);
            }
            else
            {
                player.ReturnCard(card);
            }
        }
        public Card GetCard(CardId id)
        {
            return GetCard((int)id);
        }

        public Card GetCard(int id)
        {
            return masterDeck[id];
        }

        public void StoreDelayedResources(AgricolaPlayer player, ImmutableArray<DelayedResourceCache> delayedCaches)
        {
            newDelayedResourcesAdded = true;
            foreach(var cache in delayedCaches){
                var index = cache.OnRound 
                    ? cache.Delay - 1
                    : CurrentRound - 1 + cache.Delay;
                if (index < roundsDeck.Length && index >= CurrentRound)
                {
                    var action = roundsDeck[index];
                    //var state = GetRoundActionState(action);
                    action.AddDelayedResource(player, new ResourceCache(cache.Type, cache.Count));
                    SetRoundActionState(action);
                }
            }
        }

        public void PassCardLeft(AgricolaPlayer currentOwner, MinorImprovement card)
        {

            if (players.Length > 1)
            {
                var i = Array.IndexOf(players, currentOwner);
                i = i + 1 == players.Length ? 0 : i + 1;
                var leftPlayer = players[i];
                leftPlayer.AddCardToHand(card);

            }

            currentOwner.RemoveCardFromHand(card);
        }

        /// <summary>
        /// Adds an interrupt to the interrupt queue.
        /// These will take priority over the normal game flow,
        /// and will be executed sequentially until there are no
        /// more interrupts left in queue.
        /// </summary>
        /// <param name="interrupt"></param>
        public void AddInterrupt(InterruptAction interrupt)
        { 
            interrupts = interrupts.Push(interrupt);
        }

        /// <summary>
        /// Loads the next interrupt into the games interrupt field.
        /// </summary>
        /// <returns>True if an interrupt was found.</returns>
        public bool CheckForInterrupt()
        {
            if (this.Interrupt != null) return true;
            InterruptAction interrupt = null;
            if(!interrupts.IsEmpty)
                interrupts = interrupts.Pop(out interrupt);
            this.Interrupt = interrupt;
            return this.Interrupt != null;
        }


        /*
        public static Game Load(Guid Id)
        {
            return Load(Id.ToString());
        }

        public static Game Load(String Id)
        {
            return null;
        }

        public void Save()
        {
            var connection = DbUtils.GetConnection();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var command = DbUtils.GetCommand("save_game", connection, transaction);
                    command.Parameters.AddWithValue("game_id", Id.ToString());
                    command.Parameters.AddWithValue("game_name", Name);
                    command.Parameters.AddWithValue("in_progress", true);
                    command.Parameters.AddWithValue("starting_player", StartingPlayerIndex);
                    command.ExecuteNonQuery();

                    for(var i=0;i<players.Length;i++){
                        var p = players[i];   
                        command = DbUtils.GetCommand("save_game_player", connection, transaction);
                        command.Parameters.AddWithValue("game_id", Id.ToString());
                        command.Parameters.AddWithValue("player_id", Id.ToString());
                        command.Parameters.AddWithValue("ordering", i);
                        command.Parameters.AddWithValue("food", p.PersonalSupply.Food);
                        command.Parameters.AddWithValue("wood", p.PersonalSupply.Wood);
                        command.Parameters.AddWithValue("clay", p.PersonalSupply.Clay);
                        command.Parameters.AddWithValue("reed", p.PersonalSupply.Reed);
                        command.Parameters.AddWithValue("stone", p.PersonalSupply.Stone);
                        command.Parameters.AddWithValue("grain", p.PersonalSupply.Grain);
                        command.Parameters.AddWithValue("vegetables", p.PersonalSupply.Vegetables);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }
        }
        */

        public override string Title
        {
            get { return "Agricola";  }
        }


        /// <summary>
        /// This will be set during transmission to each player to only show
        /// that player their own hand.
        /// </summary>
        public object MyHand
        {
            get;
            set;
        }

        public bool IsOver
        {
            get { return Mode == GameMode.Over; }
        }

        public bool FamilyMode
        {
            get;
            private set;
        }


        public Dictionary<String, ResourceCache[]>[] ReservedResources
        {
            /**
             * TODO
             * This needs to pull from round action state 
             */ 
            get{

                var reserved = new Dictionary<String, ResourceCache[]>[roundsDeck.Length - CurrentRound];
                for(int i = CurrentRound, j=0;i<roundsDeck.Length;i++, j++){
                    var action = roundsDeck[i];
                    reserved[j] = action.DelayedResources;
                }
                return reserved;
            }
        }

        public string StartingPlayerName
        {
            get { return StartingPlayer.Name; }
        }

        [JsonIgnore]
        public AgricolaPlayer StartingPlayer
        {
            get { return players[StartingPlayerIndex];  }
            set { StartingPlayerIndex = Array.IndexOf(players, value); }
        }

        [JsonIgnore]
        public Int32 ActivePlayerIndex
        {
            get;
            private set;
        }

        public String ActivePlayerName
        {
            get { return players[ActivePlayerIndex].Name;  }
        }

        [JsonIgnore]
        public Int32 StartingPlayerIndex
        {
            get;
            private set;
        }

        public override GamePlayer[] Players
        {
            get { return players.Clone() as GamePlayer[]; }
        }

        public AgricolaPlayer[] AgricolaPlayers
        {
            get
            {
                return players.Clone() as AgricolaPlayer[];
            }
        }

        public int CurrentRound
        {
            get;
            private set;
        }

        [JsonIgnore]
        public int RoundsRemaining
        {
            get { return TotalRounds - CurrentRound; }
        }

        public GameAction[] Actions
        {
            get { return roundActions.ToArray(); }
        }

        public Dictionary<int, string> MajorImprovementOwners
        {
            get
            {
                return majorImprovementOwners.ToDictionary(x => x.Key, y => (y.Value == null ? null : y.Value.Name));
            }
        }

        [JsonIgnore]
        public int[] OwnedMajorImprovements
        {
            get
            {
                return majorImprovementOwners.Where(x => x.Value != null).Select(x => x.Key).ToArray();
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameMode Mode
        {
            get;
            private set;
        }

        public InterruptAction Interrupt
        {
            get;
            private set;
        }

        public override void SendPlayerGameStart(GamePlayer player)
        {
            if (!(player is AgricolaPlayer))
                throw new InvalidOperationException("Attempted to send game start for a player of a different game type.");

            if (!(player.Game is AgricolaGame))
                throw new InvalidOperationException("Attempted to send game start for a game of a different type.");

            var agricolaGame = player.Game as AgricolaGame;
            if (!agricolaGame.FamilyMode)
                agricolaGame.MyHand = ((AgricolaPlayer)player).Hand;

            var gameParams = new Dictionary<string, object>();
            gameParams["deck"] = agricolaGame.GetMasterDeck();
            HubContext.Clients.Client(player.Player.ConnectionId.ToString()).startingGame(player.Game, gameParams);

        }





        private bool IsHarvestComplete()
        {
            foreach (var player in players)
            {
                if (player.Harvesting)
                    return false;
            }
            return true;
        }

        private void BeginHarvest(List<GameActionNotice> notices)
        {
            this.Mode = GameMode.Harvest;



            var trigger = new FieldPhaseTrigger();
            foreach (var player in players)
            {
                player.HarvestFields(notices);

                var events = player.GetCardEventData(player, trigger);
                ActionService.ExecuteEvents(player, events, notices);
            }
        }

        /// <summary>
        /// Updates all action spaces at the start of each round
        /// </summary>
        private void UpdateActions()
        {
            foreach (var roundAction in roundActions)
            {
                var newState = roundAction.RoundStart(GetRoundActionState(roundAction));
                SetRoundActionState(roundAction);
            }
        }

        private RoundAction[] ShuffleRoundsDeck()
        {
            var roundsDeck = BuildRoundsDeck();

            var deck = new RoundAction[14];
            var startI = 0;
            var actionDeckIndex = 0;
            var rng = new Random();
            for (var stage = 0; stage < RoundsInStages.Length; stage++)
            {
                var roundsInCurrentStage = RoundsInStages[stage];
                var actions = new List<RoundAction>();
                for (var i = 0; i < roundsInCurrentStage; i++)
                {
                    actions.Add(roundsDeck[startI + i]);
                }

                while (actions.Count > 0)
                {
                    var ni = rng.Next(actions.Count);
                    var id = actions[ni];
                    actions.Remove(id);

                    deck[actionDeckIndex++] = id;
                }

                startI += roundsInCurrentStage;
            }
            return deck;
        }

        private void BuildMajorImprovements(Dictionary<int, Card> masterDeck)
        {
            var majors = Curator.MajorImprovementsDeck;
            foreach (var major in majors)
            {
                majorImprovementOwners = majorImprovementOwners.SetItem(major.Id, null);
                masterDeck[major.Id] = major;
            }

        }


        private Dictionary<int, Card> LoadDecks()
        {
            var masterDeck = new Dictionary<int, Card>();
            BuildMajorImprovements(masterDeck);

            if (!FamilyMode)
            {

                foreach (var card in minorImprovementsDeck)
                    masterDeck[card.Id] = card;

                foreach (var card in occupationsDeck)
                    masterDeck[card.Id] = card;
            }
            return masterDeck;
        }


        /// <summary>
        /// Build the list of all the actions in the game based on the number
        /// of players.
        /// </summary>
        private void BuildActionList()
        {
            BuildBasicActions();

            switch (players.Length)
            {
                case 3:
                    Build3PlayerActions();
                    break;
                case 4:
                    Build4PlayerActions();
                    break;
                case 5:
                    Build5PlayerActions();
                    break;
            }
        }

        /// <summary>
        /// Adds the games basic actions to the action list
        /// These are part of every game
        /// </summary>
        private void BuildBasicActions()
        {
            AddAction(new BuildRoomsAndStablesAction(this, 0));
            AddAction(new StartingPlayerAction(this, FamilyMode ? 51 : 1, FamilyMode));
            AddAction(new BasicTakeAction(this, 2, Resource.Grain, 1, new GameEventTrigger[] { new TakeGrainActionTrigger() }));
            AddAction(new PlowAndSowAction(this, 3, false));
            AddAction(FamilyMode ? (RoundAction)new BuildStableAndBakeAction(this, 54) : new Monkey.Games.Agricola.Actions.RoundActions.OccupationAction(this, 4));
            AddAction(FamilyMode ? (RoundAction)new BuildingResourcesAction(this, 55,BuildingResourcesActionMode.SingleResourceWithFood, new GameEventTrigger[] { new DayLaborerActionTrigger() })
                : new BasicTakeAction(this, 5, Resource.Food, 2, new GameEventTrigger[] { new DayLaborerActionTrigger() }));
            AddAction(new BasicCacheAction(this, 6, Resource.Wood, 3));
            AddAction(new BasicCacheAction(this, 7, Resource.Clay, 1));
            AddAction(new BasicCacheAction(this, 8, Resource.Reed, 1));
            AddAction(new BasicCacheAction(this, 9, Resource.Food, 1, new GameEventTrigger[]{ new FishingActionTrigger() }));
        }

        private void Build3PlayerActions()
        {
            AddAction(FamilyMode ? new BuildingResourcesAction(this, 60, BuildingResourcesActionMode.DoubleResource) : (RoundAction)new Monkey.Games.Agricola.Actions.RoundActions.OccupationAction(this, 10));
            AddAction(new BuildingResourcesAction(this, 11, BuildingResourcesActionMode.SingleResource));
            AddAction(new BasicCacheAction(this, 12, Resource.Wood, 2));
            AddAction(new BasicCacheAction(this, 13, Resource.Clay, 1));
        }

        private void Build4PlayerActions()
        {
            AddAction(FamilyMode ? new BuildingResourcesAction(this, 64, BuildingResourcesActionMode.DoubleResource) : (RoundAction)new Monkey.Games.Agricola.Actions.RoundActions.OccupationAction(this, 14));
            AddAction(new BasicTakeAction(this, 15, new ResourceCache[]{
                new ResourceCache(Resource.Reed, 1),
                new ResourceCache(Resource.Stone, 1),
                new ResourceCache(Resource.Food, 1)
            }));
            AddAction(new BasicCacheAction(this, 16, Resource.Wood, 2));
            AddAction(new BasicCacheAction(this, 17, Resource.Wood, 1));
            AddAction(new BasicCacheAction(this, 18, Resource.Food, 1, new GameEventTrigger[] { new TravelingPlayersActionTrigger() }));
            AddAction(new BasicCacheAction(this, 19, Resource.Clay, 2));
        }
        private void Build5PlayerActions()
        {
            AddAction(FamilyMode ? new BuildingResourcesAction(this, 70, BuildingResourcesActionMode.DoubleResourceOrFamilyGrowth) : (RoundAction)new Monkey.Games.Agricola.Actions.RoundActions.OccupationAction(this, 20, true));
            AddAction(new BuildRoomOrTravelingPlayersAction(this, 21, new GameEventTrigger[] { new TravelingPlayersActionTrigger() }));
            AddAction(new AnimalChoiceAction(this, 22));
            AddAction(new BasicCacheAction(this, 23, Resource.Wood, 4));
            AddAction(new BasicCacheAction(this, 24, Resource.Clay, 3));
            AddAction(new TakeCacheComboAction(this, 25, Resource.Reed, 1, new ResourceCache[] {
                new ResourceCache(Resource.Stone, 1),
                new ResourceCache(Resource.Wood, 1)
            }));
        }

        private RoundAction[] BuildRoundsDeck()
        {
            var roundsDeck = new RoundAction[14];
            roundsDeck[0] = new BuildFencesAction(this, 100);
            roundsDeck[1] = new AnimalCacheAction(this, 101, Resource.Sheep, new GameEventTrigger[]{ new TakeAnimalActionTrigger( AnimalResource.Sheep) });
            roundsDeck[2] = new ImprovementAction(this, 102, true, true);
            roundsDeck[3] = new SowAndBakeAction(this, 103);
            roundsDeck[4] = new FamilyGrowthAction(this, 104, FamilyGrowthActionMode.Improvement);
            roundsDeck[5] = new BasicCacheAction(this, 105, Resource.Stone, 1, new GameEventTrigger[] { new TakeStoneActionTrigger() } );
            roundsDeck[6] = new RenovationAction(this, 106, RenovationActionMode.Improvement);
            roundsDeck[7] = new AnimalCacheAction(this, 107, Resource.Boar, new GameEventTrigger[] { new TakeAnimalActionTrigger(AnimalResource.Boar) });
            roundsDeck[8] = new BasicTakeAction(this, 108, Resource.Vegetables, 1, new GameEventTrigger[] { new TakeVegetablesActionTrigger() });
            roundsDeck[9] = new AnimalCacheAction(this, 109, Resource.Cattle, new GameEventTrigger[] { new TakeAnimalActionTrigger(AnimalResource.Cattle) });
            roundsDeck[10] = new BasicCacheAction(this, 110, Resource.Stone, 1, new GameEventTrigger[] { new TakeStoneActionTrigger() });
            roundsDeck[11] = new FamilyGrowthAction(this, 111, FamilyGrowthActionMode.WithoutSpace);
            roundsDeck[12] = new PlowAndSowAction(this, 112, true);
            roundsDeck[13] = new RenovationAction(this, 113, RenovationActionMode.Fences);
            return roundsDeck;
        }


        private void DealPlayerHands()
        {
            var minors = minorImprovementsDeck.ToList();
            var allOccupations = occupationsDeck.ToList();
            var occupations = allOccupations.Where(card => card.MinPlayers <= players.Length).ToList();
            var rng = new Random();

            foreach(var player in players)
            {

                if (true && players.Length <= 2)
                {
                    // debug

                    if (minors.Count > 0)
                    {
                        player.AddCardToHand(minors.Last());
                        minors.Remove(minors.Last());
                    }

                    // debug
                    if (allOccupations.Count > 0)
                    {
                        var occ = allOccupations.Last();
                        player.AddCardToHand(occ);
                        allOccupations.Remove(occ);

                        if (occupations.Contains(occ))
                            occupations.Remove(occ);
                    }

                    player.AddCardToHand(GetCard(CardId.Maid));
                    player.AddCardToHand(GetCard(CardId.Renovator));
                    //player.AddCardToHand(GetCard(CardId.Basket)); 
                    //player.AddCardToHand(GetCard(CardId.CrookedPlow));
                    //player.AddCardToHand(GetCard(160));  // Farmer
                    //player.AddCardToHand(GetCard(2038));  // Field Watchman
                    //player.AddCardToHand(GetCard(62)); // Turnwrest Plow
                    //player.AddCardToHand(GetCard(119)); // Turnwrest Plow
                }


                for (var i=0;i<StartingHandSize; i++){
                    if (minors.Count > 0)
                    {
                        var index = rng.Next(0,minors.Count);
                        player.AddCardToHand(minors[index]);
                        minors.RemoveAt(index);
                    }

                    if (occupations.Count > 0)
                    {
                        var index = rng.Next(0, occupations.Count);
                        player.AddCardToHand(occupations[index]);
                        occupations.RemoveAt(index);
                    }
                }
            }
        }

        private void UpdateScorecards()
        {
            //players.ForEach(p => p.UpdateScoreCard());
            foreach(var player in players){
                player.UpdateScoreCard();
            }
        }

        private void AddRoundAction(List<GameActionNotice> resultingNotices)
        {
            var action = roundsDeck[CurrentRound - 1];
            AddAction(action);
            var roundActionState = RoundActionStates[action.Id];
            action.DistributeDelayedResources(resultingNotices);
            SetRoundActionState(action);
        }

        private void AddAction(RoundAction action){
            if (roundActions.FirstOrDefault(x => x.Id == action.Id) != null)
                throw new InvalidOperationException("An action with id " + action.Id + " has already been registered with game " + this.Id);

            roundActions = roundActions.Add(action);
            roundActionIndices[action.Id] = roundActions.Count-1;
            SetRoundActionState(action);
        }

        /// <summary>
        /// Gets the index of the next player
        /// </summary>
        private Int32 NextPlayerIndex
        {
            get
            {
                var nextPlayerIndex = ActivePlayerIndex + 1;
                return nextPlayerIndex >= players.Length ? 0 : nextPlayerIndex;
            }
        }

        public const int TotalRounds = 14;

        public const int MaxFamilySize = 5;

        public const int StartingHandSize = 7;

        private void InitializeState()
        {
            State = State.SetItem(StateKeyInterrupts, ImmutableStack<InterruptAction>.Empty);
            State = State.SetItem(StateKeyMajorImprovementOwners, ImmutableDictionary<int, AgricolaPlayer>.Empty);
            State = State.SetItem(StateKeyRoundActions, ImmutableList<RoundAction>.Empty);
            State = State.SetItem(StateKeyRoundActionStates, ImmutableDictionary<int, RoundActionState>.Empty);
        }

        private void addFamilyOnRound(RoundAction action, AgricolaPlayer player)
        {
            ImmutableList<AgricolaPlayer> familyMembers;
            if(!familyOnRounds.TryGetValue(action.Id, out familyMembers))
            {
                familyMembers = ImmutableList<AgricolaPlayer>.Empty;
            }
            familyMembers = familyMembers.Add(player);
            familyOnRounds = familyOnRounds.SetItem(action.Id, familyMembers);
        }

        private ImmutableDictionary<int, RoundActionState> RoundActionStates
        {
            get
            {
                return (ImmutableDictionary<int, RoundActionState>)State[StateKeyRoundActionStates];
            }
            set
            {
                State = State.SetItem(StateKeyRoundActionStates, value);
            }
        }

        private RoundActionState GetRoundActionState(RoundAction roundAction)
        {
            var stateKeyRoundActionStates = ((ImmutableDictionary<int, RoundActionState>)State[StateKeyRoundActionStates]);
            if (stateKeyRoundActionStates.Keys.Contains(roundAction.Id))
                return stateKeyRoundActionStates[roundAction.Id];
            else
                return null;
        }

        private void SetRoundActionState(RoundAction roundAction)
        {
            SetRoundActionState(roundAction.Id, roundAction.State);
        }

        private void SetRoundActionState(int roundActionId, State state)
        {
            RoundActionStates = RoundActionStates.SetItem(roundActionId, state);
        }
        

        private State State = State.Empty;

        private const string StateKeyInterrupts = "StateKeyInterrupts";
        private const string StateKeyMajorImprovementOwners = "StateKeyMajorImprovementOwners";
        private const string StateKeyRoundActions = "StateKeyRoundActions";
        private const string StateKeyPlayerStates = "StateKeyPlayerStates";
        private const string StateKeyFamilyOnRounds = "StateKeyFamilyOnRounds";
        private const string StateKeyRoundActionStates = "stateKeyRoundActionStates";

        /// <summary>
        /// Family pieces on rounds
        /// </summary>
        private ImmutableDictionary<int, ImmutableList<AgricolaPlayer>> familyOnRounds
        {
            get
            {
                return (ImmutableDictionary<int, ImmutableList<AgricolaPlayer>>) State[StateKeyFamilyOnRounds];
            }
            set
            {
                State = State.SetItem(StateKeyFamilyOnRounds, value);
            }
        }

        /// <summary>
        /// Player states
        /// </summary>
        private ImmutableDictionary<AgricolaPlayer, State> playerStates
        {
            get
            {
                return (ImmutableDictionary<AgricolaPlayer, State>)State[StateKeyPlayerStates];
            }
            set
            {
                State = State.SetItem(StateKeyPlayerStates, value);
            }
        }

        /// <summary>
        /// The list of round actions
        /// </summary>
        private ImmutableList<RoundAction> roundActions
        {
            get
            {
                return (ImmutableList<RoundAction>)State[StateKeyRoundActions];
            }
            set
            {
                State = State.SetItem(StateKeyRoundActions, value);
            }
        }

        /// <summary>
        /// Mapping of major improvements to the players that own them
        /// </summary>
        private ImmutableDictionary<int, AgricolaPlayer> majorImprovementOwners
        {
            get
            {
                return (ImmutableDictionary<int, AgricolaPlayer>)State[StateKeyMajorImprovementOwners];
            }
            set
            {
                State = State.SetItem(StateKeyMajorImprovementOwners, value);
            }
        }

        /// <summary>
        /// The interrupt stack
        /// </summary>
        private ImmutableStack<InterruptAction> interrupts
        {
            get
            {
                return (ImmutableStack<InterruptAction>)State[StateKeyInterrupts];
            }
            set
            {
                State = State.SetItem(StateKeyInterrupts, value);
            }
        }

        /// <summary>
        /// Indicates reserved resources have been updated and need to be sent to the client
        /// </summary>
        private bool newDelayedResourcesAdded = false;

        /// <summary>
        /// Definition of cards per Stage
        /// </summary>
        private static readonly ImmutableArray<int> RoundsInStages = ImmutableArray.Create<int>(new int[] { 4, 3, 2, 2, 2, 1 });

        /// <summary>
        /// Players taking part in this game
        /// </summary>
        private AgricolaPlayer[] players { get; }

        /// <summary>
        /// A list of all cards involved in this game (major improvements, and all cards in players hands)
        /// </summary>
        private ImmutableDictionary<int, Card> masterDeck { get; }

        /// <summary>
        ///  The deck of rounds 
        /// </summary>
        private ImmutableArray<RoundAction> roundsDeck { get; }

        /// <summary>
        /// Deck of Minor Improvements being used
        /// </summary>
        private ImmutableArray<MinorImprovement> minorImprovementsDeck { get; }

        /// <summary>
        /// Deck of Occupations being used
        /// </summary>
        private ImmutableArray<Occupation> occupationsDeck { get; }


        /// <summary>
        /// ID to index mapping for round actions
        /// </summary>
        private Dictionary<int, int> roundActionIndices = new Dictionary<int, int>();

    }
}