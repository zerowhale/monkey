using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public class PersonalSupply
    {

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
        /// Adds a given quantity of a given resource to the cache
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        public void AddResource(Resource resource, Int32 count)
        {
            switch(resource){
                case Resource.Food: Food += count;
                    break;
                case Resource.Wood: Wood += count;
                    break;
                case Resource.Clay: Clay += count;
                    break;
                case Resource.Reed: Reed += count;
                    break;
                case Resource.Stone: Stone += count;
                    break;
                case Resource.Grain: Grain += count;
                    break;
                case Resource.Vegetables: Vegetables += count;
                    break;
            }
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

        public Int32 Food
        {
            get;
            set;
        }
       
        public Int32 Wood
        {
            get;
            set;
        }

        public Int32 Clay
        {
            get;
            set;
        }

        public Int32 Reed
        {
            get;
            set;
        }

        public Int32 Stone
        {
            get;
            set;
        }

        public Int32 Grain
        {
            get;
            set;
        }

        public Int32 Vegetables
        {
            get;
            set;
        }

        
    }
}