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
        public Farmyard()
        {
            HouseType = HouseType.Wood;
            var gridBuilder = new List<FarmyardEntity>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    gridBuilder.Add(new Empty(x, y));
                }
            }
            Fences = ImmutableList<int>.Empty;
            Pastures = ImmutableArray<int[]>.Empty;
            AnimalManager = new AnimalManager();
            Grid = gridBuilder.ToImmutableList();
        }

        private Farmyard(
            ImmutableList<FarmyardEntity> grid,
            ImmutableArray<int[]> pastures,
            ImmutableList<int> fences,
            AnimalManager animalManager,
            HouseType houseType)
        {
            this.Pastures = pastures;
            this.Grid = grid;
            this.Fences = fences;
            this.AnimalManager = animalManager;
            this.HouseType = houseType;
        }


        public bool CanFencePasture()
        {
            var fenceCount = Fences.Count;
            if (fenceCount == MAX_FENCES)
                return false;

            var fencesLeft = MAX_FENCES - fenceCount;
            var borderingFences = new FenceInfo[4];

            var minFencesRequired = 5;

            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = Grid[y * WIDTH + x];

                    if (plot is Pasture || plot is Empty)
                    {
                        var fencesRequired = 0;
                        var plotData = FenceUtils.GetPlotBorderingFenceData(x, y);

                        if (!Fences.Contains(plotData.NorthFence.Index))
                            fencesRequired++;

                        if (!Fences.Contains(plotData.EastFence.Index))
                            fencesRequired++;

                        if (!Fences.Contains(plotData.SouthFence.Index))
                            fencesRequired++;

                        if (!Fences.Contains(plotData.WestFence.Index))
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

        public Boolean IsValidRoomLocations(int index)
        {
            return IsValidRoomLocations(ImmutableArray.Create<int>(index));
        }

        public Boolean IsValidRoomLocations(ImmutableArray<int> indices)
        {
            var closed = new List<Point>();

            for (var x = 0; x < WIDTH; x++) {
                for (var y = 0; y < HEIGHT; y++) {
                    var plot = Grid[y * WIDTH + x];

                    if (plot is Field
                        || plot is Pasture
                        || (plot is Empty && ((Empty)plot).HasStable))
                        closed.Add(new Point(x, y));

                }
            }

            var proposed = new List<Point>();
            foreach (var i in indices)
            {
                proposed.Add(IndexToCoords(i));
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, RoomLocations, closed, proposed);
        }

        public Boolean IsValidPastureLocations(List<int[]> pastures)
        {
            var closed = new List<Point>();

            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = Grid[y * WIDTH + x];
                    if (plot is Field || plot is Room)
                        closed.Add(new Point(x, y));

                }
            }

            var proposed = new List<Point>();
            foreach (var list in pastures) {
                foreach (var index in list) {
                    proposed.Add(IndexToCoords(index));
                }
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, null, closed, proposed);
        }

        public Boolean IsValidFieldLocations(int[] indices)
        {
            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = Grid[y * WIDTH + x];
                    if (!(plot is Field)
                        && !(plot is Empty && ((Empty)plot).HasStable))
                        closed.Add(new Point(x, y));


                }
            }

            var proposed = new List<Point>();
            foreach (var i in indices) {
                proposed.Add(IndexToCoords(i));
            }

            return FarmyardPlacementValidator.AreValidPlots(WIDTH, HEIGHT, FieldLocations, closed, proposed);
        }

        public Boolean IsValidStablesLocations(ImmutableArray<int> stablesIndices)
        {
            if (this.StableLocations.Count > MAX_STABLES)
                return false;

            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = Grid[y * WIDTH + x];
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

                if (!(Grid[y * WIDTH + x] is Empty) && !(Grid[y * WIDTH + x] is Pasture) || ((Empty)Grid[y * WIDTH + x]).HasStable)
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
        public Boolean IsValidPlowAndSowLocations(int[] fieldIndices, SowData[] sowIndices)
        {
            var closed = new List<Point>();
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT)
                        return false;

                    var plot = Grid[y * WIDTH + x];
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
                    if (((Field)Grid[y * WIDTH + x]).Sown.Count > 0)
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
        public Boolean IsValidSowLocations(ImmutableArray<SowData> sowIndices)
        {
            var fieldLocations = FieldLocations;
            foreach (var sowData in sowIndices)
            {
                var i = sowData.Index;
                var x = i % WIDTH;
                var y = (int)(i / WIDTH);

                if (!fieldLocations.Contains(new Point(x, y)))
                    return false;

                if (((Field)Grid[y * WIDTH + x]).Sown.Count > 0)
                    return false;
            }

            return true;
        }

        public int AnimalCount(AnimalResource Type)
        {
            return AnimalManager.GetAnimalCount(Type);
        }

        public int PlantedResourceCount(Resource type)
        {
            var planted = 0;
            for (var x = 0; x < WIDTH; x++)
            {
                for (var y = 0; y < HEIGHT; y++)
                {
                    var plot = Grid[y * WIDTH + x];
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

        /// <summary>
        /// Number of rooms built.
        /// </summary>
        [JsonIgnore]
        public int RoomCount
        {
            get { return Grid.Count(x => x.GetType() == typeof(Room)); }
        }

        /// <summary>
        /// Used by the front end to assign animals in the ui
        /// </summary>
        public ImmutableArray<AnimalHousing> AnimalLocations
        {
            get { return AnimalManager.GetOccupiedHousings(); }
        }

        /// <summary>
        /// The animal manager for this farmyard
        /// </summary>
        [JsonIgnore]
        public AnimalManager AnimalManager { get; }

        [JsonIgnore]
        public List<Point> EmptyLocations
        {
            get
            {
                return Grid.Where(x => x.GetType() == typeof(Empty) && !((Empty)x).HasStable).Select(room => room.Location).ToList();
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
                return Grid.Where(x => x.GetType() == typeof(Room)).Select(room => room.Location).ToList();
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
                return Grid.OfType<Field>().Select(field => field.Location).ToList();
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
                return Grid.Where(x => x.GetType() == typeof(Pasture) && ((Pasture)x).HasStable).Select(room => room.Location).ToList();
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
                return Grid.Where(x => x.GetType() == typeof(Pasture)).Select(room => room.Location).ToList();
            }
        }

        /// <summary>
        /// List of all pastures
        /// </summary>
        public ImmutableArray<int[]> Pastures { get; }

        /// <summary>
        /// The underlying grid that holds all the farmyard 
        /// layout information
        /// </summary>
        public ImmutableList<FarmyardEntity> Grid { get; }

        /// <summary>
        /// An array of all the fence locations (as indices);
        /// </summary>
        public ImmutableList<int> Fences { get; }

        /// <summary>
        /// The type of house (wood, clay or stone).
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public HouseType HouseType { get; }

        /// <summary>
        /// Renovates the house.
        /// Logic for house renovations need to be moved into the Curator to deal with cards.
        /// </summary>
        /// <returns></returns>
        public Farmyard Renovate()
        {
            HouseType houseType;
            if (this.HouseType == HouseType.Wood)
                houseType = HouseType.Clay;
            else
                houseType = HouseType.Stone;

            return new Farmyard(Grid, Pastures, Fences, AnimalManager, houseType);
        }

        /// <summary>
        /// Adds fences to the farmyard 
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public Farmyard AddFences(int[] indices)
        {
            return new Farmyard(Grid,
                Pastures,
                Fences.AddRange(indices),
                AnimalManager,
                HouseType);            
        }

        /// <summary>
        /// Sets the farmyards pastures
        /// </summary>
        /// <param name="pastures"></param>
        /// <returns></returns>
        public Farmyard SetPastures(ImmutableArray<int[]> pastures)
        {
            if (pastures != null)
            {
                var grid = this.Grid;
                foreach (var pasture in pastures)
                {
                    foreach (var plot in pasture)
                    {
                        var x = plot % WIDTH;
                        var y = (int)(plot / WIDTH);
                        if (!(grid[y * WIDTH + x] is Pasture))
                        {
                            var hasStable = ((Empty)grid[y * WIDTH + x]).HasStable;
                            grid = grid.SetItem(y * WIDTH + x, new Pasture(hasStable, x, y));
                        }
                    }
                }

                return new Farmyard(grid, pastures, Fences, AnimalManager, HouseType);
            }
            return this;
        }

        public Farmyard HarvestFields(out ResourceCache[] harvestedResources)
        {
            var yields = new Dictionary<Resource, ResourceCache>();
            var currentGrid = Grid.ToImmutableArray();
            var newGrid = Grid;
            foreach (var plot in currentGrid)
            {
                if (plot is Field)
                {
                    var field = (Field)plot;
                    ResourceCache yield;
                    newGrid = newGrid.SetItem(field.LocationIndex, field.Harvest(out yield));

                    if (yield != null)
                    {
                        if (!yields.ContainsKey(yield.Type))
                            yields[yield.Type] = new ResourceCache(yield.Type, 0);

                        yields[yield.Type] = yields[yield.Type].updateCount(yield.Count);
                    }
                }
            }

            harvestedResources = yields.Values.ToArray();
            return new Farmyard(newGrid, Pastures, Fences, AnimalManager, HouseType);
        }

        /// <summary>
        /// Updates the animal manager.
        /// </summary>
        public Farmyard UpdateAnimalManager()
        {
            return new Farmyard(Grid, Pastures, Fences, AnimalManager.Update(this.Grid, this.Pastures), HouseType);
        }

        public Farmyard UpdateAnimalManager(AnimalHousingData[] animalAssignments)
        {
            return new Farmyard(Grid, Pastures, Fences, AnimalManager.Update(this.Grid, this.Pastures, animalAssignments), HouseType);
        }

        public Farmyard AssignAnimals(AnimalHousingData[] assignments)
        {
            return new Farmyard(Grid, Pastures, Fences, AnimalManager.AssignAnimals(assignments), HouseType);
        }

        public Farmyard RemoveAnimals(AnimalResource type, int count)
        {
            return new Farmyard(Grid, Pastures, Fences, AnimalManager.RemoveAnimals(type, count), HouseType);
        }

        public Farmyard AddRoom(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return AddRoom(x, y);
        }

        /// <summary>
        /// Attempts to set an empty plot to a room.  
        /// If the plot is already occupied this will not
        /// change it to a room.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if the plot was able to be set to a room</returns>
        public Farmyard AddRoom(int x, int y)
        {
            return new Farmyard(Grid.SetItem(y * WIDTH + x, new Room(x, y)), Pastures, Fences, AnimalManager, HouseType);
        }

        public Farmyard AddStable(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return AddStable(x, y);
        }

        public Farmyard AddStable(int x, int y)
        {
            return new Farmyard(Grid.SetItem(y * WIDTH + x, ((Empty)Grid[y * WIDTH + x]).AddStable()), Pastures, Fences, AnimalManager, HouseType);
        }

        public Farmyard PlowField(int index)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return PlowField(x, y);
        }

        public Farmyard PlowField(int x, int y)
        {
            return new Farmyard(Grid.SetItem(y * WIDTH + x, new Field(x, y)), 
                Pastures, Fences, AnimalManager, HouseType);
        }

        public Farmyard SowField(int index, Resource resource)
        {
            int x, y;
            IndexToCoords(index, out x, out y);
            return SowField(x, y, resource);
        }

        public Farmyard SowField(int x, int y, Resource resource)
        {
            if (!(Grid[y * WIDTH + x] is Field))
                throw new InvalidOperationException("Attempted to sow non field");

            return new Farm.Farmyard(Grid.SetItem(y * WIDTH + x, ((Field)Grid[y * WIDTH + x]).Sow(resource)),
                Pastures, Fences, AnimalManager, HouseType);
            
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


    }

}