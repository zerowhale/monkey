using BoardgamePlatform.Game.Utils;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Farmyard
    {
        public Farmyard(AgricolaPlayer player)
        {
            this.player = player;

            HouseType = HouseType.Wood;
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    grid[x, y] = new Empty();
                }
            }
        }

        public void Renovate()
        {
            if (this.HouseType == HouseType.Wood)
                this.HouseType = HouseType.Clay;
            else
                this.HouseType = HouseType.Stone;
        }

        public ResourceCache[] HarvestFields()
        {
            var yields = new Dictionary<Resource, ResourceCache>();
            foreach (var plot in grid)
            {
                if (plot is Field)
                {
                    var field = (Field)plot;
                    var yield = field.Harvest();

                    if (yield != null)
                    {
                        if (!yields.ContainsKey(yield.Type))
                            yields[yield.Type] = new ResourceCache(yield.Type, 0);

                        yields[yield.Type] = yields[yield.Type].updateCount(yield.Count);
                    }
                }
            }

            return yields.Values.ToArray();
        }

        public void AddFence(int index){
            fences.Add(index);
        }

        public void SetPastures(ImmutableArray<int[]> pastures){
            if (pastures != null)
            {
                this.pastures = pastures;
                foreach (var pasture in pastures)
                {
                    foreach (var plot in pasture)
                    {
                        var x = plot % WIDTH;
                        var y = (int)(plot / WIDTH);
                        if (!(grid[x, y] is Pasture))
                        {
                            var hasStable = ((Empty)grid[x, y]).HasStable;
                            grid[x, y] = new Pasture(hasStable);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Updates the animal manager.
        /// </summary>
        public void UpdateAnimalManager()
        {
            animalManager.Update(this.grid, this.pastures);
        }

        public int GetAnimalCount(AnimalResource Type)
        {
            return animalManager.GetAnimalCount(Type);
        }

        public void AssignAnimals(AnimalHousingData[] assignments)
        {
            animalManager.AssignAnimals(assignments);
        }

        public void AddRoom(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            AddRoom(x, y);
        }

        /// <summary>
        /// Attempts to set an empty plot to a room.  
        /// If the plot is already occupied this will not
        /// change it to a room.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if the plot was able to be set to a room</returns>
        public void AddRoom(int x, int y)
        {
            grid[x, y] = new Room();
            RoomCount = RoomLocations.Count;
        }

        public void AddStable(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            AddStable(x, y);
        }

        public void AddStable(int x, int y)
        {
            ((Empty)grid[x, y]).HasStable = true;
        }

        public void PlowField(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            PlowField(x, y);
        }

        public void PlowField(int x, int y)
        {
            grid[x, y] = new Field();
        }

        public int PlantedResourceCount(Resource type)
        {
            var planted = 0;
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = grid[x, y];
                    if (plot is Field)
                    {
                        var field = (Field)plot;
                        if (field.Sown.Type == type)
                            planted += field.Sown.Count;
                    }
                }
            }
            return planted;
        }

        public Boolean SowField(int index, Resource resource)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return SowField(x, y, resource);
        }

        public Boolean SowField(int x, int y, Resource resource)
        {
            if (grid[x, y] is Field)
            {
                var field = (Field)grid[x, y];
                field.Sow(player, resource);
                return true;
            }
            return false;
        }

        public bool CanFencePasture()
        {
            var fenceCount = fences.Count;
            if (fenceCount == MAX_FENCES)
                return false;

            var fencesLeft = MAX_FENCES - fenceCount;
            var borderingFences = new FenceInfo[4];

            var minFencesRequired = 5;

            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = grid[x, y];

                    if (plot is Pasture || plot is Empty)
                    {
                        var fencesRequired = 0;
                        var plotData = FenceUtils.GetPlotBorderingFenceData(x, y);

                        if (!fences.Contains(plotData.NorthFence.Index))
                            fencesRequired++;

                        if (!fences.Contains(plotData.EastFence.Index))
                            fencesRequired++;

                        if (!fences.Contains(plotData.SouthFence.Index))
                            fencesRequired++;

                        if (!fences.Contains(plotData.WestFence.Index))
                            fencesRequired++;

                        if (fencesRequired != 0 && fencesRequired < minFencesRequired)
                            minFencesRequired = fencesRequired;
                    }

                }
            }

            // this will only be greater than 4 if there are no valid locations to put a fence at all
            if (minFencesRequired > 4 || minFencesRequired > fencesLeft)
                return false;

            return true;
        }

        public Boolean RoomLocationsValid(int index)
        {
            return RoomLocationsValid(ImmutableArray.Create<int>(index));
        }

        public Boolean RoomLocationsValid(ImmutableArray<int> indices)
        {
            var closed = new List<Point>();

            for(var x=0;x<WIDTH;x++){
                for(var y=0;y<HEIGHT;y++){
                    var plot = grid[x, y];

                    if (plot is Field
                        || plot is Pasture
                        || (plot is Empty && ((Empty)plot).HasStable))
                        closed.Add(new Point(x,y));

                }
            }
            
            var proposed = new List<Point>();
            foreach (var i in indices)
            {
                proposed.Add(IndexToCoords(i));
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, RoomLocations, closed, proposed);
        }

        public Boolean PastureLocationsValid(List<int[]> pastures)
        {
            var closed = new List<Point>();

            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = grid[x, y];
                    if (plot is Field || plot is Room)
                        closed.Add(new Point(x, y));

                }
            }

            var proposed = new List<Point>();
            foreach(var list in pastures){
                foreach(var index in list){
                    proposed.Add(IndexToCoords(index));
                }
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, null, closed, proposed);
        }

        public Boolean FieldLocationsValid(int[] indices)
        {
            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = grid[x, y];
                    if (!(plot is Field)
                        && !(plot is Empty && ((Empty)plot).HasStable))
                        closed.Add(new Point(x, y));
                    

                }
            }

            var proposed = new List<Point>();
            foreach(var i in indices){
                proposed.Add(IndexToCoords(i));
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, FieldLocations, closed, proposed);
        }



        public Boolean StablesLocationsValid(ImmutableArray<int> stablesIndices)
        {
            if (this.StableLocations.Count > MAX_STABLES)
                return false;

            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = grid[x, y];
                    if (plot is Field
                        || plot is Pasture
                        || (plot is Empty && ((Empty)plot).HasStable))
                    {
                        closed.Add(new Point(x, y));
                    }
                }
            }


            foreach (var i in stablesIndices)
            {
                if (i < 0 || i >= WIDTH * HEIGHT)
                    return false;

                var x = i % WIDTH;
                var y = (int)(i / WIDTH);

                if (!(grid[x, y] is Empty) && !(grid[x, y] is Pasture) || ((Empty)grid[x, y]).HasStable)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Validates that both plow and sow data are valid, including checks for sowing
        /// proposed plow locations.
        /// </summary>
        /// <param name="fieldIndices"></param>
        /// <param name="sowIndices"></param>
        /// <returns></returns>
        public Boolean PlowAndSowLocationsValid(int[] fieldIndices, SowData[] sowIndices)
        {
            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT)
                        return false;

                    var plot = grid[x, y];
                    if (plot is Room
                        || plot is Pasture
                        || (plot is Empty && ((Empty)plot).HasStable))
                    {
                        closed.Add(new Point(x, y));
                    }
                }
            }

            var proposed = new List<Point>();
            foreach (var i in fieldIndices)
            {
                proposed.Add(IndexToCoords(i));
            }

            var fieldLocations = FieldLocations;
            if (!FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, fieldLocations, closed, proposed))
                return false;

            foreach (var sowData in sowIndices)
            {
                var i = sowData.Index;
                if (i < 0 || i > WIDTH * HEIGHT)
                    return false;

                var x = i % WIDTH;
                var y = (int)(i / WIDTH);

                if (!fieldIndices.Contains(i) && !fieldLocations.Contains(new Point(x, y)))
                    return false;

                if (fieldLocations.Contains(new Point(x, y)))
                {
                    if (((Field)grid[x, y]).Sown.Count > 0)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if proposed sow locations are valid against
        /// existing fields only.
        /// </summary>
        /// <param name="sowIndices"></param>
        /// <returns></returns>
        public Boolean SowLocationsValid(ImmutableArray<SowData> sowIndices)
        {
            var fieldLocations = FieldLocations;
            foreach (var sowData in sowIndices)
            {
                var i = sowData.Index;
                var x = i % WIDTH;
                var y = (int)(i / WIDTH);

                if (!fieldLocations.Contains(new Point(x, y)))
                    return false;

                if (((Field)grid[x, y]).Sown.Count > 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Number of rooms built.
        /// </summary>
        [JsonIgnore]
        public int RoomCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Used by the front end to assign animals in the ui
        /// </summary>
        public AnimalHousing[] AnimalLocations
        {
           get { return animalManager.GetOccupiedHousings(); }
        }

        /// <summary>
        /// The animal manager for this farmyard
        /// </summary>
        [JsonIgnore]
        public AnimalManager AnimalManager
        {
            get { return animalManager; }
        }

        [JsonIgnore]
        public List<Point> EmptyLocations
        {
            get
            {
                var locations = new List<Point>();

                for (var x = 0; x < WIDTH; x++)
                {
                    for (var y = 0; y < HEIGHT; y++)
                    {
                        if (grid[x, y] is Empty && !(grid[x,y] is Pasture) && !((Empty)grid[x,y]).HasStable)
                            locations.Add(new Point(x, y));
                    }
                }
                return locations;
            }
        }

        /// <summary>
        /// Listing of all room locations (as x,y coordinates).
        /// </summary>
        [JsonIgnore]
        public List<Point> RoomLocations
        {
            get
            {
                var locations = new List<Point>();

                for (var x = 0; x < WIDTH; x++)
                {
                    for (var y = 0; y < HEIGHT; y++)
                    {
                        if (grid[x, y] is Room)
                            locations.Add(new Point(x, y));
                    }
                }

                return locations;
            }
        }

        /// <summary>
        /// Listing of all field locations (as x,y coordinates).
        /// </summary>
        [JsonIgnore]
        public List<Point> FieldLocations
        {
            get
            {
                var locations = new List<Point>();

                for (var x = 0; x < WIDTH; x++)
                {
                    for (var y = 0; y < HEIGHT; y++)
                    {
                        if (grid[x, y] is Field)
                            locations.Add(new Point(x, y));
                    }
                }

                return locations;
            }
        }

        /// <summary>
        /// Listing of all stable locations (as x,y coordinates).
        /// </summary>
        [JsonIgnore]
        public List<Point> StableLocations
        {
            get
            {
                var locations = new List<Point>();

                for (var x = 0; x < WIDTH; x++)
                {
                    for (var y = 0; y < HEIGHT; y++)
                    {
                        if (grid[x, y] is Empty && ((Empty)grid[x,y]).HasStable)
                            locations.Add(new Point(x, y));
                    }
                }

                return locations;
            }
        }

        /// <summary>
        /// Listing of all pasture locations (as x,y coordinates).
        /// </summary>
        [JsonIgnore]
        public List<Point> PastureLocations
        {
            get
            {
                var locations = new List<Point>();

                for (var x = 0; x < WIDTH; x++)
                {
                    for (var y = 0; y < HEIGHT; y++)
                    {
                        if (grid[x, y] is Pasture)
                            locations.Add(new Point(x, y));
                    }
                }

                return locations;
            }
        }

        /// <summary>
        /// List of all pastures
        /// </summary>
        public ImmutableArray<int[]> Pastures
        {
            get { return pastures; }
        }

        /// <summary>
        /// The type of house (wood, clay or stone).
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))] 
        public HouseType HouseType
        {
            get;
            set;
        }

        /// <summary>
        /// The underlying grid that holds all the farmyard 
        /// layout information
        /// </summary>
        public FarmyardEntity[,] Grid
        {
            get { return grid; }
        }

        /// <summary>
        /// An array of all the fence locations (as indices);
        /// </summary>
        public int[] Fences
        {
            get { return fences.ToArray(); }
        }

        public const int WIDTH = 5;
        public const int HEIGHT = 3;
        public const int MAX_STABLES = 4;
        public const int MAX_FENCES = 15;
        public const int FENCES_WIDTH = 6;
        public const int FENCES_HEIGHT = 7;
        

        private void IndexToCoords(int index, out int x, out int y)
        {
            x = index % WIDTH;
            y = (int)(index / WIDTH);
        }

        private Point IndexToCoords(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return new Point(x, y);
        }



        /// <summary>
        /// Listing of the Pastures, which can be comprised of
        /// 1 or more plots.
        /// </summary>
        private ImmutableArray<int[]> pastures = ImmutableArray<int[]>.Empty;

        private FarmyardEntity[,] grid = new FarmyardEntity[WIDTH, HEIGHT];
        private List<int> fences = new List<int>();

        private AnimalManager animalManager = new AnimalManager();

        private AgricolaPlayer player;
    }

}