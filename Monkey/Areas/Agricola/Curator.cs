using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.AnytimeActions;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Cards.Costs;
using Monkey.Games.Agricola.Cards.GameEndPoints;
using Monkey.Games.Agricola.Cards.Prerequisites;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Farm;
using Monkey.Games.Agricola.Utils;
using BoardgamePlatform.Game.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Collections.Immutable;

namespace Monkey.Games.Agricola
{
    /// <summary>
    /// The curator answers all questions about the rules.  
    /// Anything that can be modified by another source, such 
    /// as improvements, occupations, or the game state
    /// should be requested through the Curator.
    /// </summary>
    public static class Curator
    {

        /// <summary>
        /// Loads the major improvement objects from xml and returns a json
        /// version of the data for front end use.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string LoadMajorImprovements(String filePath)
        {
            var xml = XDocument.Load(filePath);
            majorImprovements = xml.Descendants("MajorImprovement").Select(Card.Create).ToDictionary(key => key.Id, value => (MajorImprovement)value);
            majorImprovementDeck = majorImprovementDeck.AddRange(majorImprovements.Values.ToArray<MajorImprovement>());
            return JsonConvert.SerializeObject(majorImprovements);
        }

        public static string LoadOccupations(String filePath)
        {

            var xml = XDocument.Load(filePath);
            occupations = xml.Descendants("Occupation").Select(Card.Create).ToDictionary(key => key.Id, value => (Occupation)value);
            occupationDeck = occupationDeck.AddRange(occupations.Values.ToArray<Occupation>());
            return JsonConvert.SerializeObject(occupations);
        }


        public static string LoadMinorImprovements(String filePath)
        {
            var xml = XDocument.Load(filePath);
            minorImprovements = xml.Descendants("MinorImprovement").Select(Card.Create).ToDictionary(key => key.Id, value => (MinorImprovement)value);
            minorImprovementDeck = minorImprovementDeck.AddRange(minorImprovements.Values.ToArray<MinorImprovement>());
            return JsonConvert.SerializeObject(minorImprovements);
        }

