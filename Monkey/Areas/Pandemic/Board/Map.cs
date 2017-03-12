using BoardgamePlatform.Game.Utils;
using Monkey.Games.Pandemic.Notification;
using Monkey.Games.Pandemic.Board;
using Monkey.Games.Pandemic.ClientState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BoardgamePlatform.Game.Notification;

namespace Monkey.Games.Pandemic
{
    public class Map
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public Map()
        {
            InitializeCities();
            partialUpdate = new PartialMapUpdate();
        }


        public CityNode GetNode(City city)
        {
            return Cities[city];
        }


        /// <summary>
        /// Checks if two cities are linked.
        /// </summary>
        /// <param name="cityA">One of the cities</param>
        /// <param name="cityB">The other city</param>
        /// <returns>True if the cities are linked.</returns>
        public bool AreNeighbors(City cityA, City cityB)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Are Neighbors [{0}] [{1}].", cityA, cityB);

            var returnValue = false;
            if (cityA != cityB)
            {
                returnValue = Links.Where(x => x.A == cityA && x.B == cityB || x.B == cityA && x.A == cityB).Any();
            }
            if (logger.IsDebugEnabled)
                logger.DebugFormat("  [{0}]", returnValue);

            return returnValue;
        }


        /// <summary>
        /// Returns a list of cities linked to the provided city.
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public List<City> GetNeighbors(City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Get Neighbors [{0}]", city);

            var cities = new List<City>();
            foreach (var link in Links)
            {
                if (link.A == city)
                    cities.Add(link.B);
                else if (link.B == city)
                    cities.Add(link.A);
            }

            if (logger.IsDebugEnabled)
                logger.DebugFormat("  [{0}]", String.Join(",", cities.ToArray()));

            return cities;
        }

        public int AddDiseasesToCity(City city, DiseaseColor color, int count, List<City> immune, PandemicGame broadcaster)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Add Diseases To City [{0}] [{1}] [{2}]. Immune Cities: [{3}]", city, count, color, immune != null ? String.Join(",", immune.ToArray()) : "null");

            var outbreakCount = 0;
            var cityNode = Cities[city];

            if (immune == null)
                immune = new List<City>();

            if (!immune.Contains(city))
            {
                var numAdded = cityNode.TryAddDiseases(color, count);
                if(numAdded > 0 && broadcaster != null)
                    broadcaster.Broadcast((int)city, NoticeVerb.InfectionIncreased, new IntPredicate(numAdded));

                if (numAdded != count)
                {
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("  Couldn't add [{0}] [{1}]", count, color);

                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("  Outbreak in [{0}]", city);

                    if(broadcaster != null)
                        broadcaster.Broadcast((int)city, NoticeVerb.Outbreak);

                    outbreakCount++;
                    immune.Add(city);

                    var neighbors = GetNeighbors(city);
                    foreach (var neighbor in neighbors)
                    {
                        if (!immune.Contains(neighbor))
                        {
                            if (logger.IsDebugEnabled)
                                logger.DebugFormat("  Spreading to  [{0}]", neighbor);

                            outbreakCount += AddDiseasesToCity(neighbor, color, 1, immune, broadcaster);
                        }
                    }

                }
                else
                {
                    partialUpdate.AddCityUpdate(cityNode.GetUpdate());
                }
            }
            return outbreakCount;
        }

        public int AddDiseasesToCity(City city, int count, List<City> immune, PandemicGame broadcaster)
        {
            var cityNode = this.Cities[city];
            return AddDiseasesToCity(city, cityNode.Color, count, immune, broadcaster);
        }

        public PartialMapUpdate GetPartialUpdate(bool reset = true)
        {
            if (logger.IsInfoEnabled)
                logger.Info("Get Partial Update");

            var update = partialUpdate;
            if(reset)
                partialUpdate = new PartialMapUpdate();
            return update;
        }

        public bool HasResearchStation(City city)
        {
            return Cities[city].ResearchStation;
        }

        public bool HasDiseases(City city, DiseaseColor[] diseases)
        {
            return Cities[city].HasDiseases(diseases);
        }


        public int TreatAllDiseasesOfType(City city, DiseaseColor disease)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Treat All Diseases of Type [{0}] in [{1}]", disease, city);

            var curedCount = Cities[city].RemoveAllDiseasesOfType(disease);
            if (curedCount > 0)
            {
                partialUpdate.AddCityUpdate(Cities[city].GetUpdate());
            }
            return curedCount;

        }

        public Dictionary<DiseaseColor, int> TreatDiseases(City city, DiseaseColor[] diseases, bool[] cures, bool isMedic = false)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Treat Diseases [{0}] in [{1}]. Cured [{2}], as medic? [{3}]", String.Join(",", diseases), city, String.Join(",", cures), isMedic);

            Dictionary<DiseaseColor, int> removed = null;

            if (isMedic)
                removed = Cities[city].RemoveAllDiseasesOfTypes(diseases);
            else
                removed = Cities[city].RemoveDiseases(diseases, cures);

            partialUpdate.AddCityUpdate(Cities[city].GetUpdate());

            return removed;
        }


        /// <summary>
        /// Constructs a research station at the specified city
        /// </summary>
        /// <param name="city"></param>
        public void BuildResearchStation(City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Build Research Station in [{0}]", city);

            var node = Cities[city];
            if (node.ResearchStation)
            {
                var err = new InvalidOperationException("Can not build a research center on a city that already has one.");
                logger.Error(err);
                throw err;
            }

            node.ResearchStation = true;
            partialUpdate.AddCityUpdate(node.GetUpdate());
        }

        /// <summary>
        /// Removes a research station from the specified city.
        /// </summary>
        /// <param name="city"></param>
        public void RemoveResearchStation(City city)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Removing Research Station in [{0}]", city);

            var node = Cities[city];
            if (!node.ResearchStation)
            {
                var err = new InvalidOperationException("Can not remove a research center from a city that does not have one.");
                logger.Error(err);
                throw err;
            }

            node.ResearchStation = false;
            partialUpdate.AddCityUpdate(node.GetUpdate());
        }

        /// <summary>
        /// Gets the number of research stations currently built
        /// </summary>
        public int GetResearchStationCount()
        {
            var count = 0;
            foreach (var city in Cities)
            {
                if (city.Value.ResearchStation)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Checks if two cities are connected by shuttle (both have research centers).
        /// </summary>
        /// <param name="city1">One of the cities.</param>
        /// <param name="city2">The other city.</param>
        /// <returns></returns>
        public bool IsShuttleAvailable(City city1, City city2)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Is Shuttle Available between [{0}] and [{1}]", city1, city2);

            var ret = city1 != city2 
                && Cities[city1].ResearchStation && Cities[city2].ResearchStation;

            if (logger.IsDebugEnabled)
                logger.DebugFormat("  [{0}]", ret);

            return ret;
        }

        public bool IsEradicated(DiseaseColor color)
        {
            foreach(var city in Cities.Values){
                if (city.DiseaseCounters[(int)color] != 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a cities color.
        /// </summary>
        /// <param name="city">The city.</param>
        /// <returns>The color of the city.</returns>
        public DiseaseColor GetCityColor(City city)
        {
            return Cities[city].Color;
        }
        
        /// <summary>
        /// Gets the cities population.
        /// </summary>
        /// <param name="city">The city.</param>
        /// <returns>The population of the city.</returns>
        public int GetCityPopulation(City city)
        {
            return Cities[city].Population;
        }

        public bool DepletedDisease(out DiseaseColor? color)
        {
            int red = 24, black = 24, yellow = 24, blue = 24;

            foreach(var city in Cities.Values)
            {
                var diseaseCounterse = city.DiseaseCounters;
                red -= city.DiseaseCounters[(int)DiseaseColor.Red];
                black -= city.DiseaseCounters[(int)DiseaseColor.Black];
                yellow -= city.DiseaseCounters[(int)DiseaseColor.Yellow];
                blue -= city.DiseaseCounters[(int)DiseaseColor.Blue];
            }

            if(red < 0)
            {
                color = DiseaseColor.Red;
                return true;
            }

            if(black < 0)
            {
                color = DiseaseColor.Black;
                return true;
            }

            if(yellow < 0)
            {
                color = DiseaseColor.Yellow;
                return true;
            }

            if(blue < 0)
            {
                color = DiseaseColor.Blue;
                return true;
            }

            color = null;
            return false;
        }


        /// <summary>
        ///  List of cities on the map.
        /// </summary>
        public Dictionary<City, CityNode> Cities = new Dictionary<City, CityNode>();

        /// <summary>
        /// Builds the city list
        /// </summary>
        private void InitializeCities()
        {
            if (logger.IsInfoEnabled)
                logger.Info("Initialize Cities.");

            Cities[City.SanFrancisco] = new CityNode(City.SanFrancisco, "San Francisco", DiseaseColor.Blue, 249, 421, 5864000);
            Cities[City.Chicago] = new CityNode(City.Chicago, "Chicago", DiseaseColor.Blue, 410, 350, 9121000);
            Cities[City.Montreal] = new CityNode(City.Montreal, "Montreal", DiseaseColor.Blue, 521, 339, 3429000);
            Cities[City.NewYork] = new CityNode(City.NewYork, "New York", DiseaseColor.Blue, 668, 363, 20464000);
            Cities[City.London] = new CityNode(City.London, "London", DiseaseColor.Blue, 820, 250, 8586000);
            Cities[City.Essen] = new CityNode(City.Essen, "Essen", DiseaseColor.Blue, 964, 246, 575000);
            Cities[City.StPetersburg] = new CityNode(City.StPetersburg, "St. Petersburg", DiseaseColor.Blue, 1140, 223, 4879000);
            Cities[City.Atlanta] = new CityNode(City.Atlanta, "Atlanta", DiseaseColor.Blue, 445, 461, 4715000);
            Cities[City.Washington] = new CityNode(City.Washington, "Washington", DiseaseColor.Blue, 578, 445, 4679000);
            Cities[City.Madrid] = new CityNode(City.Madrid, "Madrid", DiseaseColor.Blue, 820, 421, 5427000);
            Cities[City.Paris] = new CityNode(City.Paris, "Paris", DiseaseColor.Blue, 935, 350, 10755000);
            Cities[City.Milan] = new CityNode(City.Milan, "Milan", DiseaseColor.Blue, 1024, 316, 5232000);
            Cities[City.LosAngeles] = new CityNode(City.LosAngeles, "Los Angeles", DiseaseColor.Yellow, 276, 523, 14900000);
            Cities[City.MexicoCity] = new CityNode(City.MexicoCity, "Mexico City", DiseaseColor.Yellow, 400, 569, 19463000);
            Cities[City.Miami] = new CityNode(City.Miami, "Miami", DiseaseColor.Yellow, 537, 548, 5582000);
            Cities[City.Bogota] = new CityNode(City.Bogota, "Bogota", DiseaseColor.Yellow, 524, 657, 8702000);
            Cities[City.Lima] = new CityNode(City.Lima, "Lima", DiseaseColor.Yellow, 456, 764, 9121000);
            Cities[City.Santiago] = new CityNode(City.Santiago, "Santiago", DiseaseColor.Yellow, 440, 864, 6015000);
            Cities[City.SaoPaulo] = new CityNode(City.SaoPaulo, "Sao Paulo", DiseaseColor.Yellow, 674, 764, 20186000);
            Cities[City.BuenosAires] = new CityNode(City.BuenosAires, "Buenos Aires", DiseaseColor.Yellow, 604, 864, 13639000);
            Cities[City.Lagos] = new CityNode(City.Lagos, "Lagos", DiseaseColor.Yellow, 904, 613, 11547000);
            Cities[City.Khartoum] = new CityNode(City.Khartoum, "Khartoum", DiseaseColor.Yellow, 1062, 586, 4887000);
            Cities[City.Kinshasa] = new CityNode(City.Kinshasa, "Kinshasa", DiseaseColor.Yellow, 984, 685, 9046000);
            Cities[City.Johannesburg] = new CityNode(City.Johannesburg, "Johannesburg", DiseaseColor.Yellow, 1057, 817, 3888000);
            Cities[City.Algiers] = new CityNode(City.Algiers, "Algiers", DiseaseColor.Black, 915, 475, 2946000);
            Cities[City.Istanbul] = new CityNode(City.Istanbul, "Istanbul", DiseaseColor.Black, 1054, 412, 13576000);
            Cities[City.Moscow] = new CityNode(City.Moscow, "Moscow", DiseaseColor.Black, 1174, 296, 15512000);
            Cities[City.Cairo] = new CityNode(City.Cairo, "Cairo", DiseaseColor.Black, 1044, 512, 14718000);
            Cities[City.Baghdad] = new CityNode(City.Baghdad, "Baghdad", DiseaseColor.Black, 1141, 452, 6204000);
            Cities[City.Tehran] = new CityNode(City.Tehran, "Terhan", DiseaseColor.Black, 1270, 363, 7419000);
            Cities[City.Riyadh] = new CityNode(City.Riyadh, "Riyadh", DiseaseColor.Black, 1147, 551, 5037000);
            Cities[City.Karachi] = new CityNode(City.Karachi, "Karachi", DiseaseColor.Black, 1242, 501, 20711000);
            Cities[City.Delhi] = new CityNode(City.Delhi, "Delhi", DiseaseColor.Black, 1358, 413, 22242000);
            Cities[City.Kolkata] = new CityNode(City.Kolkata, "Kolkata", DiseaseColor.Black, 1402, 481, 14374000);
            Cities[City.Mumbai] = new CityNode(City.Mumbai, "Mumbai", DiseaseColor.Black, 1253, 593, 16910000);
            Cities[City.Chennai] = new CityNode(City.Chennai, "Chennai", DiseaseColor.Black, 1347, 635, 8865000);
            Cities[City.Beijing] = new CityNode(City.Beijing, "Beijing", DiseaseColor.Red, 1490, 315, 17311000);
            Cities[City.Seoul] = new CityNode(City.Seoul, "Seoul", DiseaseColor.Red, 1594, 361, 22547000);
            Cities[City.Shanghai] = new CityNode(City.Shanghai, "Shanghai", DiseaseColor.Red, 1490, 420, 13482000);
            Cities[City.Tokyo] = new CityNode(City.Tokyo, "Tokyo", DiseaseColor.Red, 1686, 401, 13189000);
            Cities[City.Bangkok] = new CityNode(City.Bangkok, "Bangkok", DiseaseColor.Red, 1427, 587, 7151000);
            Cities[City.HongKong] = new CityNode(City.HongKong, "Hong Kong", DiseaseColor.Red, 1504, 527, 7106000);
            Cities[City.Taipei] = new CityNode(City.Taipei, "Taipei", DiseaseColor.Red, 1617, 547, 8338000);
            Cities[City.Osaka] = new CityNode(City.Osaka, "Osaka", DiseaseColor.Red, 1689, 492, 2871000);
            Cities[City.Jakarta] = new CityNode(City.Jakarta, "Jakarta", DiseaseColor.Red, 1432, 727, 26063000);
            Cities[City.HoChiMinhCity] = new CityNode(City.HoChiMinhCity, "Ho Chi Minh City", DiseaseColor.Red, 1517, 637, 8314000);
            Cities[City.Manila] = new CityNode(City.Manila, "Manila", DiseaseColor.Red, 1652, 655, 20767000);
            Cities[City.Sydney] = new CityNode(City.Sydney, "Sydney", DiseaseColor.Red, 1698, 829, 3785000);

            Cities[City.Atlanta].ResearchStation = true;
        }

        /// <summary>
        ///  List of all links between cities
        /// </summary>
        public CityNodeLink[] Links = new CityNodeLink[]{
            new CityNodeLink(City.SanFrancisco, City.Chicago),
            new CityNodeLink(City.SanFrancisco, City.LosAngeles),
            new CityNodeLink(City.Chicago, City.Montreal),
            new CityNodeLink(City.Chicago, City.Atlanta),
            new CityNodeLink(City.Montreal, City.NewYork),
            new CityNodeLink(City.Montreal, City.Washington),
            new CityNodeLink(City.NewYork, City.London),
            new CityNodeLink(City.NewYork, City.Madrid),
            new CityNodeLink(City.London, City.Essen),
            new CityNodeLink(City.London, City.Paris),
            new CityNodeLink(City.Essen, City.StPetersburg),
            new CityNodeLink(City.Essen, City.Milan),
            new CityNodeLink(City.StPetersburg, City.Moscow),
            new CityNodeLink(City.Atlanta, City.Washington),
            new CityNodeLink(City.Atlanta, City.Miami),
            new CityNodeLink(City.Washington, City.NewYork),
            new CityNodeLink(City.Madrid, City.Paris),
            new CityNodeLink(City.Madrid, City.London),
            new CityNodeLink(City.Madrid, City.Algiers),
            new CityNodeLink(City.Paris, City.Essen),
            new CityNodeLink(City.Paris, City.Milan),
            new CityNodeLink(City.Paris, City.Algiers),
            new CityNodeLink(City.Milan, City.Istanbul),
            new CityNodeLink(City.LosAngeles, City.Chicago),
            new CityNodeLink(City.LosAngeles, City.MexicoCity),
            new CityNodeLink(City.MexicoCity, City.Chicago),
            new CityNodeLink(City.MexicoCity, City.Miami),
            new CityNodeLink(City.MexicoCity, City.Bogota),
            new CityNodeLink(City.MexicoCity, City.Lima),
            new CityNodeLink(City.Miami, City.Washington),
            new CityNodeLink(City.Bogota, City.Miami),
            new CityNodeLink(City.Bogota, City.SaoPaulo),
            new CityNodeLink(City.Bogota, City.BuenosAires),
            new CityNodeLink(City.Lima, City.Bogota),
            new CityNodeLink(City.Lima, City.Santiago),
            new CityNodeLink(City.SaoPaulo, City.Madrid),
            new CityNodeLink(City.SaoPaulo, City.Lagos),
            new CityNodeLink(City.BuenosAires, City.SaoPaulo),
            new CityNodeLink(City.Lagos, City.Khartoum),
            new CityNodeLink(City.Lagos, City.Kinshasa),
            new CityNodeLink(City.Kinshasa, City.Khartoum),
            new CityNodeLink(City.Kinshasa, City.Johannesburg),
            new CityNodeLink(City.Johannesburg, City.Khartoum),
            new CityNodeLink(City.Algiers, City.Istanbul),
            new CityNodeLink(City.Algiers, City.Cairo),
            new CityNodeLink(City.Istanbul, City.Moscow),
            new CityNodeLink(City.Istanbul, City.Baghdad),
            new CityNodeLink(City.Istanbul, City.StPetersburg),
            new CityNodeLink(City.Moscow, City.Tehran),
            new CityNodeLink(City.Cairo, City.Istanbul),
            new CityNodeLink(City.Cairo, City.Baghdad),
            new CityNodeLink(City.Cairo, City.Riyadh),
            new CityNodeLink(City.Cairo, City.Khartoum),
            new CityNodeLink(City.Baghdad, City.Riyadh),
            new CityNodeLink(City.Baghdad, City.Karachi),
            new CityNodeLink(City.Baghdad, City.Tehran),
            new CityNodeLink(City.Riyadh, City.Karachi),
            new CityNodeLink(City.Tehran, City.Delhi),
            new CityNodeLink(City.Karachi, City.Delhi),
            new CityNodeLink(City.Karachi, City.Mumbai),
            new CityNodeLink(City.Karachi, City.Tehran),
            new CityNodeLink(City.Delhi, City.Kolkata),
            new CityNodeLink(City.Delhi, City.Chennai),
            new CityNodeLink(City.Kolkata, City.Bangkok),
            new CityNodeLink(City.Kolkata, City.HongKong),
            new CityNodeLink(City.Mumbai, City.Delhi),
            new CityNodeLink(City.Mumbai, City.Chennai),
            new CityNodeLink(City.Chennai, City.Kolkata),
            new CityNodeLink(City.Chennai, City.Bangkok),
            new CityNodeLink(City.Chennai, City.Jakarta),
            new CityNodeLink(City.Beijing, City.Seoul),
            new CityNodeLink(City.Beijing, City.Shanghai),
            new CityNodeLink(City.Seoul, City.Tokyo),
            new CityNodeLink(City.Shanghai, City.Seoul),
            new CityNodeLink(City.Shanghai, City.Tokyo),
            new CityNodeLink(City.Shanghai, City.HongKong),
            new CityNodeLink(City.Shanghai, City.Taipei),
            new CityNodeLink(City.Tokyo, City.SanFrancisco),
            new CityNodeLink(City.Tokyo, City.Osaka),
            new CityNodeLink(City.Bangkok, City.HongKong),
            new CityNodeLink(City.Bangkok, City.Jakarta),
            new CityNodeLink(City.Bangkok, City.HoChiMinhCity),
            new CityNodeLink(City.HongKong, City.Taipei),
            new CityNodeLink(City.HongKong, City.HoChiMinhCity),
            new CityNodeLink(City.HongKong, City.Manila),
            new CityNodeLink(City.Taipei, City.Osaka),
            new CityNodeLink(City.Taipei, City.Manila),
            new CityNodeLink(City.Jakarta, City.HoChiMinhCity),
            new CityNodeLink(City.Jakarta, City.Sydney),
            new CityNodeLink(City.HoChiMinhCity, City.Manila),
            new CityNodeLink(City.Manila, City.Sydney),
            new CityNodeLink(City.Manila, City.SanFrancisco),
            new CityNodeLink(City.Sydney, City.LosAngeles)
        };

        private PartialMapUpdate partialUpdate;



    }
}