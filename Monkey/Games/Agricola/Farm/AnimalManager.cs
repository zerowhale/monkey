using Monkey.Game.Utils;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class AnimalManager
    {

        public AnimalManager()
        {
            this.housings["house"] = new AnimalHousing("house", 1);
            ResetAnimalCounts();
        }

        public void Update(FarmyardEntity[,] grid, List<int[]> pastures)
        {
            this.Update(grid, pastures, null);
        }

        public void Update(FarmyardEntity[,] grid, List<int[]> pastures, AnimalHousingData[] animalAssignments){
            var oldHousings = new Dictionary<String, AnimalHousing>();
            if (animalAssignments == null)
            {
                foreach (var key in housings.Keys)
                {
                    if(housings[key].AnimalCount > 0)
                        oldHousings[key] = housings[key];
                }
            }

            var stables = new List<Point>();
            housings.Clear();

            // Default house pet
            var id = "house";
            this.housings[id] = new AnimalHousing(id , 1);

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
                    var plot = grid[x,y];

                    count++;
                    if (plot is Empty && ((Empty)plot).HasStable)
                    {
                        stableCount++;
                    }

                    stablesCounted.Add(pid);
                }
        
                var capacity = (count * 2) << stableCount;
                id = "pasture" + compiledId;
                this.housings[id] = new AnimalHousing(id, capacity);
            }

            for (var x = 0; x < Farmyard.WIDTH; x++)
            {
                for (var y = 0; y < Farmyard.HEIGHT; y++)
                {
                    var plot = grid[x, y];
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
                    this.housings[id] = new AnimalHousing(id, 1);
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
                    AssignAnimals(eHousings);
            }
            else
                AssignAnimals(animalAssignments);
        }

        public AnimalHousing[] GetOccupiedHousings()
        {
            var occupied = new List<AnimalHousing>();
            foreach (var housing in housings.Values)
            {
                if (housing.AnimalCount > 0)
                    occupied.Add(housing);
            }
            return occupied.ToArray();
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

        public void AssignAnimals(AnimalHousingData[] assignments)
        {
            foreach (var assignment in assignments)
            {
                if (!housings.ContainsKey(assignment.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id " + assignment.Id + " not found.");
            }

            foreach (var housing in housings.Values)
            {
                housing.Empty();
            }

            ResetAnimalCounts();
            foreach (var assignment in assignments)
            {
                housings[assignment.Id].SetAnimals(assignment.Type, assignment.Count);
                CountAnimal(assignment.Type, assignment.Count);
            }
        }

        public void AssignAnimals(AnimalHousing[] housings) {
            ResetAnimalCounts();
            foreach (var housing in housings)
            {
                if (housing.AnimalCount > 0 && !this.housings.ContainsKey(housing.Id))
                    throw new ArgumentException("Invalid animal assignments, housing id "+ housing.Id + " not found.");

                this.housings[housing.Id].SetAnimals(housing.AnimalType, housing.AnimalCount);
                CountAnimal(housing.AnimalType, housing.AnimalCount);
            }
        }


        public void RemoveAnimals(AnimalResource type, int count)
        {
            var matches = housings.Values.Where(x => x.AnimalType == type).OrderBy(x => x.AnimalCount);
            foreach (var m in matches)
            {
                var toRemove = m.AnimalCount < count ? m.AnimalCount : count;
                m.SetAnimals(type, m.AnimalCount - toRemove);
                count -= toRemove;
                CountAnimal(type, -toRemove);

                if (count == 0)
                    break;
            }

            
        }


        public int GetAnimalCount(AnimalResource type)
        {
            return animalCounts[type];
        }

        private void CountAnimal(AnimalResource type, int count)
        {
            animalCounts[type] += count;
        }

        private void ResetAnimalCounts()
        {
            animalCounts[AnimalResource.Sheep] = 0;
            animalCounts[AnimalResource.Boar] = 0;
            animalCounts[AnimalResource.Cattle] = 0;
        }

        private Dictionary<string, AnimalHousing> housings = new Dictionary<string, AnimalHousing>();
        private Dictionary<AnimalResource, int> animalCounts = new Dictionary<AnimalResource, int>();
    
    }

}