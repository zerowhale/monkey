using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using State = System.Collections.Immutable.ImmutableDictionary<string, object>;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BasicCacheAction: RoundAction
    {
        public BasicCacheAction(AgricolaGame game, Int32 id, Resource type, Int32 count, GameEventTrigger[] eventTriggers = null)
            : base(game, id, eventTriggers)
        {
            resourcesPerRound = new ResourceCache(type, count);
        }

        public override State RoundStart(State state)
        {
            State = AddCacheResources(state, resourcesPerRound);
            return State = base.RoundStart(State);
        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if(data != null && data.GetType() == typeof(CacheExchangeData)){
                var exchangeData = (CacheExchangeData)data;
                if (exchangeData.exchanges != null)
                {

                    var ownedCardIds = player.OwnedCardIds;
                    var requestedIds = exchangeData.exchanges.Where(x => x.Count > 0).Select(x => x.Id);
                    var ownsAllCards = requestedIds.Intersect(ownedCardIds).Count() == requestedIds.Count();

                    if (!ownsAllCards)
                        return false;

                    availableConversions = player.OwnedCards.Where(x => requestedIds.Contains(x.Id)).SelectMany(card => card.CacheExchanges);
                    var total = new Dictionary<Resource, int>();
                    foreach (var exchange in exchangeData.exchanges)
                    {
                        if (exchange.Count > 0)
                        {

                            var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == exchange.Id
                                && x.InType == exchange.InType && x.InAmount == exchange.InAmount
                                && x.OutType == exchange.OutType);
                            if (conversionDefinition == null)
                                return false;

                            if (conversionDefinition.InLimit.HasValue && conversionDefinition.InLimit.Value < exchange.Count / exchange.InAmount)
                                return false;

                            if (!total.ContainsKey(exchange.InType))
                                total[exchange.InType] = 0;

                            total[exchange.InType] += exchange.InAmount;
                        }
                    }

                    foreach (var kvp in total)
                    {
                        if (!GetCacheResources(State).ContainsKey(kvp.Key))
                            return false;

                        if (GetCacheResources(State)[kvp.Key].Count < kvp.Value)
                            return false;
                    }



                }
            }

            if (!base.CanExecute(player, data))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            if (data.GetType() == typeof(CacheExchangeData) && ((CacheExchangeData)data).exchanges != null)
            {
                
                var leave = new Dictionary<Resource, int>();
                var exchangeData = (CacheExchangeData)data;
                var gains = new Dictionary<Resource, int>();
                foreach (var exchange in exchangeData.exchanges)
                {
                    if (exchange.Count > 0)
                    {

                        // This only checks the resource conversions.
                        // Currently the activation evetns are only client side.
                        // I was too drunk to write the server side at the time.
                        var conversionDefinition = availableConversions.FirstOrDefault(x => x.Id == exchange.Id
                            && x.InType == exchange.InType && x.InAmount == exchange.InAmount
                            && x.OutType == exchange.OutType);


                        if (!leave.ContainsKey(exchange.InType))
                            leave[exchange.InType] = exchange.Count;
                        else
                            leave[exchange.InType] += exchange.Count;

                        var newAmount = (exchange.Count / exchange.InAmount) * conversionDefinition.OutAmount;
                        if (!gains.ContainsKey(exchange.OutType))
                            gains[exchange.OutType] = newAmount;
                        else
                            gains[exchange.OutType] += newAmount;
                    }
                }

                State = TakeCaches(State, player, leave, gains);
            }
            else
            {
                State = TakeCaches(State, player);
            }
            return this;
        }

        /// <summary>
        /// Non-state
        /// </summary>
        private IEnumerable<ResourceConversion> availableConversions;
    }

}