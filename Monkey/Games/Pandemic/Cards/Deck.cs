using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Cards
{
    public class Deck
    {
        public void AddCard(ICard card)
        {
            cards.Add(card);
        }

        public void AddDeck(Deck deck)
        {
            ICard card;
            while (deck.Draw(out card))
            {
                AddCard(card);
            }
        }

        /// <summary>
        /// Checks if a card with the given ID is in the deck.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasCard(int id)
        {
            foreach (var card in cards)
            {
                if (card.Id == id)
                    return true;
            }
            return false;
        }

        public void RemoveCardById(int id)
        {
            ICard toRemove = null;
            foreach (var card in cards)
            {
                if (card.Id == id)
                {
                    toRemove = card;
                    break;
                }
            }
            if (toRemove != null)
                cards.Remove(toRemove);
        }

        /// <summary>
        /// Does a standard randomization of the deck
        /// </summary>
        public void Shuffle()
        {
            var remaining = cards.ToList();
            cards.Clear();
            while (remaining.Count() > 0)
            {
                var i = random.Next(remaining.Count());
                cards.Add(remaining[i]);
                remaining.RemoveAt(i);
            }
        }

        /// <summary>
        /// Attempts to draw the top card from the deck.
        /// </summary>
        /// <param name="card">The drawn card, or null if no card could be drawn.</param>
        /// <returns>True if a card was drawn, false if the deck was empty</returns>
        public bool DrawFromBottom(out ICard card)
        {
            if(cards.Count() > 0){
                card = cards[0];
                cards.RemoveAt(0);
                return true;
            }
            card = null;
            return false;                
        }

        /// <summary>
        /// Attempts to draw the card at the bottom of the deck.
        /// </summary>
        /// <param name="card">The drawn card, or null if the decks was empty.</param>
        /// <returns>True if a card was drawn, false if the deck was empty.</returns>
        public bool Draw(out ICard card)
        {
            if (cards.Count() > 0)
            {
                var top = cards.Count - 1;
                card = cards[top];
                cards.RemoveAt(top);
                return true;
            }
            card = null;
            return false;                

        }

        /// <summary>
        /// Returns a number of cards from the top of the deck without
        /// modifying the content of the deck.
        /// </summary>
        /// <param name="count">The number of cards to return.</param>
        /// <returns>A list of cards representing the top of the deck.</returns>
        public ICard[] Peek(int count)
        {
            var cards = new List<ICard>();
            for (var i = 0; i < count; i++)
            {
                cards.Add(this.cards[i]);
            }
            return cards.ToArray();
        }

        /// <summary>
        /// Returns the deck of cards in order as an array.
        /// The first card is the top of the deck, the last card is the bottom.
        /// </summary>
        /// <returns></returns>
        public ICard[] GetDeckList()
        {
            return cards.ToArray();
        }

        /// <summary>
        /// Number of cards in the deck.
        /// </summary>
        public int Count
        {
            get { return cards.Count(); }
        }

        /// <summary>
        /// Backing list of cards in the deck.
        /// </summary>
        private List<ICard> cards = new List<ICard>();

        /// <summary>
        /// Random
        /// </summary>
        private static Random random = new Random();
    }
}