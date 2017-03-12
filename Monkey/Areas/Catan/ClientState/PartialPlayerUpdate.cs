using Monkey.Games.Catan.Cards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Catan.ClientState
{
    public class PartialPlayerUpdate
    {
        public PartialPlayerUpdate(string name)
        {
            Name = name;
            Dirty = false;
        }

        [JsonIgnore]
        public bool Dirty
        {
            get;
            private set;
        }

        /// <summary>
        /// The player name. This is required to identify the player on the client.
        /// </summary>
        public string Name
        {
            get;
            private set;
         }

        /// <summary>
        /// Adds a card to the Cards-added-to-hand list
        /// </summary>
        /// <param name="card">The card added to the players hand</param>
        public void AddCard(ICard card)
        {
            if (addCards == null)
                addCards = new List<ICard>();
            addCards.Add(card);
            Dirty = true;
        }

        /// <summary>
        /// Adds a card to the Cards-removed-from-hand list
        /// </summary>
        /// <param name="card">The card removed from the players hand</param>
        public void RemoveCard(ICard card)
        {
            if (removeCards == null)
                removeCards = new List<ICard>();
            removeCards.Add(card);
            Dirty = true;
        }

        /// <summary>
        /// The cards that have been added to the players hand since last update
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] AddCards
        {
            get { return addCards == null ? null : addCards.Select(x => x.Id).ToArray(); }
        }

        /// <summary>
        /// The cards that have been removed from the players hand since last update
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] RemoveCards
        {
            get 
            { 
                return removeCards == null ? null : removeCards.Select(x => x.Id).ToArray(); 
            }
        }

        private List<ICard> addCards;
        private List<ICard> removeCards;
    }
}