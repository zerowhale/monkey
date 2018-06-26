using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public sealed class PersonalSupply
    {
       
        private PersonalSupply()
        {

        }

        private PersonalSupply(Int32 wood, Int32 clay, Int32 stone, Int32 reed, Int32 grain, Int32 vegetables, Int32 food) 
        {
            this.Wood = wood;
            this.Clay = clay;
            this.Stone = stone;
            this.Reed = reed;
            this.Grain = grain;
            this.Vegetables = vegetables;
            this.Food = food;
        }

        public static PersonalSupply Empty
        {
            get { return emptyPersonalSupply; }
        }

        /// <summary>
        ///  Adds the cache to the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public PersonalSupply AddResource(ResourceCache cache)
        {
            return this.AddResource(cache.Type, cache.Count);
        }

        /// <summary>
        /// Subtracts the cache from the personal supply
        /// </summary>
        /// <param name="cache"></param>
        public PersonalSupply RemoveResource(ResourceCache cache)
        {
            return this.AddResource(cache.Type, -cache.Count);
        }
        
        /// <summary>
        /// Adds a given quantity of a given resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public PersonalSupply AddResource(Resource resource, Int32 count)
        {
            var wood = this.Wood;
            var clay = this.Clay;
            var stone = this.Stone;
            var reed = this.Reed;
            var grain = this.Grain;
            var vegetables = this.Vegetables;
            var food = this.Food;

            switch(resource){
                case Resource.Food:
                    food += count;
                    break;
                case Resource.Wood:
                    wood += count;
                    break;
                case Resource.Clay:
                    clay += count;
                    break;
                case Resource.Reed:
                    reed += count;
                    break;
                case Resource.Stone:
                    stone += count;
                    break;
                case Resource.Grain:
                    grain += count;
                    break;
                case Resource.Vegetables:
                    vegetables += count;
                    break;
            }
            return new PersonalSupply(wood, clay, stone, reed, grain, vegetables, food);
        }

        /// <summary>
        /// Gets the quantity of a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public int GetResource(Resource resource)
        {
            switch (resource)
            {
                case Resource.Food:
                    return this.Food;
                case Resource.Wood:
                    return this.Wood;
                case Resource.Clay:
                    return this.Clay;
                case Resource.Reed:
                    return this.Reed;
                case Resource.Stone:
                    return this.Stone;
                case Resource.Grain:
                    return this.Grain;
                case Resource.Vegetables:
                    return this.Vegetables;
            }
            return 0;
        }

        [JsonProperty]
        public Int32 Food { get; }

        [JsonProperty]
        public Int32 Wood { get; }

        [JsonProperty]
        public Int32 Clay { get; }

        [JsonProperty]
        public Int32 Reed { get; }

        [JsonProperty]
        public Int32 Stone { get; }

        [JsonProperty]
        public Int32 Grain { get; }

        [JsonProperty]
        public Int32 Vegetables { get; }

        private static readonly PersonalSupply emptyPersonalSupply = new PersonalSupply();
    }
}