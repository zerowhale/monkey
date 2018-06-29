using BoardgamePlatform.Game.Utils;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    /// <summary>
    /// The Animal Manager handles assigning animals to animal housings (stables, pastures, player house).
    /// It verifies manual assignments or can automatically place animals.
    /// </summary>
    public class AnimalManager
    {
        public AnimalManager()
        {
            this.housings = ImmutableDictionary<string, AnimalHousing>.Empty.SetItem("house", new AnimalHousing("house", 1));
        }

        public AnimalManager(ImmutableDictionary<string, AnimalHousing> housings)
        {
            this.housings = housings;
        }

        public AnimalManager Update(ImmutableList<FarmyardEntity> grid, ImmutableArray<int[]> pastures)
        {
            return this.Update(grid, pastures, null);
        }

        public AnimalManager Update(ImmutableList<FarmyardEntity> grid, ImmutableArray<int[]> pastures, AnimalHousingData[] animalAssignments){
            var oldHousings = new Dictionary<String, AnimalHousing>();
            if (animalAssignments == null)
            {
                foreach (var key in this.housings.Keys)
                {
                    if(this.housings[key].AnimalCount > 0)
                        oldHousings[key] = this.housings[key];
                }
            }

            var stables = new List<Point>();

            var housings = new Dictionary<string, AnimalHousing>();

            // Default house pet
            var id = "house";
            housings[id] = new AnimalHousing(id, 1);

            var stablesCounted = new List<int>();
            foreach (var pasture in pastures) {
                var compiledId = "";
                var count = 0;
                var stableCount = 0;

                foreach (var pid in pasture) {
                    if (compiledId != "") compiledId += "_";
                    compiledId += pid;

                    var x = pid % Farmyard.WIDTH;
                    var y = (int)(pid / Farmyard.WIDTH);
                    var plot = grid[y * Farmyard.WIDTH + x];

                    count++;
                    if (plot is Empty && ((Empty)plot).HasStable)
                    {
                        stableCount++;
                    }

                    stablesCounted.Add(pid);
                }
        
                var capacity = (count * 2) << stableCount;
                id = "pasture" + compiledId;
                housings[id] = new AnimalHousing(id, capacity);
            }

            for (var x = 0; x < Farmyard.WIDTH; x++)
            {
                for (var y = 0; y < Farmyard.HEIGHT; y++)
                {
                    var plot = grid[y * Farmyard.WIDTH + x];
                    if (plot is Empty && !(plot is Pasture) && ((Empty)plot).HasStable)
                        stables.Add(new Point(x, y));
                }
            }



            foreach (var stable in stables)
            {

                var index = stable.Y * Farmyard.WIDTH + stable.X;
                if (!stablesCounted.Contains(index))
                {
                    id = "stable" + index;
                    housings[id] = new AnimalHousing(id, 1);
                }
            }

            if (animalAssignments == null)
            {
                var eHousings = oldHousings.Values.ToArray();
                var skipAutomated = false;
                foreach (var housing in eHousings)
                {
                    if (housing.AnimalCount > 0 && !this.housings.ContainsKey(housing.Id))
                        skipAutomated = true;

                }
             
                if(!skipAutomated)
                    assignAnimals(housings, eHousings);
            }
            else
                assignAnimals(housings, animalAssignments);

            return new AnimalManager(housings.ToImmutableDictionary());
        }

        public ImmutableArray<AnimalHousing> GetOccupiedHousings()
        {
            var occupied = new List<AnimalHousing>();
            foreach (var housing in housings.Values)
            {
                if (housing.AnimalCount > 0)
                    occupied.Add(housing);
            }
            return occupied.ToImmutableArray();
        }

        public Boolean AreAssignmentsValid(AnimalHousingData[] assignments)
        {

            foreach (var assignment in assignments)
            {
                if (!housings.ContainsKey(assignment.Id))
                    return false;

                if (housings[assignment.Id].Capacity < assignment.Count)
                    return false;
            }
            return true;
        }

        public AnimalManager AssignAnimals(AnimalHousingData[] assignments)
        {
            ImmutableDictionary<string, AnimalHousing> newHousings = housings;
            foreach (var assignment in assignments)
            {
                if (!housings.ContainsKey(assignment.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id " + assignment.Id + " not found.");
            }

            foreach (var kvp in housings) 
            {
                newHousings = newHousings.SetItem(kvp.Key, kvp.Value.Empty());
            }

            foreach (var assignment in assignments)
            {
                newHousings = newHousings.SetItem(assignment.Id, housings[assignment.Id].SetAnimals(assignment.Type, assignment.Count));
            }
            return newHousings == null ? this : new AnimalManager(newHousings);
        }


        public AnimalManager AssignAnimals(AnimalHousing[] housings) {
            ImmutableDictionary<string, AnimalHousing> newHousings = null;
            foreach (var housing in housings)
            {
                if (housing.AnimalCount > 0 && !this.housings.ContainsKey(housing.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id "+ housing.Id + " not found.");

               newHousings = this.housings.SetItem(housing.Id, this.housings[housing.Id].SetAnimals(housing.AnimalType.Value, housing.AnimalCount));
            }
            return newHousings == null ? this : new AnimalManager(newHousings);
        }


        public AnimalManager RemoveAnimals(AnimalResource type, int count)
        {
            ImmutableDictionary<string, AnimalHousing> newHousings = null;
            var matches = housings.Values.Where(x => x.AnimalType == type).OrderBy(x => x.AnimalCount);
            foreach (var m in matches)
            {
                var toRemove = m.AnimalCount < count ? m.AnimalCount : count;
                newHousings = housings.SetItem(m.Id, m.SetAnimals(type, m.AnimalCount - toRemove));
                count -= toRemove;

                if (count == 0)
                    break;
            }
            return newHousings == null ? this : new AnimalManager(newHousings);
        }


        public int GetAnimalCount(AnimalResource type)
        {
            return housings.Values.Where(x => x.AnimalType == type).Sum(y => y.AnimalCount);
        }

        private void assignAnimals(Dictionary<string, AnimalHousing> housings, AnimalHousing[] oldHousings)
        {
            foreach (var oldHousing in oldHousings)
            {
                if (oldHousing.AnimalCount > 0 && !this.housings.ContainsKey(oldHousing.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id " + oldHousing.Id + " not found.");

                housings[oldHousing.Id] = oldHousing.SetAnimals(oldHousing.AnimalType.Value, oldHousing.AnimalCount);
            }
        }

        private void assignAnimals(Dictionary<string, AnimalHousing> housings, AnimalHousingData[] assignments)
        {
            foreach (var assignment in assignments)
            {
                if (!housings.ContainsKey(assignment.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id " + assignment.Id + " not found.");
            }

            var temp = housings.ToImmutableDictionary();
            foreach (var kvp in temp)
            {
                housings[kvp.Key] = kvp.Value.Empty();
            }

            foreach (var assignment in assignments)
            {
                housings[assignment.Id] = housings[assignment.Id].SetAnimals(assignment.Type, assignment.Count);
            }
        }

        private ImmutableDictionary<string, AnimalHousing> housings { get; }    
    }

}