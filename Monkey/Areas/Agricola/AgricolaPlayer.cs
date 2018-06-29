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
using System.Dynamic;
using System.Linq;

namespace Monkey.Games.Agricola
{
    public class AgricolaPlayer : GamePlayer
    {

        public AgricolaPlayer(AgricolaGame game, Player player)
            : base((IGame<GameHub>)game, player)
        {
            InitializeState();
        }

        [JsonIgnore]
        public ImmutableDictionary<string, Object> State
        {
            get;
            private set;
        }

        public ImmutableDictionary<string, Object> AddFamilyMember()
        {
            State = State.SetItem(StateKeyFamilySize, FamilySize + 1);
            State = State.SetItem(StateKeyNumBabies, NumBabies + 1);
            return State;
        }

        public ImmutableDictionary<string, Object> ReturnFamilyHome()
        {
            State = State.SetItem(StateKeyFamilyAtHome, FamilySize);
            State = State.SetItem(StateKeyNumBabies, 0);
            return State;
        }

        public ImmutableDictionary<string, Object> UseFamilyMember()
        {
            if (!HasFamilyMemberAvailable())
                throw new InvalidOperationException("No family members available");
            return State = State.SetItem(StateKeyFamilyAtHome, FamilyAtHome - 1);
        }


        /// <summary>
        ///  Adds the cache to the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public ImmutableDictionary<string, Object> AddResource(ResourceCache cache)
        {
            return State = this.AddResource(cache.Type, cache.Count);
        }

        /// <summary>
        /// Subtracts the cache from the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public ImmutableDictionary<string, Object> RemoveResource(ResourceCache cache)
        {
            return State = AddResource(cache.Type, -cache.Count);
        }

        /// <summary>
        /// Removes one of the resource from the personal supply
        /// </summary>
        /// <param name="resource"></param>
        public ImmutableDictionary<string, Object> RemoveResource(Resource resource)
        {

            PersonalSupply = PersonalSupply.AddResource(resource, -1);
            return State;
        }

        /// <summary>
        /// Removes one of the resource from the personal supply
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public ImmutableDictionary<string, Object> RemoveResource(Resource resource, Int32 count)
        {
            PersonalSupply = PersonalSupply.AddResource(resource, -count);
            return State;
        }

        /// <summary>
        /// Adds one of a resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        public ImmutableDictionary<string, Object> AddResource(Resource resource)
        {
            PersonalSupply = PersonalSupply.AddResource(resource, 1);
            return State;
        }

        /// <summary>
        /// Adds a given quantity of a given resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public ImmutableDictionary<string, Object> AddResource(Resource resource, Int32 count)
        {
            PersonalSupply = PersonalSupply.AddResource(resource, count);
            return State;
        }

        public ImmutableDictionary<string, Object> AddMajorImprovement(int id)
        {
            MajorImprovements = MajorImprovements.Add(id);
            OwnedCards = OwnedCards.Add(Curator.GetMajorImprovement(id));
            return State;
        }

        public ImmutableDictionary<string, Object> RemoveMajorImprovement(int id)
        {
            MajorImprovements = MajorImprovements.Remove(id);
            OwnedCards = OwnedCards.Remove(Curator.GetMajorImprovement(id));
            return State;
        }

        public ImmutableDictionary<string, Object> PlayCard(Card card)
        {
            if (HandOccupations.Contains(card))
                HandOccupations = HandOccupations.Remove(card);
            else if (HandMinors.Contains(card))
                HandMinors = HandMinors.Remove(card);
            else return State;

            OwnedCards = OwnedCards.Add(card);
            return State;
        }

        public ImmutableDictionary<string, Object> AddCardToHand(Card card)
        {
            if(card is Occupation)
            {
                HandOccupations = HandOccupations.Add(card);
            }
            else if(card is MinorImprovement)
            {
                HandMinors = HandMinors.Add(card);
            }
            else
            {
                throw new InvalidOperationException("Only minor improvements and occupations can be added to a players hand.");
            }
            return State;
        }

        public ImmutableDictionary<string, Object> RemoveCardFromHand(Card card)
        {
            if (card is Occupation)
            {
                HandOccupations = HandOccupations.Remove(card);
            }
            else if (card is MinorImprovement)
            {
                HandMinors = HandMinors.Remove(card);
            }
            else
            {
                throw new InvalidOperationException("Only minor improvements and occupations can be removed from a players hand.");
            }
            return State;
        }

