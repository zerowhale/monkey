using BoardgamePlatform.Game;
using Monkey.Games.Catan.Cards;
using Monkey.Games.Catan.ClientState;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Catan
{
    public class CatanPlayer: GamePlayer
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CatanPlayer(CatanGame game, Player player)
            : base((IGame<GameHub>)game, player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("New Catan Player [{0}] [{1}]", player.Name, this.Color);

            playerUpdate = new PartialPlayerUpdate(Name);
        }

        /// <summary>
        /// Gets the players partial update object, containing all the information
        /// that has changed since the last update was requested.
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public PartialPlayerUpdate GetPartialUpdate(bool reset = true)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Get Partial Update");

            var update = playerUpdate;
            if(reset)
                playerUpdate = new PartialPlayerUpdate(Name);
            return update;
        }

        public bool HasUpdate()
        {
            return playerUpdate.Dirty;
        }
        
        /// <summary>
        /// Adds a card to the players hand.
        /// </summary>
        /// <param name="card">The card to add.</param>
        public void AddCardToHand(ICard card)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Add Card To Hand [{0}], Card:[{1}]", Name, card.Name);

            hand.Add(card);
            playerUpdate.AddCard(card);
        }

        public ICard RemoveCardFromHand(ICard card)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Remove Card From Hand [{0}], Card:[{1}]", Name, card.Name);

            if (hand.Contains(card))
            {
                hand.Remove(card);
                playerUpdate.RemoveCard(card);
                return card;
            }
            return null;
        }

        public ICard GetCardById(int id)
        {
            return hand.Where(x => x.Id == id).FirstOrDefault();
        }

        public ICard RemoveCardFromHandById(int id) 
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Remove Card From Hand By Id [{0}], Card:[{1}]", Name, id);

            ICard card = null;
            foreach (var c in hand)
            {
                if (c.Id == id)
                {
                    card = c;
                    break;
                }
            }

            if (card != null)
                return RemoveCardFromHand(card);
            return null;
        }

        [JsonProperty(PropertyName="Hand")]
        public int[] HandIds
        {
            get
            {
                return hand.Select(x => x.Id).ToArray();
            }
        }

        public int HandSize
        {
            get { return hand.Count(); }
        }

        /// <summary>
        /// Copy of the players hand.
        /// </summary>
        [JsonIgnore]
        public ICard[] Hand
        {
            get
            {
                return hand.ToArray();
            }
        }

        /// <summary>
        /// The players hand.
        /// </summary>
        private  List<ICard> hand = new List<ICard>();

        private PartialPlayerUpdate playerUpdate;

    }
}