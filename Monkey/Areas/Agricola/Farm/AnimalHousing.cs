using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class AnimalHousing
    {
        
        public AnimalHousing(string id, int capacity) {
            Id = id;
            Capacity = capacity;
        }

        public void SetAnimals(AnimalResource type, int count){
            if(count > Capacity)
                throw new ArgumentOutOfRangeException("count");

            AnimalType = type;
            AnimalCount = count;
        }

        public void Empty()
        {
            this.AnimalCount = 0;
        }

        public Boolean IsEmpty(){
            return AnimalCount == 0;
        }

        public AnimalHousing Clone()
        {
            var housing = new AnimalHousing(this.Id, this.Capacity);
            housing.AnimalType = this.AnimalType;
            housing.AnimalCount = this.AnimalCount;
            return housing;
        }

        public string Id
        {
            get;
            private set;
        }

        public int Capacity
        {
            get;
            private set;
        }


        [JsonConverter(typeof(StringEnumConverter))]
        public AnimalResource AnimalType
        {
            get;
            private set;
        }

        public int AnimalCount
        {
            get;
            private set;
        }

        [JsonIgnore]
        public int RemainingCapacity
        {
            get { return Capacity - AnimalCount; }
        }

    }
}