        /// <summary>
        /// Recalculates the score card for the player
        /// </summary>
        /// <returns></returns>
        public ImmutableDictionary<string, Object> UpdateScoreCard()
        {
            this.ScoreCard = new ScoreCard(this);
            return State;
        }


        /// <summary>
        /// Returns true if there are any family members at home
        /// </summary>
        /// <returns></returns>
        public Boolean HasFamilyMemberAvailable()
        {
            return FamilyAtHome > 0;
        }

        public void PlayCard(int id)
        {
            var card = ((AgricolaGame)Game).GetCard(id);
            PlayCard(card);
        }



        public void ReturnCard(Card card)
        {
            if (this.OwnsCard(card.Id))
            {
                OwnedCards = OwnedCards.Remove(card);
            }
        }

        /// <summary>
        /// Sets a cards metadata information
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public AgricolaPlayer SetCardMetadata(Card card, ImmutableDictionary<string, Object> data)
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
        public AgricolaPlayer SetCardMetadata(int cardId, ImmutableDictionary<string, Object> data)
        {
            CardMetadata = CardMetadata.SetItem(cardId, data);
            return this;
        }

        public AgricolaPlayer SetCardMetadataField(Card card, string key, object value)
        {
            return SetCardMetadataField(card.Id, key, value);
        }
        
        public AgricolaPlayer SetCardMetadataField(int card, string key, object value)
        {
            ImmutableDictionary<string, Object> metadata;
            if(!TryGetCardMetadata(card, out metadata))
            {
                metadata = ImmutableDictionary<string, Object>.Empty;
            }            
            return this.SetCardMetadata(card, metadata.SetItem(key, value));
        }

        /// <summary>
        /// Retrieves a cards metadata information
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean TryGetCardMetadata(Card card, out ImmutableDictionary<string, Object> data)
        {
            return TryGetCardMetadata(card.Id, out data);
        }

        /// <summary>
        /// Retrieves a cards metadata information by card id.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean TryGetCardMetadata(int card, out ImmutableDictionary<string, Object> data)
        {
            return CardMetadata.TryGetValue(card, out data);
        }

        public Boolean TryGetCardMetadataField(Card card, string field, out ImmutableDictionary<string, Object> metadata, out Object fieldData)
        {
            fieldData = null;
            if (TryGetCardMetadata(card, out metadata))
            {
                return metadata.TryGetValue(field, out fieldData);
            }
            return false;
        }