        public static bool CanAfford(AgricolaPlayer player, ResourceCache[] costs){
            foreach(var c in costs){
                if (player.GetResource(c.Type) < c.Count)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Retrieves the card data for the given major improvement
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MajorImprovement GetMajorImprovement(int id)
        {
            return majorImprovements[id];
        }

        /// <summary>
        /// The full deck of major improvements
        /// </summary>
        public static ImmutableArray<MajorImprovement> MajorImprovementsDeck
        {
            get { return majorImprovementDeck; }
        }

        /// <summary>
        /// The full deck of minor improvements
        /// </summary>
        public static ImmutableArray<MinorImprovement> MinorImprovementsDeck
        {
            get { return minorImprovementDeck; }
        }

        /// <summary>
        /// The full deck of occupations
        /// </summary>
        public static ImmutableArray<Occupation> OccupationsDeck
        {
            get { return occupationDeck; }
        }

        /// <summary>
        /// Checks if a player has room to grow their family.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanGrowFamily(AgricolaPlayer player, bool withoutSpace = false)
        {
            if(player.FamilySize >= AgricolaGame.MaxFamilySize)
                return false;
                
            return player.FamilySize < player.Farmyard.RoomCount || withoutSpace;
        }



        public static List<ResourceConversion> GetAnytimeResourceConversions(AgricolaPlayer player)
        {
            resourceConversions.Clear();

            resourceConversions.Add(new ResourceConversion(-1, Resource.Food, 1, null, Resource.Food, 1));
            resourceConversions.Add(new ResourceConversion(-2, Resource.Grain, 1, null, Resource.Food, 1));
            resourceConversions.Add(new ResourceConversion(-3, Resource.Vegetables, 1, null, Resource.Food, 1));

            var cards = player.OwnedCards;

            foreach (var card in cards)
            {
                if (card.ResourceConversions != null)
                {
                    foreach (var rc in card.ResourceConversions)
                    {
                        //if (!rc.InLimit.HasValue)
                        //updateResourceConversions(card.Id, rc.InType, rc.InAmount, rc.InLimit, rc.OutType, rc.OutAmount);
                        resourceConversions.Add(rc);
                    }
                }
            }
            return resourceConversions;
        }


        /// <summary>
        /// Returns the values of all resources at harvest time
        /// </summary>
        /// <param name="player"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static List<ResourceConversion> GetHarvestFoodValues(AgricolaPlayer player)
        {
            resourceConversions.Clear();
            resourceConversions.Add(new ResourceConversion(-1, Resource.Food, 1, null, Resource.Food, 1));
            resourceConversions.Add(new ResourceConversion(-2, Resource.Grain, 1, null, Resource.Food, 1));
            resourceConversions.Add(new ResourceConversion(-3, Resource.Vegetables, 1, null, Resource.Food, 1));

            var cards = player.OwnedCards;
            foreach(var card in cards){
                if (card.ResourceConversions != null)
                {
                    foreach (var rc in card.ResourceConversions)
                    {
                        //updateResourceConversions(card.Id, rc.InType, rc.InAmount, rc.InLimit, rc.OutType, rc.OutAmount);
                        resourceConversions.Add(rc);
                    }
                
                }
            }

            return resourceConversions;
        }


        /// <summary>
        /// Checks if an improvement is available.  If the requested improvement is
        /// major it checks if it's unowned.  if it's minor it checks if it's in the 
        /// requesting players hand.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsImprovementAvailable(AgricolaPlayer player, int id)
        {
            var game = player.Game as AgricolaGame;
            if (majorImprovements.Keys.Contains(id))
            {
                return !game.OwnedMajorImprovements.Contains(id);
            }
            else
            {
                return player.HandMinors.Contains(((AgricolaGame)player.Game).GetCard(id));
                    
            }
            throw new ArgumentOutOfRangeException("id");
        }

        public static int GetOwnedOccupationCount(AgricolaPlayer player)
        {
            var count = 0;
            foreach (var card in player.OwnedCards)
            {
                if (card is Occupation)
                {
                    count++;
                    if (card.Id == 148) // Academic
                        count++;
                }
            }
            return count;
        }

        public static ResourceCache[] GetOccupationCost(AgricolaPlayer player, int actionId, int occupationId)
        {
            var foodCost = 0;
            var numOccupationsPlayed = GetOwnedOccupationCount(player);

            switch (actionId)
            {
                case 4: 
                    foodCost = numOccupationsPlayed == 0 ? 0 : 1;
                    break;

                case 14:
                case 20:
                    foodCost = numOccupationsPlayed < 2 ? 1 : 2;
                    break;

                default:
                    foodCost = 2;
                    break;
            }

            var card = ((AgricolaGame)player.Game).GetCard(occupationId);
            List<ResourceCache> costs;

            if (card.Costs.Length > 0)
            {
                var costOption = card.Costs[0];
                if (costOption is ResourceCardCost)
                    costs = ((ResourceCardCost)costOption).Resources.ToList();
                else
                    costs = new List<ResourceCache>();
            }
            else
            {
                costs = new List<ResourceCache>();
            }

            if(foodCost > 0){

                for (var i = 0; i < costs.Count; i++)
                {
                    var resource = costs[i];
                    if (resource.Type == Resource.Food)
                    {
                        var newResource = resource.updateCount(foodCost);
                        costs[i] = newResource;
                        return costs.ToArray();
                    }
                }
                costs.Add(new ResourceCache(Resource.Food, foodCost));

            }
            
            return costs.ToArray();
        }

        public static int? GetMaxBakeInput(AgricolaPlayer player, int cardId)
        {
            var card = ((AgricolaGame)player.Game).GetCard(cardId);
            var limit = card.BakeProperties.InLimit;

            return limit.Value;
        }

        public static int GetMaxPlowable(AgricolaPlayer player, int actionId, out Plow[] plows)
        {
            var maxPlowable = 1;

            var availablePlows = new List<Plow>();
            var ownedCards = player.OwnedCards;
            foreach (var card in ownedCards)
            {
                if(card is MinorImprovement){
                    var minor = (MinorImprovement)card;
                    if (minor.Plow != null 
                        && minor.Plow.Used < minor.Plow.MaxUses
                        && minor.Plow.OnActions.Contains(actionId))
                    {
                        availablePlows.Add(minor.Plow);
                        if(maxPlowable < minor.Plow.Fields)
                            maxPlowable = minor.Plow.Fields;
                    }
                }
            }

            plows = availablePlows.Count > 0 ? availablePlows.ToArray() : null;
            return maxPlowable;
        }


        public static ResourceCache GetBakeOutput(AgricolaPlayer player, int cardId, int inputAmount)
        {
            var card = ((AgricolaGame)player.Game).GetCard(cardId);
            if (card.BakeProperties != null)
                return new ResourceCache(card.BakeProperties.OutType, card.BakeProperties.OutAmount * (inputAmount / card.BakeProperties.InAmount));

            return null;
        }

        /// <summary>
        /// Returns true if the player has an Oven on their farm
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasOven(AgricolaPlayer player)
        {
            return player.OwnedCards.Where(x => x is Improvement).Any(x => (x as Improvement).Oven);
        }

        /// <summary>
        /// Returns true if the player has any cards that allow them to convert resources into food.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanCook(AgricolaPlayer player)
        {
            var ids = player.OwnedCardIds;
            foreach(var id in ids){
                var card = ((AgricolaGame)player.Game).GetCard(id);
                if(card.ResourceConversions != null)
                {
                    foreach(var rc in card.ResourceConversions){
                        if (rc.OutType == Resource.Food)
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool CanBake(AgricolaPlayer player)
        {
            var ids = player.OwnedCardIds;
            foreach (var id in ids)
            {
                if (((AgricolaGame)player.Game).GetCard(id).BakeProperties != null)
                    return true;
            }
            return false;
        }

        public static bool CanAffordToBake(AgricolaPlayer player)
        {
            var minBakeCost = Int32.MaxValue;
            var ids = player.OwnedCardIds;
            foreach (var id in ids)
            {
                var card = ((AgricolaGame)player.Game).GetCard(id);
                if (card.BakeProperties != null && minBakeCost > card.BakeProperties.InAmount)
                    minBakeCost = card.BakeProperties.InAmount;
            }

            return player.Grain >= minBakeCost; 
        }

        /// <summary>
        /// Checks if a player can afford to renovate their house
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanAffordRenovation(AgricolaPlayer player, out ResourceCache[] costs)
        {
            costs = GetRenovationCost(player);
            foreach(var cost in costs){
                if (player.GetResource(cost.Type) < cost.Count)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static ResourceCache[] GetRenovationCost(AgricolaPlayer player)
        {
            var costs = new List<ResourceCache>();
            var numRooms = player.Farmyard.RoomLocations.Count;

            switch (player.Farmyard.HouseType)
            {
                case HouseType.Wood:
                    costs.Add(new ResourceCache(Resource.Clay, numRooms));
                    costs.Add(new ResourceCache(Resource.Reed, 1));
                    break;

                case HouseType.Clay:
                    costs.Add(new ResourceCache(Resource.Stone, numRooms));
                    costs.Add(new ResourceCache(Resource.Reed, 1));
                    break;
            }
            return costs.ToArray();
        }

        /// <summary>
        /// Returns true if the user has the ability to fence in any pastures given
        /// the number of fences remaining.
        /// 
        /// This does not factor affording costs in.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanPlaceFences(AgricolaPlayer player)
        {

            return player.Farmyard.EmptyLocations.Count > 0 && Curator.HasFencesLeft(player) && player.Farmyard.CanFencePasture();
        }

        public static bool HasFencesLeft(AgricolaPlayer player)
        {
            return Farmyard.MAX_FENCES - player.Farmyard.Fences.Count > 0;
        }


        /// <summary>
        /// Checks if a player can afford a fence
        /// </summary>
        /// <param name="player"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool CanAffordFences(AgricolaPlayer player, int actionId, int count, out ResourceCache[] costs)
        {
            switch (actionId)
            {
                case (int)InterruptActionId.FencePasture:
                    costs = new ResourceCache[]{};
                    return true;
                default:
                    costs = new ResourceCache[]{new ResourceCache(Resource.Wood, count)};
                    return player.Wood >= count;
            }
        }

        /// <summary>
        /// Gets the cost of the fence for the player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetFenceCost(AgricolaPlayer player, int count)
        {
            return count;
        }

        /// <summary>
        /// Gets the cost of the number of stables requested
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static ResourceCache[] GetStablesCosts(AgricolaPlayer player, int actionId, int numStables)
        {
            var costs = new List<ResourceCache>();
            var woodCost = numStables;

            if (actionId == (int)InterruptActionId.BuildStable)
                woodCost = 0;
            else if (actionId != 54)  // Build 1 Stable for 1 Wood action
                woodCost *= 2;
            
            costs.Add( new ResourceCache(Resource.Wood, woodCost));
            return costs.ToArray();
        }



        public static ResourceCache[] GetRoomsCosts(AgricolaPlayer player, int actionId, int numRooms)
        {
            var costs = new List<ResourceCache>();

            if (actionId == (int)AnytimeActionId.BuildRoom 
                || actionId == (int)InterruptActionId.BuildRoom)
                return costs.ToArray();

            switch (player.Farmyard.HouseType)
            {
                case HouseType.Wood:
                    costs.Add(new ResourceCache(Resource.Wood, 5 * numRooms));
                    break;
                case HouseType.Clay:
                    costs.Add(new ResourceCache(Resource.Clay, 5 * numRooms));
                    break;
                case HouseType.Stone:
                    costs.Add(new ResourceCache(Resource.Stone, 5 * numRooms));
                    break;
            }
            costs.Add(new ResourceCache(Resource.Reed, 2 * numRooms));
            
            return costs.ToArray();
        }



        /// <summary>
        /// Checks if a player can afford a given improvement
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cardId"></param>
        /// <param name="paymentIndex"></param>
        /// <returns></returns>
        public static bool CanAffordCard(AgricolaPlayer player, int cardId, int paymentIndex, out ResourceCache[] costs)
        {
            costs = null;
            var card = ((AgricolaGame)player.Game).GetCard(cardId);

            if (paymentIndex < 0 || paymentIndex >= card.Costs.Length)
                return false;

            var cost = card.Costs[paymentIndex];
            if (cost is ResourceCardCost)
            {
                var rcCost = (ResourceCardCost)cost;
                costs = rcCost.Resources.ToArray();

                foreach (var resource in rcCost.Resources)
                {
                    if(resource.Type.IsAnimal()){
                        if (resource.Count > player.Farmyard.AnimalManager.GetAnimalCount((AnimalResource)resource.Type))
                            return false;
                    }
                    else{
                        if (resource.Count > player.GetResource(resource.Type))
                            return false;
                    }
                }
            }
            else if (cost is ReturnCardCardCost)
            {
                var miCost = (ReturnCardCardCost)cost;
                var ownedCards = player.OwnedCardIds;

                return miCost.Ids.Intersect(ownedCards).Count() > 0;
            }
            return true;
        }


        public static CardCost GetCardCost(AgricolaPlayer player, int cardId, int paymentIndex = 0)
        {
            var card = ((AgricolaGame)player.Game).GetCard(cardId);

            if (paymentIndex < 0 || card.Costs.Length <= paymentIndex)
                throw new ArgumentOutOfRangeException("paymentIndex");

            return card.Costs[paymentIndex];
        }


        /// <summary>
        /// Calculates a players score for fields
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int CalculateFieldsScore(AgricolaPlayer player)
        {
            var fields = player.Farmyard.FieldLocations.Count();
            // 165 - yoeman farmer
            if (fields < 2) return player.OwnsCard(165) ? 0 : -1;
            else if (fields < 5) return fields - 1;
            else return 4;
        }

        public static int CalculatePasturesScore(AgricolaPlayer player)
        {
            var pastures = player.Farmyard.Pastures.Count();
            // 165 - yoeman farmer
            if (pastures == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (pastures > 4) return 4;
            else return pastures;
        }

        public static int CalculateGrainScore(AgricolaPlayer player)
        {
            var grain = player.Grain + player.Farmyard.PlantedResourceCount(Resource.Grain);
            // 165 - yoeman farmer
            if (grain == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (grain < 4) return 1;
            else if (grain < 6) return 2;
            else if (grain < 8) return 3;
            else return 4;
        }

        public static int CalculateVegetablesScore(AgricolaPlayer player)
        {
            var vegetables = player.Vegetables + player.Farmyard.PlantedResourceCount(Resource.Vegetables);
            // 165 - yoeman farmer
            if (vegetables == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (vegetables > 4) return 4;
            else return vegetables;
        }

        public static int CalculateSheepScore(AgricolaPlayer player)
        {
            var sheep = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Sheep);
            // 165 - yoeman farmer
            if (sheep == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (sheep < 4) return 1;
            else if (sheep < 6) return 2;
            else if (sheep < 8) return 3;
            return 4;
        }

        public static int CalculateBoarScore(AgricolaPlayer player)
        {
            var boar = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Boar);
            // 165 - yoeman farmer
            if (boar == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (boar < 3) return 1;
            else if (boar < 5) return 2;
            else if (boar < 7) return 3;
            else return 4;
        }

        public static int CalculateCattleScore(AgricolaPlayer player)
        {
            var cattle = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Cattle);
            // 165 - yoeman farmer
            if (cattle == 0) return player.OwnsCard(165) ? 0 : -1;
            else if (cattle < 2) return 1;
            else if (cattle < 4) return 2;
            else if (cattle < 6) return 3;
            else return 4;
        }

        public static int CalculateUnusedSpaceScore(AgricolaPlayer player)
        {
            return -player.Farmyard.EmptyLocations.Count();
        }

        public static int CalculateFencedStablesScore(AgricolaPlayer player)
        {
            var pastures = player.Farmyard.PastureLocations;
            var stables = player.Farmyard.StableLocations;

            return pastures.Intersect(stables).Count();
        }

        /// <summary>
        /// Calculates the points gained for each room a player has
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int CalculateRoomsScore(AgricolaPlayer player)
        {
            var roomValue = 0;
            if (player.Farmyard.HouseType == HouseType.Clay)
                roomValue = 1;
            else if (player.Farmyard.HouseType == HouseType.Stone)
                roomValue = 2;

            return player.Farmyard.RoomCount * roomValue;
        }

        /// <summary>
        /// Calculates the points gained for each family member a player has
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int CalculateFamilyMemberScore(AgricolaPlayer player)
        {
            return player.FamilySize * 3;
        }

        /// <summary>
        /// Returns the amount of points lost for begging cards for a player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int CalculateBeggingScore(AgricolaPlayer player)
        {
            return -player.BeggingCards * 3;
        }

        private static Dictionary<int, MajorImprovement> majorImprovements;
        private static Dictionary<int, MinorImprovement> minorImprovements;
        private static Dictionary<int, Occupation> occupations;

        private static ImmutableArray<MajorImprovement> majorImprovementDeck = ImmutableArray<MajorImprovement>.Empty;
        private static ImmutableArray<MinorImprovement> minorImprovementDeck = ImmutableArray<MinorImprovement>.Empty;
        private static ImmutableArray<Occupation> occupationDeck = ImmutableArray<Occupation>.Empty;

        private static List<ResourceConversion> resourceConversions = new List<ResourceConversion>();

    }
}