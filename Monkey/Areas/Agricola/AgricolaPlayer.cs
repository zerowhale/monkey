using BoardgamePlatform.Game;
using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Farm;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Monkey.Games.Agricola
{
    public class AgricolaPlayer : GamePlayer
    {

        public AgricolaPlayer(AgricolaGame game, Player player)
            :base((IGame<GameHub>)game, player)
        {
            PersonalSupply = PersonalSupply.Empty;
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
            this.majorImprovements = majorImprovements.Add(id);
            ownedCards = ownedCards.Add(Curator.GetMajorImprovement(id));
        }

        public void RemoveMajorImprovement(int id)
        {
            this.majorImprovements = majorImprovements.Remove(id);
            ownedCards = ownedCards.Remove(Curator.GetMajorImprovement(id));
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

            ownedCards = ownedCards.Add(card);
        }

        public void ReturnCard(Card card)
        {
            if (this.OwnsCard(card.Id))
            {
                ownedCards = ownedCards.Remove(card);
            }
        }

        /// <summary>
        /// Sets a cards metadata information
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public AgricolaPlayer SetCardMetadata(Card card, Object data)
        {
            CardMetadata = CardMetadata.SetItem(card.Id, data);
            return this;
        }

        /// <summary>
        /// Sets a cards metadata information
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public AgricolaPlayer SetCardMetadata(int cardId, Object data)
        {
            CardMetadata = CardMetadata.SetItem(cardId, data);
            return this;
        }
        /// <summary>
        /// Retrieves a cards metadata information
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean TryGetCardMetadata(Card card, out Object data)
        {
            return TryGetCardMetadata(card.Id, out data);
        }

        /// <summary>
        /// Retrieves a cards metadata information by card id.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean TryGetCardMetadata(int card, out Object data)
        {
            return CardMetadata.TryGetValue(card, out data);
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
                this.PersonalSupply = this.PersonalSupply.AddResource(cache);
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
                this.PersonalSupply = this.PersonalSupply.AddResource(cache.Type, -cache.Count);
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
                        this.PersonalSupply = this.PersonalSupply.AddResource(conversionDefinition.InType, -conversion.Count);
                }
                else
                {
                    if (!Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                        this.PersonalSupply = this.PersonalSupply.AddResource(conversionDefinition.InType, -conversion.Count);

                    // This does not handle what happens if an item can be converted into animals. 
                    // Not sure this actually exists in the game.
                    this.PersonalSupply = this.PersonalSupply.AddResource(conversionDefinition.OutType, (conversion.Count / conversionDefinition.InAmount) * conversionDefinition.OutAmount);
                }
            }

            if (foodValue < foodNeeded)
            {
                begTotal = foodNeeded - foodValue;
                BeggingCards += begTotal;
            }

            var remainder = foodValue - foodNeeded;
            if (remainder > 0)
                this.PersonalSupply = this.PersonalSupply.AddResource(Resource.Food, remainder);

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



        /// <summary>
        ///  Adds the cache to the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public void AddResource(ResourceCache cache)
        {
            this.AddResource(cache.Type, cache.Count);
        }

        /// <summary>
        /// Subtracts the cache from the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public void RemoveResource(ResourceCache cache)
        {
            this.AddResource(cache.Type, -cache.Count);
        }

        /// <summary>
        /// Removes one of the resource from the personal supply
        /// </summary>
        /// <param name="resource"></param>
        public void RemoveResource(Resource resource)
        {
            this.PersonalSupply = this.PersonalSupply.AddResource(resource, -1);
        }

        /// <summary>
        /// Removes one of the resource from the personal supply
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public void RemoveResource(Resource resource, Int32 count)
        {
            this.PersonalSupply = this.PersonalSupply.AddResource(resource, -count);
        }

        /// <summary>
        /// Adds one of a resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        public void AddResource(Resource resource)
        {
            this.PersonalSupply = this.PersonalSupply.AddResource(resource, 1);
        }

        /// <summary>
        /// Adds a given quantity of a given resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public void AddResource(Resource resource, Int32 count)
        {
            this.PersonalSupply = this.PersonalSupply.AddResource(resource, count);
        }


        /// <summary>
        /// Gets the quantity of a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public int GetResource(Resource resource)
        {
            return this.PersonalSupply.GetResource(resource);
        }

        /// <summary>
        /// Gets the amount of food in the players personal supply
        /// </summary>
        public int Food
        {
            get { return this.PersonalSupply.Food; }
        }

        /// <summary>
        /// Gets the amount of grain in the players personal supply
        /// </summary>
        public int Grain
        {
            get { return this.PersonalSupply.Grain; }
        }

        /// <summary>
        /// Gets the amount of stone in the players personal supply
        /// </summary>
        public int Stone
        {
            get { return this.PersonalSupply.Stone; }
        }

        /// <summary>
        /// Gets the amount of wood in the players personal supply
        /// </summary>
        public int Wood
        {
            get { return this.PersonalSupply.Wood; }
        }
        
        /// <summary>
        /// Gets the amount of vegetables in the players personal supply
        /// </summary>
        public int Vegetables
        {
            get { return this.PersonalSupply.Vegetables; }
        }

        /// <summary>
        /// Gets the amount of clay in the players personal supply
        /// </summary>
        public int Clay
        {
            get { return this.PersonalSupply.Clay; }
        }

        /// <summary>
        /// Gets the amount of reed in the players personal supply
        /// </summary>
        public int Reed
        {
            get { return this.PersonalSupply.Reed; }
        }


        private PersonalSupply PersonalSupply;
        private ImmutableList<Card> ownedCards = ImmutableList<Card>.Empty;
        private ImmutableList<int> majorImprovements = ImmutableList<int>.Empty;
        private int familySize;

        /// <summary>
        /// Any cards requiring preservation of custom state data should store
        /// it in the metadata
        /// </summary>
        public ImmutableDictionary<int, Object> CardMetadata = ImmutableDictionary<int, Object>.Empty;

    }
}