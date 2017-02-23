using Monkey.Games.Pandemic.ClientState;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Board
{
    public class CityNode
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public CityNode(City city, string name, DiseaseColor color, int x, int y, int population)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("New City Node [{0}], Name: [{1}], Color: [{2}], X: [{3}], Y:[{4}], Pop:[{5}].", city, name, color, x, y, population);
            
            City = city;
            Name = name;
            Color = color;
            X = x;
            Y = y;
            Population = population;
            var colors = Enum.GetValues(typeof(DiseaseColor)).Cast<DiseaseColor>();
            foreach (var c in colors)
                diseaseCubes[c] = 0;

            partialUpdate.Id = Id;
        }

        /// <summary>
        /// Attempts to add the given number of disease cubes of the given color to the city.
        /// If the city already has maxed out the number of cubes then the addition fails and false
        /// is returned.
        /// </summary>
        /// <param name="disease">The color of the disease to add</param>
        /// <param name="count">The number of the cubes to add</param>
        /// <returns></returns>
        public int TryAddDiseases(DiseaseColor disease, int count)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Try Add Diseases [{0}] [{1}]", count, disease);

            var added = diseaseCubes[disease] + count > PandemicGame.DISEASE_COUNT_FOR_OUTBREAK ?
                PandemicGame.DISEASE_COUNT_FOR_OUTBREAK - diseaseCubes[disease] :
                count;

            diseaseCubes[disease] += count;
            if (diseaseCubes[disease] > PandemicGame.DISEASE_COUNT_FOR_OUTBREAK)
            {
                diseaseCubes[disease] = PandemicGame.DISEASE_COUNT_FOR_OUTBREAK;
                return added;
            }

            partialUpdate.SetDisease(disease, diseaseCubes[disease]);
            return added;
        }

        public Dictionary<DiseaseColor, int> RemoveDiseases(DiseaseColor[] diseases, bool[] cures)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Remove Diseases [{0}]. Cures: [{1}].", String.Join(",", diseases), String.Join(",", cures));

            var removed = new Dictionary<DiseaseColor, int>();

            foreach (var disease in diseases)
            {
                var currentDiseaseCount = diseaseCubes[disease];
                if (diseaseCubes[disease] == 0)
                {
                    var err = new InvalidOperationException("Can't remove a disease from a city that does not have that disease");
                    logger.Error(err);
                    throw err;
                }


                if (cures[(int)disease])
                {
                    diseaseCubes[disease] = 0;
                    removed[disease] = currentDiseaseCount;
                }
                else
                {
                    diseaseCubes[disease]--;

                    if (!removed.Keys.Contains(disease))
                        removed[disease] = 0;
                    removed[disease]++;

                }
                partialUpdate.SetDisease(disease, diseaseCubes[disease]);
            }

            return removed;
        }

        /// <summary>
        /// Attempts to remove all diseases of a color from the city.
        /// </summary>
        /// <param name="disease"></param>
        /// <returns>The number of diseases of that color that were treated.</returns>
        public int RemoveAllDiseasesOfType(DiseaseColor disease)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Remove All Diseases Of Type [{0}]", disease);

            var count = diseaseCubes[disease];
            if (count > 0)
            {
                diseaseCubes[disease] = 0;
                partialUpdate.SetDisease(disease, 0);
            }
            return count;
        }

        public Dictionary<DiseaseColor, int> RemoveAllDiseasesOfTypes(DiseaseColor[] diseases)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Remove All Diseases of Types [{0}]",String.Join(",", diseases));

            var removed = new Dictionary<DiseaseColor, int>();

            foreach (var disease in diseases)
            {
                removed[disease] = RemoveAllDiseasesOfType(disease);
            }

            return removed;
        }

        public bool HasDiseases(DiseaseColor[] diseases)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Has Diseases [{0}]",String.Join(",", diseases));

            var diseaseTotals = DiseaseCounters;
            foreach(var disease in diseases){
                diseaseTotals[(int)disease]--;
                if (diseaseTotals[(int)disease] < 0)
                {
                    if (logger.IsDebugEnabled)
                        logger.Debug("  [false]");

                    return false;
                }
            }
            if (logger.IsDebugEnabled)
                logger.Debug("  [true]");
            return true;
        }

        public PartialCityNodeUpdate GetUpdate()
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Get Update");

            return partialUpdate;
        }

        public int Id
        {
            get
            {
                return (int)City;
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Id number
        /// </summary>
        public City City
        {
            get;
            private set;
        }

        /// <summary>
        /// City name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public DiseaseColor Color
        {
            get;
            private set;
        }

        /// <summary>
        /// X Coordinate on the map
        /// </summary>
        public int X
        {
            get;
            private set;
        }

        /// <summary>
        /// Y Coordinate on the map
        /// </summary>
        public int Y
        {
            get;
            private set;
        }

        /// <summary>
        /// Get's an array of all the disease cube valleys
        /// ordered by the disease color enum index.
        /// </summary>
        public int[] DiseaseCounters
        {
            get
            {
                return new int[] {
                    diseaseCubes[DiseaseColor.Yellow], 
                    diseaseCubes[DiseaseColor.Red],
                    diseaseCubes[DiseaseColor.Blue],
                    diseaseCubes[DiseaseColor.Black]
                };
            }
        }

        /// <summary>
        /// Whether or not there is a research station in this sity
        /// </summary>
        public bool ResearchStation
        {
            get
            {
                return researchStation;
            }
            set
            {
                researchStation = value;
                partialUpdate.ResearchStation = value;
            }
        }

        /// <summary>
        /// Population of the city
        /// </summary>
        [JsonIgnore]
        public int Population
        {
            get;
            private set;
        }

        /// <summary>
        /// Counters for disease cubes in this city
        /// </summary>
        private Dictionary<DiseaseColor, int> diseaseCubes = new Dictionary<DiseaseColor,int>();
        private PartialCityNodeUpdate partialUpdate = new PartialCityNodeUpdate();

        private bool researchStation = false;
    }
}