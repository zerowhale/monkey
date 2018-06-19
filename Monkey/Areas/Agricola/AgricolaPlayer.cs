using BoardgamePlatform.Game;
using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.AnytimeActions;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Farm;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public class AgricolaPlayer : GamePlayer
    {

        public AgricolaPlayer(AgricolaGame game, Player player)
            :base((IGame<GameHub>)game, player)
        {
            PersonalSupply = new PersonalSupply();
            Farmyard = new Farmyard(this);
            if (!game.FamilyMode)
            {
                HandMinors = new List<Card>();
                HandOccupations = new List<Card>();
            }
        }


        public void AddFamilyMember()
        {
            this.familySize++;
            this.NumBabies++;
        }

        public void AddMajorImprovement(int id)
        {
            majorImprovements.Add(id);
            ownedCards.Add(Curator.GetMajorImprovement(id));
        }

        public void RemoveMajorImprovement(int id)
        {
            majorImprovements.Remove(id);
            ownedCards.Remove(Curator.GetMajorImprovement(id));
        }


        public void ReturnFamilyHome()
        {
            FamilyAtHome = FamilySize;
            NumBabies = 0;
        }

        public Boolean HasFamilyMemberAvailable()
        {
            return FamilyAtHome > 0;
        }

        public Boolean UseFamilyMember()
        {
            if (FamilyAtHome > 0)
            {
                FamilyAtHome--;
                return true;
            }
            return false;
        }


        public void PlayCard(int id)
        {
            var card = ((AgricolaGame)Game).GetCard(id);
            PlayCard(card);
        }

        public void PlayCard(Card card)
        {
            if (HandOccupations.Contains(card))
                HandOccupations.Remove(card);
            else if (HandMinors.Contains(card))
                HandMinors.Remove(card);
            else return;

            ownedCards.Add(card);
        }

        public void ReturnCard(Card card)
        {
            if (this.OwnsCard(card.Id))
            {
                ownedCards.Remove(card);
            }
        }

        /// <summary>
        /// This is probably not the correct place for this method.  This does not have anything to
        /// do with game rules, but is a mechanism for getting event data from game objects.
        /// </summary>
        /// <param name="resolvingPlayer"></param>
        /// <param name="triggeringPlayer"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public List<TriggeredEvent> GetCardEventData(AgricolaPlayer triggeringPlayer, GameEventTrigger trigger)
        {
            var resolvingPlayer = (AgricolaPlayer)this;
            var events = new List<TriggeredEvent>();
            foreach (var card in resolvingPlayer.OwnedCards)
            {
                if (card.Events == null)
                    continue;

                var cardEvents = card.Events.Where(x => x.Triggers.Any(s => s.Triggered(resolvingPlayer, triggeringPlayer, trigger)));
                foreach (var cardEvent in cardEvents)
                {
                    cardEvent.ActiveTrigger = trigger;
                    cardEvent.OwningCard = card;
                }
                events.AddRange(cardEvents);
            }

            return events;
        }

        public void HarvestFields(List<GameActionNotice> notices)
        {
            var resources = this.Farmyard.HarvestFields();
            foreach (var cache in resources)
            {
                this.PersonalSupply.AddResource(cache);
            }
            notices.Add(new GameActionNotice(this.Name, NoticeVerb.Harvested.ToString(), resources.ToList<INoticePredicate>()));
            Harvesting = true;
        }

        /// <summary>
        /// Checks if this player can afford all the costs in the cost
        /// list.  The cost list is assumed to not contain duplicate
        /// resource types, as this method does not consolidate them
        /// before checking if the personal supply has sufficent funds.
        /// </summary>
        /// <param name="costs">A list of ResourceCache objects representing 
        /// the totaled costs of some purchase</param>
        /// <returns></returns>
        public bool CanAfford(ResourceCache[] costs)
        {
            foreach (var cache in costs)
            {
                if (this.PersonalSupply.GetResource(cache.Type) < cache.Count)
                    return false;
            }
            return true;
        }



        /// <summary>
        /// Deducts the costs in the cost list from this players personal supply
        /// </summary>
        /// <param name="costs"></param>
        public void PayCosts(ResourceCache[] costs)
        {
            foreach (var cache in costs)
            {
                this.PersonalSupply.AddResource(cache.Type, -cache.Count);
            }
        }

        public bool CanFeedFamily(ResourceConversionData[] data)
        {
            var availableConversions = Curator.GetHarvestFoodValues(this);

            foreach (var conversion in data)
            {
                var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == conversion.Id 
                    && x.InType == conversion.InType && x.InAmount == conversion.InAmount
                    && x.OutType == conversion.OutType);

                if(conversionDefinition == null)
                    return false;

                // Invalid input amount
                if (conversion.Count % conversionDefinition.InAmount != 0)
                    return false;

                if (Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                {
                    var owned = this.Farmyard.GetAnimalCount((AnimalResource)Enum.Parse(typeof(AnimalResource), conversionDefinition.InType.ToString()));
                    if (owned < conversion.Count)
                        return false;
                }
                else
                {
                    if (PersonalSupply.GetResource(conversionDefinition.InType) < conversion.Count)
                        return false;
                }


            }

            return true;
        }

        /// <summary>
        /// Feeds the players family with the given resources.
        /// Any shortage adds begging cards to the player.
        /// </summary>
        /// <param name="data">The list of resources with which to feed your family.</param>
        /// <returns>The number of new begger cards</returns>
        public int FeedFamily(ResourceConversionData[] data, List<INoticePredicate> notices)
        {
            var availableConversions = Curator.GetHarvestFoodValues(this);
            var foodValue = 0;
            var begTotal = 0;
            var foodNeeded = FamilySize * 2 - NumBabies;

            foreach (var conversion in data)
            {
                var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == conversion.Id
                    && x.InType == conversion.InType && x.InAmount == conversion.InAmount
                    && x.OutType == conversion.OutType);
                if (conversionDefinition.OutType == Resource.Food)
                {
                    foodValue += (conversion.Count / conversionDefinition.InAmount) * conversionDefinition.OutAmount;

                    notices.Add(new ResourceCache(conversionDefinition.InType, conversion.Count));

                    if (!Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                        PersonalSupply.AddResource(conversionDefinition.InType, -conversion.Count);
                }
                else
                {
                    if (!Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                        PersonalSupply.AddResource(conversionDefinition.InType, -conversion.Count);

                    // This does not handle what happens if an item can be converted into animals. 
                    // Not sure this actually exists in the game.
                    PersonalSupply.AddResource(conversionDefinition.OutType, (conversion.Count / conversionDefinition.InAmount) * conversionDefinition.OutAmount);
                }
            }

            if (foodValue < foodNeeded)
            {
                begTotal = foodNeeded - foodValue;
                BeggingCards += begTotal;
            }

            var remainder = foodValue - foodNeeded;
            if (remainder > 0)
                PersonalSupply.AddResource(Resource.Food, remainder);

            Harvesting = false;
            return begTotal;
        }

        public bool OwnsCard(int id)
        {
            return this.OwnedCardIds.Contains(id);
        }

        public void UpdateScoreCard()
        {
            this.ScoreCard = new ScoreCard(this);
        }

        public int[] MajorImprovements
        {
            get { return majorImprovements.ToArray(); }
        }

        public bool Harvesting
        {
            get;
            private set;
        }

        public int FamilyAtHome
        {
            get;
            private set;
        }

        public int FamilySize
        {
            get
            {
                return familySize;
            }
        }

        public int NumBabies
        {
            get;
            private set;
        }

        public ScoreCard ScoreCard
        {
            get;
            private set;
        }


        [JsonIgnore]
        public Card[] OwnedCards
        {
            get { return ownedCards.ToArray(); }
        }

        public int[] OwnedCardIds
        {
            get { return ownedCards.Select(x => x.Id).ToArray(); }
        }


        public PersonalSupply PersonalSupply
        {
            get;
            private set;
        }

        public Farmyard Farmyard
        {
            get;
            private set;
        }

        public int BeggingCards
        {
            get;
            private set;
        }

        [JsonIgnore]
        public Object Hand
        {
            get
            {
                return new
                {
                    Minors = HandMinors.Select(x => x.Id).ToArray(),
                    Occupations = HandOccupations.Select(x => x.Id).ToArray()
                };
            }
        }

        [JsonIgnore]
        public List<Card> HandMinors
        {
            get;
            private set;
        }

        [JsonIgnore]
        public List<Card> HandOccupations
        {
            get;
            private set;
        }


        private List<int> majorImprovements = new List<int>();
        private List<Card> ownedCards = new List<Card>();
        private int familySize;

    }
}