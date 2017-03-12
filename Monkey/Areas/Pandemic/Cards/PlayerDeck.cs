using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Cards
{
    public class PlayerDeck: Deck
    {

        /// <summary>
        /// Splits the deck into roughly even piles and adds an epidemic card
        /// to each.  Then each pile is shuffled and combined together into the 
        /// player deck.
        /// </summary>
        /// <param name="count"></param>
        public void ShuffleInEpidemicCards(int count)
        {
            this.Shuffle();

            var decks = new List<Deck>();
            for (var i = 0; i < count; i++)
            {
                var deck = new Deck();
                deck.AddCard(new EpidemicCard());
                decks.Add(deck);
            }

            var intoIndex = 0;
            ICard card;
            while (Draw(out card))
            {
                decks[intoIndex].AddCard(card);
                intoIndex++;
                if (intoIndex >= decks.Count())
                    intoIndex = 0;
            }

            // Order is important, largest stacks are first in the list
            // so we need to add them backwards (so the largest decks end up on top).
            for (var d = decks.Count() - 1; d >= 0; d--)
            {
                var deck = decks[d];
                deck.Shuffle();
                while (deck.Draw(out card))
                    AddCard(card);
            }

        }
        
    }
}