using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Cards.Costs;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Farm;
using Monkey.Games.Agricola.Notification;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Monkey.Games.Agricola.Actions.Services
{
    public static class ActionService
    {


        /// <summary>
        /// Helper method for processing a list of triggers
        /// </summary>
        /// <param name="player"></param>
        /// <param name="eventTriggers"></param>
        /// <param name="resultingNotices"></param>
        public static void CheckTriggers(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices)
        {
            if (eventTriggers != null)
            {
                foreach (var trigger in eventTriggers)
                {
                    ProcessEventTrigger(player, trigger, resultingNotices);
                }
            }
        }

        /// <summary>
        /// Compiles a list of events fired by a trigger
        /// </summary>
        /// <param name="player"></param>
        /// <param name="trigger"></param>
        /// <param name="resultingNotices"></param>
        public static void ProcessEventTrigger(AgricolaPlayer player, GameEventTrigger trigger, List<GameActionNotice> resultingNotices)
        {
            foreach (var p in ((AgricolaGame)player.Game).AgricolaPlayers)
            {

                var events = p.GetCardEventData(player, trigger);
                if(events.Count > 0)
                    ActionService.ExecuteEvents(p, events, resultingNotices);
            }
        }

        /// <summary>
        /// Executes a list of events
        /// </summary>
        /// <param name="player"></param>
        /// <param name="events"></param>
        /// <param name="resultingNotices"></param>
        public static void ExecuteEvents(AgricolaPlayer player, List<EventData> events, List<GameActionNotice> resultingNotices)
        {
            foreach (var eventData in events)
            {
                eventData.TriggeredEvent.Execute(player, eventData.Trigger, eventData.Card, resultingNotices);
            }
        }






        /// <summary>
        /// Assigns multiple resources to a player. 
        /// This method triggers no events.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="resources"></param>
        /// <param name="resultingNotices"></param>
        public static void AssignResources(AgricolaPlayer player, ResourceCache[] resources, List<GameActionNotice> resultingNotices){
            foreach(var resource in resources)
                ActionService.AssignResource(player, resource, resultingNotices);
        }


        /// <summary>
        /// Assigns a single resource to a player.
        /// This method triggers no events.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="resource"></param>
        /// <param name="resultingNotices"></param>
        public static void AssignResource(AgricolaPlayer player, ResourceCache resource, List<GameActionNotice> resultingNotices)
        {

            player.AddResource(resource);
            var resourcePredicateFound = false;
            foreach (var predicates in resultingNotices.Where(x => ((string)x.Subject) == player.Name).Select(x => x.Predicates))
            {
                if (predicates != null)
                {
                    for(var i = 0; i< predicates.Count; i++)
                    {
                        var predicate = predicates[i];
                        if (predicate is ResourceCache)
                        {
                            resourcePredicateFound = true;
                            var cache = (ResourceCache)predicate;
                            if (cache.Type == resource.Type)
                            {
                                predicates[i] = cache.updateCount(resource.Count);
                                return;
                            }
                        }
                    }

                    if (resourcePredicateFound)
                    {
                        predicates.Add(new ResourceCache(resource.Type, resource.Count));
                        return;
                    }
                }
            }
            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Take.ToString(), new ResourceCache(resource.Type, resource.Count)));

        }

        /// <summary>
        /// Assigns multiple resources to a player and triggers
        /// events.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="caches"></param>
        /// <param name="eventTriggers">Triggers attached to the calling actions.</param>
        /// <param name="resultingNotices"></param>
        public static void AssignCacheResources(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, ResourceCache[] caches)
        {
            var animals = new List<ResourceCache>();
            foreach (var cache in caches)
            {
                if (cache.Type.IsAnimal())
                {
                    animals.Add(cache);
                }
                else
                {
                    AssignCacheResource(player, null, resultingNotices, cache, true);
                }
            }

            if (animals.Count > 0)
            {
                ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animals.ToArray(), resultingNotices));
            }

            CheckTriggers(player, eventTriggers, resultingNotices);

        }

        /// <summary>
        /// Assigns a single resource to a player and triggers
        /// events.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cache"></param>
        /// <param name="eventTriggers">Triggers attached to the calling actions.</param>
        /// <param name="resultingNotices"></param>
        /// <param name="partOfMultiCache"></param>
        public static void AssignCacheResource(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, ResourceCache cache, bool partOfMultiCache = false)
        {

            if (cache.Type.IsAnimal())
            {
                //player.Game.AddInterrupt(new AssignAnimalsAction(player, cache, resultingNotices));
            }
            else
            {
                player.AddResource(cache.Type, cache.Count);
                resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Take.ToString(), new ResourceCache(cache.Type, cache.Count)));

                var resourceTrigger = new TakeCachedResourceTrigger(cache.Type);
                ProcessEventTrigger(player, resourceTrigger, resultingNotices);
            
                if (!partOfMultiCache)
                {
                    var onlyResourceTrigger = new TakeSoleCachedResourceTrigger(cache.Type);
                    ProcessEventTrigger(player, onlyResourceTrigger, resultingNotices);
                }

                CheckTriggers(player, eventTriggers, resultingNotices);
            }

        }


        public static void AssignTakeResource(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, ResourceCache cache)
        {
            ActionService.AssignResource(player, cache, resultingNotices);
            CheckTriggers(player, eventTriggers, resultingNotices);
        }

        public static void AssignTakeResources(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, ResourceCache[] caches)
        {
            ActionService.AssignResources(player, caches, resultingNotices);
            CheckTriggers(player, eventTriggers, resultingNotices);
        }

        /// <summary>
        /// Checks that the requested build stable data is valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stables"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static bool CanBuildStables(AgricolaPlayer player, ImmutableArray<int> stables, int actionId)
        {
            var costs = Curator.GetStablesCosts(player, actionId, stables.Length);
            return player.CanAfford(costs) && player.Farmyard.StablesLocationsValid(stables);
        }

        /// <summary>
        /// Pays for and builds the requested stables
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stables"></param>
        /// <param name="actionId"></param>
        /// <param name="resultingNotices"></param>
        public static void BuildStables(AgricolaPlayer player, ImmutableArray<int> stables, int actionId, List<GameActionNotice> resultingNotices)
        {
            var costs = Curator.GetStablesCosts(player, actionId, stables.Length);
            player.PayCosts(costs);

            foreach (var stable in stables)
            {
                player.Farmyard.AddStable(stable);
            }

            player.Farmyard.UpdateAnimalManager();
            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Build.ToString(), new BuildPredicate(stables.Length, Buildable.Stable)));
        }

        /// <summary>
        /// Checks that the requested rooms are valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rooms"></param>
        /// <returns></returns>
        public static bool CanBuildRooms(AgricolaPlayer player, int actionId, ImmutableArray<int> rooms)
        {
            var costs = Curator.GetRoomsCosts(player, actionId, rooms.Length);
            return player.CanAfford(costs) && player.Farmyard.RoomLocationsValid(rooms);
        }

        /// <summary>
        /// Pays for and builds the requested rooms.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rooms"></param>
        /// <param name="resultingNotices"></param>
        public static void BuildRooms(AgricolaPlayer player, int actionId, ImmutableArray<int> rooms, List<GameActionNotice> resultingNotices)
        {
            var costs = Curator.GetRoomsCosts(player, actionId, rooms.Length);
            player.PayCosts(costs);

            foreach (var room in rooms)
            {
                player.Farmyard.AddRoom(room);
            }

            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Build.ToString(), new BuildPredicate(rooms.Length, Buildable.Room)));
        }

        /// <summary>
        /// Ensures that the requested animal assignment data is valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="newAnimals"></param>
        /// <returns></returns>
        public static bool CanAssignAnimals(AgricolaPlayer player, AnimalCacheActionData data, Dictionary<AnimalResource, int> newAnimals)
        {
            return ActionService.CanAssignAnimals(player, data, player.Farmyard.AnimalManager, newAnimals);
        }

        /// <summary>
        /// Ensures that the requested animal assignment data is valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="newAnimals"></param>
        /// <returns></returns>
        public static bool CanAssignAnimals(AgricolaPlayer player, AnimalCacheActionData data, AnimalManager manager, Dictionary<AnimalResource, int> newAnimals)
        {
            var requestedTotals = AnimalHousingData.GetTotals(data.Assignments);
            requestedTotals[AnimalResource.Sheep] += data.Free[AnimalResource.Sheep] + data.Cook[AnimalResource.Sheep];
            requestedTotals[AnimalResource.Boar] += data.Free[AnimalResource.Boar] + data.Cook[AnimalResource.Boar];
            requestedTotals[AnimalResource.Cattle] += data.Free[AnimalResource.Cattle] + data.Cook[AnimalResource.Cattle];

            var currentSheep = player.Farmyard.GetAnimalCount(AnimalResource.Sheep);

            // If the incoming total of animals in all assignments is different then the currently
            // assigned total animals plus the new animals being taken from the cache return false.
            if (requestedTotals[AnimalResource.Sheep] != currentSheep + (newAnimals != null && newAnimals.ContainsKey(AnimalResource.Sheep) ? newAnimals[AnimalResource.Sheep] : 0)
                || requestedTotals[AnimalResource.Boar] != player.Farmyard.GetAnimalCount(AnimalResource.Boar) + (newAnimals != null && newAnimals.ContainsKey(AnimalResource.Boar) ? newAnimals[AnimalResource.Boar] : 0)
                || requestedTotals[AnimalResource.Cattle] != player.Farmyard.GetAnimalCount(AnimalResource.Cattle) + (newAnimals != null && newAnimals.ContainsKey(AnimalResource.Cattle) ? newAnimals[AnimalResource.Cattle] : 0))
                return false;

            if (!manager.AreAssignmentsValid(data.Assignments))
                return false;

            var cooking = false;
            foreach (var count in data.Cook.Values)
            {
                if (count > 0) cooking = true;
            }

            if (cooking && !Curator.CanCook(player))
                return false;

            return true;
        }

        /// <summary>
        /// Applies the requested animal assignment data, cooking animals
        /// for food in the process
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        public static void AssignAnimals(AgricolaPlayer player, AnimalCacheActionData data, List<GameActionNotice> resultingNotices)
        {
            player.Farmyard.AssignAnimals(data.Assignments);

            var cooking = false;
            foreach (var count in data.Cook.Values)
            {
                if (count > 0)
                {
                    cooking = true;
                    break;
                }
            }

            var freedAnimalsPredicates = new List<INoticePredicate>();
            foreach (var animal in data.Free.Keys)
            {
                if (data.Free[animal] > 0)
                {
                    freedAnimalsPredicates.Add(new ResourceCache((Resource)animal, data.Free[animal]));
                }
            }
            if (freedAnimalsPredicates.Count > 0)
                resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.FreeAnimals.ToString(), freedAnimalsPredicates));
            

            if (cooking)
            {
                var conversions = Curator.GetHarvestFoodValues(player);
                var cookedAnimalPredicates = new List<INoticePredicate>();
                foreach (var animal in data.Cook.Keys)
                {
                    if (data.Cook[animal] > 0)
                    {
                        var definition = conversions.Where(x => x.InType.ToString() == animal.ToString()).OrderByDescending(x => x.OutAmount).First();
                        var inputCache = new ResourceCache((Resource)animal, data.Cook[animal]);
                        var outputCache = new ResourceCache(Resource.Food, definition.OutAmount * data.Cook[animal]);
                        player.AddResource(outputCache);
                        cookedAnimalPredicates.Add(new ConversionPredicate(inputCache, outputCache));
                    }
                }
                if(cookedAnimalPredicates.Count > 0)
                    resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Converted.ToString(), cookedAnimalPredicates));

            }
        }

        /// <summary>
        /// Checks if a player has the ability to cook any goods
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CanCook(AgricolaPlayer player, ResourceConversionData[] data)
        {
            var availableConversions = Curator.GetAnytimeResourceConversions(player);

            foreach (var conversion in data)
            {
                var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == conversion.Id
                    && x.InType == conversion.InType && x.InAmount == conversion.InAmount
                    && x.OutType == conversion.OutType);
                if (conversionDefinition == null)
                    return false;

                if (conversionDefinition.InLimit.HasValue && (conversionDefinition.InLimit.Value < conversion.Count / conversion.InAmount))
                    return false;

                if (conversion.Count % conversionDefinition.InAmount != 0)
                    return false;

                if (Enum.IsDefined(typeof(AnimalResource), conversionDefinition.InType.ToString()))
                {
                    var animalType = (AnimalResource)(Enum.Parse(typeof(AnimalResource), conversionDefinition.InType.ToString()));
                    if(player.Farmyard.AnimalManager.GetAnimalCount(animalType) <  conversion.Count)
                        return false;
                }
                else if (player.GetResource(conversionDefinition.InType) <  conversion.Count)
                {
                    return false;
                }
            }

            return true;
        }

        public static void Cook(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, ResourceConversionData[] data, List<GameActionNotice> resultingNotices)
        {
            var availableConversions = Curator.GetAnytimeResourceConversions(player);

            var trigger = new ResourceConversionTrigger();
            foreach (var conversion in data)
            {
                var conversionDefinition = availableConversions.Where(x => x.Id == conversion.Id
                    && x.InType == conversion.InType && x.InAmount == conversion.InAmount
                    && x.OutType == conversion.OutType).OrderByDescending(a => a.OutAmount).FirstOrDefault();


                if (!conversionDefinition.InType.IsAnimal())
                {
                    var inputCache = new ResourceCache(conversionDefinition.InType, -conversion.Count );
                    var outputCache = new ResourceCache(conversionDefinition.OutType, (conversion.Count / conversionDefinition.InAmount) * conversionDefinition.OutAmount);
                    // Deduct the cost
                    player.AddResource(inputCache);

                    if (!conversionDefinition.OutType.IsAnimal())
                    {
                        // Add the converted resources
                        player.AddResource(outputCache);
                    }
                    else
                    {
                        ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, (AnimalResource)conversionDefinition.OutType, conversionDefinition.OutAmount, resultingNotices));
                    }

                    inputCache = new ResourceCache(inputCache.Type, inputCache.Count * -1);
                    resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Converted.ToString(), new ConversionPredicate(inputCache, outputCache)));

                    trigger.AddConvertedResources(ResourcesConvertedData.FromResourceConversion(conversionDefinition, conversion.Count / conversionDefinition.InAmount));
                }
            }

            ProcessEventTrigger(player, trigger, resultingNotices);

            CheckTriggers(player, eventTriggers, resultingNotices);
         
        }

        /// <summary>
        /// Checks that the requested bake information is valid.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CanBake(AgricolaPlayer player, ImmutableArray<ResourceConversionData> data)
        {
            // Verify all the ids requested for bake actions are owned by the player
            var ownedCards = player.OwnedCardIds;
            if (data.Any(p => !ownedCards.Contains(p.Id)))
                return false;


            var totalGrainNeeded = 0;
            foreach (var p in data)
            {
                var card = ((AgricolaGame)player.Game).GetCard(p.Id);
                var definition = card.BakeProperties;
                if (definition.InLimit.HasValue 
                    && (p.Count / definition.InAmount) > definition.InLimit.Value)
                    return false;

                totalGrainNeeded += p.Count;
            }

            if (totalGrainNeeded > player.Grain)
                return false;

            return true;
        }

        /// <summary>
        /// Executes the requested bake operation.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        public static void Bake(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, ImmutableArray<ResourceConversionData> data)
        {
            if (data != null && data.Length > 0)
            {
                var bakeInput = new ResourceCache(Resource.Grain, 0);
                var bakeOutput = new ResourceCache(Resource.Food, 0);
                foreach (var bake in data)
                {
                    var card = ((AgricolaGame)player.Game).GetCard(bake.Id);
                    var resources = Curator.GetBakeOutput(player, bake.Id, bake.Count);
                    if (resources != null)
                    {
                        player.AddResource(card.BakeProperties.InType, -bake.Count);
                        player.AddResource(resources);

                        bakeInput = bakeInput.updateCount(bake.Count);
                        bakeOutput = bakeOutput.updateCount(resources.Count);
                    }
                }

                resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Bake.ToString(), new ConversionPredicate(bakeInput, bakeOutput)));

                var trigger = new BakeTrigger(bakeInput.Count);
                ProcessEventTrigger(player, trigger, resultingNotices);
                CheckTriggers(player, eventTriggers, resultingNotices);
            }
        }

        /// <summary>
        /// Checks if the fence data is valid.
        /// </summary>
        /// <param name="player">The player building the fences</param>
        /// <param name="data">Data defining the fences to be built</param>
        /// <param name="pastures">A list of pastures resulting from the fences built</param>
        /// <param name="costs">The costs of the requested fences</param>
        /// <returns>True if the fence data is valid</returns>
        public static bool CanBuildFences(AgricolaPlayer player, int actionId, BuildFencesActionData data, out List<int[]> pastures, out ResourceCache[] costs)
        {
            var fenceData = (BuildFencesActionData)data;

            pastures = null;
            if (!Curator.CanAffordFences(player, actionId, fenceData.Fences.Length, out costs))
                return false;

            var fenceValidator = new FencePlacementValidator(fenceData.Fences, player.Farmyard, out pastures);
            if (!fenceValidator.Valid || !player.Farmyard.PastureLocationsValid(pastures))
                return false;

            var tempAnimalManager = new AnimalManager();
            tempAnimalManager = tempAnimalManager.Update(player.Farmyard.Grid, pastures.ToImmutableArray());
            if (!ActionService.CanAssignAnimals(player, (AnimalCacheActionData)fenceData.AnimalData, tempAnimalManager, null))
                return false;

            return true;
        }

        public static void BuildFences(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, BuildFencesActionData data, ImmutableArray<int[]> pastures)
        {
            var oldPastureCount = player.Farmyard.Pastures.Length;

            BuildFencesTrigger trigger = null;
            if (data != null)
            {
                if (data.Fences != null)
                {

                    foreach (var fence in data.Fences)
                    {
                        player.Farmyard.AddFence(fence);
                    }

                    if (resultingNotices != null)
                    {
                        var predicates = new List<INoticePredicate>();
                        predicates.Add(new BuildPredicate(data.Fences.Length, Buildable.Fence));
                        predicates.Add(new BuildPredicate(pastures.Length - oldPastureCount, Buildable.Pasture));

                        resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Build.ToString(), predicates));
                    }
                    player.Farmyard.SetPastures(pastures);

                    if (data.Fences.Length > 0)
                    {
                        trigger = new BuildFencesTrigger(data.Fences.Length);
                    }

                }

                if (data.AnimalData != null)
                {
                    player.Farmyard.UpdateAnimalManager();
                    if (data.AnimalData != null)
                        ActionService.AssignAnimals(player, data.AnimalData, resultingNotices);
                }

                if(trigger != null)
                    ProcessEventTrigger(player, trigger, resultingNotices);

                CheckTriggers(player, eventTriggers, resultingNotices);
            }
        }


        public static bool CanBuyImprovement(AgricolaPlayer player, ImprovementActionData data)
        {
            ResourceCache[] costs;
            return CanBuyImprovement(player, data, out costs);
        }

        public static bool CanBuyImprovement(AgricolaPlayer player, ImprovementActionData data, out ResourceCache[] costs)
        {
            costs = null;
            var card = ((AgricolaGame)player.Game).GetCard(data.Id);
            if (!Curator.IsImprovementAvailable(player, data.Id) 
                || !Curator.CanAffordCard(player, data.Id, data.PaymentOption, out costs)
                || (card is MinorImprovement && !card.PrerequisitesMet(player)))
                return false;
            return true;
        }

        public static void BuyImprovement(AgricolaPlayer player, ImprovementActionData data, List<GameActionNotice> resultingNotices)
        {
            var improvementData = (ImprovementActionData)data;
            var card = ((AgricolaGame)player.Game).GetCard(improvementData.Id);
            var cost = Curator.GetCardCost(player, improvementData.Id, improvementData.PaymentOption);

            if (cost is ResourceCardCost)
            {
                var rcCost = (ResourceCardCost)cost;
                foreach (var resource in rcCost.Resources)
                {

                    if (resource.Type.IsAnimal())
                    {
                        player.Farmyard.RemoveAnimals((AnimalResource)resource.Type, resource.Count);
                    }
                    else
                    {
                        player.AddResource(resource.Type, -resource.Count);
                    }
                }
            }
            else if (cost is ReturnCardCardCost)
            {
                var miCost = (ReturnCardCardCost)cost;
                var ownedCardIds = player.OwnedCardIds;

                foreach (var id in miCost.Ids)
                {
                    if (ownedCardIds.Contains(id))
                    {
                        ((AgricolaGame)player.Game).ReturnCard(player, id);
                        break;
                    }
                }
            }
            else if (!(cost is FreeCardCost))
            {
                throw new NotImplementedException("Cost type not supported.");
            }

            // Apply the card
            if (card is MajorImprovement)
                ((AgricolaGame)player.Game).AssignMajorImprovement(improvementData.Id, player);
            else if (card is MinorImprovement)
            {
                if (((MinorImprovement)card).PassesLeft)
                    ((AgricolaGame)player.Game).PassCardLeft(player, (MinorImprovement)card);
                else
                    player.PlayCard(card);
            }

            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.PurchaseImprovement.ToString(), new IdPredicate(data.Id)));
            foreach (var evnt in card.OnPlayEvents)
            {
                evnt.Execute(player, null, card, resultingNotices);
            }


        }


        /// <summary>
        /// Validates data for a Plow AND sow action.  
        /// These are coupled together to validate sow 
        /// actions against a field added in the same plow
        /// action.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CanPlowAndSow(AgricolaPlayer player, int actionId, int[] fields, SowData[] sow, int? plowUsed = null)
        {
            if (fields.Length == 0 && sow.Length == 0) return false;
            if (fields.Length > 0)
            {
                var maxPlowable = 1;

                if(plowUsed.HasValue){
                    var cardId = plowUsed.Value;
                    var card = ((AgricolaGame)player.Game).GetCard(cardId);
                    if(!(card is MinorImprovement))
                        return false;

                    var minor = (MinorImprovement)card;

                    if (!player.OwnsCard(cardId) || minor.Plow == null || minor.Plow.Used >= minor.Plow.MaxUses)
                        return false;

                    maxPlowable = minor.Plow.Fields;
                }

                if (fields.Length > maxPlowable)
                    return false;
            }

            if (sow.Length > 0)
            {
                var grainNeeded = 0;
                var vegetablesNeeded = 0;
                foreach (var i in sow)
                {
                    if (i.Type == Resource.Grain)
                        grainNeeded++;
                    else if (i.Type == Resource.Vegetables)
                        vegetablesNeeded++;
                }

                if (player.Grain < grainNeeded || player.Vegetables < vegetablesNeeded)
                    return false;
            }
            return player.Farmyard.PlowAndSowLocationsValid(fields, sow);
        }


        public static bool CanSowAndBake(AgricolaPlayer player, ImmutableArray<SowData> sowData, ImmutableArray<ResourceConversionData> bakeData)
        {
            var grainNeeded = 0;
            var vegetablesNeeded = 0;

            if (bakeData == null && sowData == null)
                return false;

            if (bakeData != null)
            {
                // Verify all the ids requested for bake actions are owned by the player
                var ownedCards = player.OwnedCardIds;
                if (bakeData.Any(p => !ownedCards.Contains(p.Id)))
                    return false;


                foreach (var p in bakeData)
                {
                    var card = ((AgricolaGame)player.Game).GetCard(p.Id);
                    var definition = card.BakeProperties;
                    if (definition.InLimit.HasValue
                        && (p.Count / definition.InAmount) > definition.InLimit.Value)
                        return false;

                    grainNeeded += p.Count;
                }
            }

            if (sowData != null)
            {
                foreach (var i in sowData)
                {
                    if (i.Type == Resource.Grain)
                        grainNeeded++;
                    else if (i.Type == Resource.Vegetables)
                        vegetablesNeeded++;
                }
            }

            if (player.Grain < grainNeeded || player.Vegetables < vegetablesNeeded
                || (sowData == null || !player.Farmyard.SowLocationsValid(sowData)))
                return false;

            return true;
        }

        public static bool CanSow(AgricolaPlayer player, ImmutableArray<SowData> sowData)
        {
            var grainNeeded = 0;
            var vegetablesNeeded = 0;
            foreach (var i in sowData)
            {
                if (i.Type == Resource.Grain)
                    grainNeeded++;
                else if (i.Type == Resource.Vegetables)
                    vegetablesNeeded++;
            }

            if (player.Grain < grainNeeded || player.Vegetables < vegetablesNeeded
                || !player.Farmyard.SowLocationsValid(sowData))
                return false;

            return true;
        }

        public static void Plow(AgricolaPlayer player, int[] fields, List<GameActionNotice> resultingNotices, int? plowUsed = null)
        {
            if(fields.Length > 0){
                foreach (var field in fields)          
                    player.Farmyard.PlowField(field);

                var predicates = new List<INoticePredicate>();
                predicates.Add(new BuildPredicate(fields.Length, Buildable.Field));

                if (plowUsed != null && fields.Length > 1)
                {
                    var minor = (((AgricolaGame)player.Game).GetCard(plowUsed.Value) as MinorImprovement);
                    ImmutableDictionary<string, Object> metadata;
                    Plow plow = minor.Plow;
                    if (player.TryGetCardMetadata(minor, out metadata))
                        plow = metadata["plow"] as Plow;

                    player.SetCardMetadata(minor, metadata.SetItem("plow", plow.Use()));

                    predicates.Add(new StringPredicate(minor.Name));
                }

                resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Plow.ToString(), predicates));
            
            }
        }

        public static void Sow(AgricolaPlayer player, ImmutableArray<SowData> sowData, List<GameActionNotice> resultingNotices)
        {
            if (sowData.Length > 0)
            {
                var grains = 0;
                var vegetables = 0;
                foreach (var sow in sowData)
                {
                    player.AddResource(sow.Type, -1);
                    player.Farmyard.SowField(sow.Index, sow.Type);

                    if (sow.Type == Resource.Grain) grains++;
                    if (sow.Type == Resource.Vegetables) vegetables++;
                }

                var sowPredicates = new List<INoticePredicate>();
                if (grains > 0)
                {
                    sowPredicates.Add(new ResourceCache(Resource.Grain, grains));
                }
                if (vegetables > 0)
                {
                    sowPredicates.Add(new ResourceCache(Resource.Vegetables, vegetables));
                }

                resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Sow.ToString(), sowPredicates));
            }
        }

        public static bool CanRenovate(AgricolaPlayer player, out ResourceCache[] costs)
        {
            return Curator.CanAffordRenovation(player, out costs);
        }

        public static void Renovate(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            var costs = Curator.GetRenovationCost(player);
            foreach (var cost in costs)
                player.RemoveResource(cost);
            player.Farmyard.Renovate();

            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Renovate.ToString(), new StringPredicate(player.Farmyard.HouseType.ToString())));
        }

        public static bool CanPlayOccupation(AgricolaPlayer player, int actionId, int cardId)
        {

            var card = ((AgricolaGame)player.Game).GetCard(cardId);
            if (!player.HandOccupations.Contains(card))
                return false;

            ResourceCache[] costs = Curator.GetOccupationCost(player, actionId, cardId);
            if (!player.CanAfford(costs) || !card.PrerequisitesMet(player)) 
                return false;

            return true;
        }

        /// <summary>
        /// Puts an occupation into player for the given player. After all custom event triggers are fired,
        /// a PlayOccupationTrigger will be fired.
        /// </summary>
        /// <param name="player">The player who owns the occupation</param>
        /// <param name="eventTriggers">Any triggers that should be fired when this occupation is played</param>
        /// <param name="resultingNotices">A list of notices that should be sent to players</param>
        /// <param name="data">The data for the occupation being played</param>
        public static void PlayOccupation(AgricolaPlayer player, List<GameEventTrigger> eventTriggers, List<GameActionNotice> resultingNotices, OccupationActionData data)
        {
            var card = ((AgricolaGame)player.Game).GetCard(data.Id.Value);
            var cost = Curator.GetOccupationCost(player, data.ActionId, data.Id.Value);

            foreach (var resource in cost)
            {
                player.AddResource(resource.Type, -resource.Count);
            }
            player.PlayCard(data.Id.Value);

            foreach (var evnt in card.OnPlayEvents)
            {
                evnt.Execute(player, null, card, resultingNotices);
            }

            resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.PlayOccupation.ToString(), new IdPredicate(data.Id.Value)));

            CheckTriggers(player, eventTriggers, resultingNotices);


            var trigger = new PlayOccupationTrigger(card);
            ProcessEventTrigger(player, trigger, resultingNotices);

        }
            

    }
}