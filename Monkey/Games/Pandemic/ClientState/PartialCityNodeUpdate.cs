using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.ClientState
{
    public class PartialCityNodeUpdate
    {
        public int Id { get; set; }

        public void SetDisease(DiseaseColor color, int count){
            if(color == DiseaseColor.Red)
                redDisease = count;
            else if(color == DiseaseColor.Yellow)
                yellowDIsease = count;
            else if(color == DiseaseColor.Black)
                blackDisease = count;
            else if(color == DiseaseColor.Blue)
                blueDisease = count;

            diseaseCountsSet = true;
        }

        /// <summary>
        /// Get's an array of all the disease cube counts
        /// ordered by the disease color enum index.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] DiseaseCounters
        {
            get
            {
                return diseaseCountsSet ?
                    new int[] {
                        yellowDIsease,
                        redDisease,
                        blueDisease,
                        blackDisease
                    }
                : null;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ResearchStation
        {
            get;
            set;
        }

        private int redDisease;
        private int yellowDIsease;
        private int blueDisease;
        private int blackDisease;

        private bool diseaseCountsSet = false;
    }
}