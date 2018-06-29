using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    /// <summary>
    /// This is a data tranfer object loaded from the client
    /// </summary>
    public class AnimalHousingData
    {
        public string Id
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public AnimalResource Type
        {
            get;
            set;
        }

        /// <summary>
        /// Given an array of assignments, this will tally of the totals of each animal.
        /// </summary>
        /// <param name="assignments"></param>
        /// <returns></returns>
        public static Dictionary<AnimalResource, int> GetTotals(AnimalHousingData[] assignments)
        {
            var totals = new Dictionary<AnimalResource, int>();
            totals[AnimalResource.Sheep] = 0;
            totals[AnimalResource.Boar] = 0;
            totals[AnimalResource.Cattle] = 0;

            foreach(var assignment in assignments){
                totals[assignment.Type] += assignment.Count;
            }

            return totals;
        }
    }
}