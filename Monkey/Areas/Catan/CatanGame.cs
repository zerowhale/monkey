using Microsoft.AspNet.SignalR;
using BoardgamePlatform.Game;
using BoardgamePlatform.Game.Notification;
using Monkey.Games.Catan.ClientState;
using Monkey.Games.Catan.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Catan
{
    public class CatanGame: GameBase<CatanHub>, IGameUpdate
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CatanGame(string name, string viewPath, int maxPlayers, Player[] players, Dictionary<string, object> props)
            : base(name, viewPath, maxPlayers, players, props)
        {

            if (logger.IsInfoEnabled)
                logger.InfoFormat("Catan Game Initialized. Name: [{0}]", name);
            
            InitializePlayers(players);
            
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
            get { return "Settlers of Catan"; }
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

        private GameState gameState
        {
            get
            {
                return _gameState;
            }
            set
            {
                // Once the game is over the game state can no longer be changed.
                if (_gameState == GameState.Finished)
                    return;

                _gameState = value;
                if (partialUpdate != null)
                    partialUpdate.State = value;
            }
        }

        private void Broadcast(Object obj)
        {
            HubContext.Clients.Group(Id.ToString()).message(obj);
        }

        private bool IsActivePlayer(CatanPlayer player)
        {
            return players[activePlayerIndex] == player;
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

            return gameUpdate;
        }
        

        /// <summary>
        /// Determines who the first player is based on which player
        /// has the highest population city in hand
        /// </summary>
        private void DetermineStartingPlayer(){

            var player = players[random.Next(players.Length - 1)];

            StartTurn(player);
        }

        private CatanPlayer GetNextPlayer()
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
        private void StartTurn(CatanPlayer player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Start turn [{0}].", player.Name);

            ActivePlayerIndex = Array.IndexOf(players, player);

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
        /// Sets up the Pandemic player objects
        /// </summary>
        /// <param name="players"></param>
        private void InitializePlayers(Player[] players)
        {
            this.players = new CatanPlayer[players.Length];

            var random = new Random();
            for (var i = 0; i < players.Length; i++)
            {
                players[i].GamePlayer = new CatanPlayer(this, players[i]);
                this.players[i] = (CatanPlayer)players[i].GamePlayer;
            }

            DetermineStartingPlayer();
        }

        /// <summary>
        /// List of the players
        /// </summary>
        private CatanPlayer[] players;

        private int activePlayerIndex;

        private static Random random = new Random();

        private PartialGameUpdate partialUpdate;

        private GameState _gameState;
     
    }
}