        public Boolean TryGetCardMetadataField(int card, string field, out ImmutableDictionary<string, Object> metadata, out Object fieldData)
        {
            fieldData = null;
            if(TryGetCardMetadata(card, out metadata))
            {
                return metadata.TryGetValue(field, out fieldData);
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolvingPlayer"></param>
        /// <param name="triggeringPlayer"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public List<EventData> GetCardEventData(AgricolaPlayer triggeringPlayer, GameEventTrigger trigger)
        {
            var resolvingPlayer = (AgricolaPlayer)this;
            var events = new List<EventData>();
            foreach (var card in resolvingPlayer.OwnedCards)
            {
                if (card.Events == null)
                    continue;

                var cardEvents = card.Events.Where(x => x.Triggers.Any(s => s.Triggered(resolvingPlayer, triggeringPlayer, trigger)));
                foreach (var cardEvent in cardEvents)
                {
                    EventData data = new EventData(trigger, cardEvent, card);
                    events.Add(data);
                }
            }

            return events;
        }

        public void HarvestFields(List<GameActionNotice> notices)
        {
            ResourceCache[] resources;
            Farmyard = Farmyard.HarvestFields(out resources);
            foreach (var cache in resources)
            {
                this.PersonalSupply = this.PersonalSupply.AddResource(cache);
            }
            notices.Add(new GameActionNotice(this.Name, NoticeVerb.Harvested.ToString(), resources.ToList<INoticePredicate>()));
            Harvesting = true;
        }

        public void AddRoom(int index)
        {
            Farmyard = Farmyard.AddRoom(index);
        }

        public void AddRoom(int x, int y)
        {
            Farmyard = Farmyard.AddRoom(x, y);
        }

        public void AddStable(int index)
        {
            Farmyard = Farmyard.AddStable(index);
        }

        public void PlowField(int index)
        {
            Farmyard = Farmyard.PlowField(index);
        }

        public void SowField(int index, Resource resource)
        {
            Farmyard = Farmyard.SowField(index, resource);
        }


        public void Renovate()
        {
            Farmyard = Farmyard.Renovate();
        }

        public void AddFences(int[] indices)
        {
            Farmyard = Farmyard.AddFences(indices);
        }

        public void SetPastures(ImmutableArray<int[]> pastures)
        {
            Farmyard = Farmyard.SetPastures(pastures);
        }

        public Farmyard UpdateAnimalManager()
        {
            return Farmyard = Farmyard.UpdateAnimalManager();
        }

        public Farmyard AssignAnimals(AnimalHousingData[] assignments)
        {
            return Farmyard = Farmyard.AssignAnimals(assignments);
        }

        public Farmyard RemoveAnimals(AnimalResource type, int count)
        {
            return Farmyard = Farmyard.RemoveAnimals(type, count);
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

                if (conversionDefinition == null)
                    return false;

                // Invalid input amount
                if (conversion.Count % conversionDefinition.InAmount != 0)
                    return false;

                if (Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                {
                    var owned = this.Farmyard.AnimalCount((AnimalResource)Enum.Parse(typeof(AnimalResource), conversionDefinition.InType.ToString()));
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

        /// <summary>
        /// Returns true if the player owns the card with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool OwnsCard(int id)
        {
            return this.OwnedCardIds.Contains(id);
        }

        /// <summary>
        /// Number of family members still available to take actions
        /// </summary>
        public int FamilyAtHome
        {
            get
            {
                return (int)State[StateKeyFamilyAtHome];
            }
        }

        public int FamilySize
        {
            get
            {
                return (int)State[StateKeyFamilySize];
            }
        }

        public int NumBabies
        {
            get
            {
                return (int)State[StateKeyNumBabies];
            }
        }













        /// <summary>
        /// Used by partial updates to retrieve the players whole hand
        /// </summary>
        [JsonIgnore]
        public Object Hand
        {
            get
            {
                return new
                {
                    Minors = HandMinors.Select(x => x.Id).ToImmutableArray(),
                    Occupations = HandOccupations.Select(x => x.Id).ToImmutableArray()
                };
            }
        }

        /// <summary>
        /// Gets the quantity of a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public int GetResource(Resource resource)
        {
            return PersonalSupply.GetResource(resource);
        }

        /// <summary>
        /// Gets the amount of food in the players personal supply
        /// </summary>
        public int Food
        {
            get { return PersonalSupply.Food; }
        }

        /// <summary>
        /// Gets the amount of grain in the players personal supply
        /// </summary>
        public int Grain
        {
            get { return PersonalSupply.Grain; }
        }

        /// <summary>
        /// Gets the amount of stone in the players personal supply
        /// </summary>
        public int Stone
        {
            get { return PersonalSupply.Stone; }
        }

        /// <summary>
        /// Gets the amount of wood in the players personal supply
        /// </summary>
        public int Wood
        {
            get { return PersonalSupply.Wood; }
        }

        /// <summary>
        /// Gets the amount of vegetables in the players personal supply
        /// </summary>
        public int Vegetables
        {
            get { return PersonalSupply.Vegetables; }
        }

        /// <summary>
        /// Gets the amount of clay in the players personal supply
        /// </summary>
        public int Clay
        {
            get { return PersonalSupply.Clay; }
        }

        /// <summary>
        /// Gets the amount of reed in the players personal supply
        /// </summary>
        public int Reed
        {
            get { return PersonalSupply.Reed; }
        }

        /// <summary>
        /// Number of Begging cards the player has.
        /// </summary>
        public int BeggingCards
        {
            get
            {
                return ((int)State[StateKeyBeggingCards]);
            }
            private set
            {
                State = State.SetItem(StateKeyBeggingCards, value);
            }
        }

        public ImmutableList<int> MajorImprovements
        {
            get
            {
                return ((ImmutableList<int>)State[StateKeyMajorImprovements]);
            }
            private set
            {
                State = State.SetItem(StateKeyMajorImprovements, value);
            }
        }

        [JsonIgnore]
        public ImmutableList<Card> OwnedCards
        {
            get
            {
                return ((ImmutableList<Card>)State[StateKeyOwnedCards]);
            }
            private set
            {
                State = State.SetItem(StateKeyOwnedCards, value);
            }
        }

        [JsonIgnore]
        public ImmutableList<Card> HandMinors
        {
            get
            {
                return ((ImmutableList<Card>)State[StateKeyHandMinors]);
            }
            private set
            {
                State = State.SetItem(StateKeyHandMinors, value);
            }
        }

        [JsonIgnore]
        public ImmutableList<Card> HandOccupations
        {
            get
            {
                return ((ImmutableList<Card>)State[StateKeyHandOccupations]);
            }
            private set
            {
                State = State.SetItem(StateKeyHandOccupations, value);
            }
        }

        public ScoreCard ScoreCard
        {
            get
            {
                return ((ScoreCard)State[StateKeyScorecard]);
            }
            private set
            {
                State = State.SetItem(StateKeyScorecard, value);
            }
        }

        public bool Harvesting
        {
            get
            {
                return ((bool)State[StateKeyIsHarvesting]);
            }
            private set
            {
                State = State.SetItem(StateKeyIsHarvesting, value);
            }
        }

        /// <summary>
        /// Any cards requiring preservation of custom state data should store
        /// it in the metadata
        /// </summary>
        public ImmutableDictionary<int, ImmutableDictionary<string, Object>> CardMetadata
        {
            get
            {
                return ((ImmutableDictionary<int, ImmutableDictionary<string, Object>>)State[StateKeyCardMetadata]);
            }
            private set
            {
                State = State.SetItem(StateKeyCardMetadata, value);
            }
        }

        public Farmyard Farmyard
        {
            get
            {
                return (Farmyard)State[StateKeyFarmyard];
            }
            private set
            {
                State = State.SetItem(StateKeyFarmyard, value);
            }
        }

        public int[] OwnedCardIds
        {
            get { return OwnedCards.Select(x => x.Id).ToArray(); }
        }

        private void InitializeState()
        {
            State = ImmutableDictionary<string, Object>.Empty;
            State = State.SetItem(StateKeyFamilySize, 0);
            State = State.SetItem(StateKeyNumBabies, 0);
            State = State.SetItem(StateKeyFamilyAtHome, 0);
            State = State.SetItem(StateKeyBeggingCards, 0);
            State = State.SetItem(StateKeyPersonalSupply, PersonalSupply.Empty);
            State = State.SetItem(StateKeyMajorImprovements, ImmutableList<int>.Empty);
            State = State.SetItem(StateKeyOwnedCards, ImmutableList<Card>.Empty);
            State = State.SetItem(StateKeyHandMinors, ImmutableList<Card>.Empty);
            State = State.SetItem(StateKeyHandOccupations, ImmutableList<Card>.Empty);
            State = State.SetItem(StateKeyIsHarvesting, false);
            State = State.SetItem(StateKeyCardMetadata, ImmutableDictionary<int, ImmutableDictionary<string, Object>>.Empty);
            State = State.SetItem(StateKeyFarmyard, new Farmyard());
        }

        private PersonalSupply PersonalSupply
        {
            get
            {
                return ((PersonalSupply)State[StateKeyPersonalSupply]);
            }
            set
            {
                State = State.SetItem(StateKeyPersonalSupply, value);
            }
        }

        private const string StateKeyFamilySize = "FamilySize";
        private const string StateKeyNumBabies = "NumBabies";
        private const string StateKeyFamilyAtHome = "FamilyAtHome";
        private const string StateKeyPersonalSupply = "PersonalSupply";
        private const string StateKeyMajorImprovements = "MajorImprovements";
        private const string StateKeyOwnedCards = "OwnedCards";
        private const string StateKeyBeggingCards = "BeggingCards";
        private const string StateKeyHandMinors = "HandMinors";
        private const string StateKeyHandOccupations = "HandOccupations";
        private const string StateKeyScorecard = "Scorecard";
        private const string StateKeyIsHarvesting = "IsHarvesting";
        private const string StateKeyCardMetadata = "CardMetada";
        private const string StateKeyFarmyard = "Farmyard";

    }
}