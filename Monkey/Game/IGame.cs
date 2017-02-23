using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public interface IGame<out R>
    {

        GamePlayer[] Players
        {
            get;
        }

        void SendPlayerGameStart(GamePlayer player);

        /// <summary>
        /// Path to the view for the game
        /// </summary>
        string ViewPath
        {
            get;
        }

        /// <summary>
        /// Display title for the game
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Unique Identifier for the game
        /// </summary>
        Guid Id
        {
            get;
        }

                
    }
}