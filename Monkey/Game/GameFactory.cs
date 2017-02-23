using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public static class GameFactory
    {
        /// <summary>
        /// Creates an instance of a game of the provided type.
        /// Additional game values are loaded from the game manifest at this point. 
        /// </summary>
        /// <param name="gameType">The type of the game to create.</param>
        /// <param name="name">The player specified name of the game.</param>
        /// <param name="players">List of players in the game.</param>
        /// <param name="props">Custom properties the game needs.</param>
        /// <returns></returns>
        public static IGame<GameHub> CreateGame(string gameType, string name, Player[] players, Dictionary<string, object> props){
            var manifest = GameRegistry.GetRegisteredGame(gameType);

            Object[] parameters = new Object[] { name, manifest.ViewPath, manifest.MaxPlayers, players, props };
            var obj = (IGame<GameHub>)Activator.CreateInstance(manifest.Type, parameters);
            return obj;
        }

    }
}