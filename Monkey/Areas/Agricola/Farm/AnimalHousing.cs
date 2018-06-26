using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    /// <summary>
    /// Immutable class that represents an animal housing
    /// </summary>
    public class AnimalHousing
    {
        
        public AnimalHousing(string id, int capacity, AnimalResource? animalType = null, int count = 0) {
            Id = id;
            Capacity = capacity;
            AnimalType = animalType;
            AnimalCount = count;
        }
        
        /// <summary>
        /// Sets the quantity and type of animals in the housing and returns 
        /// the new object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public AnimalHousing SetAnimals(AnimalResource type, int count){
            if(count > Capacity)
                throw new ArgumentOutOfRangeException("count");

            return new AnimalHousing(Id, Capacity, type, count);
        }

        /// <summary>
        /// Returns a copy of this housing with no animals assigned to it
        /// </summary>
        /// <returns></returns>
        public AnimalHousing Empty()
        {
            return new AnimalHousing(Id, Capacity, AnimalType, 0);
        }

        /// <summary>
        /// True if the housing has no animals in it
        /// </summary>
        /// <returns></returns>
        public Boolean IsEmpty(){
            return AnimalCount == 0;
        }

        /// <summary>
        /// Identifer of this housing
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Total number of animals that fit in this housing
        /// </summary>
        public int Capacity { get; }
        
        /// <summary>
        /// Type of animal in this housing
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AnimalResource? AnimalType { get; }

        /// <summary>
        /// Number of animals in this housing
        /// </summary>
        public int AnimalCount { get; }

        /// <summary>
        /// Amount of space left in this housing
        /// </summary>
        [JsonIgnore]
        public int RemainingCapacity
        {
            get { return Capacity - AnimalCount; }
        }

    }
}