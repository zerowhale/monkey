using Monkey.Games.Agricola.Farm;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Utils
{
    public class FencePlacementValidator
    {
        public FencePlacementValidator(int[] newFences, Farmyard farmyard, out List<int[]> pastures)
        {
            var existingFences = farmyard.Fences;
            var totalFenceCount = existingFences.Length + newFences.Length;

            if (totalFenceCount > Farmyard.MAX_FENCES)
            {
                Valid = false;
                pastures = null;
                return;
            }

            fences = new int[totalFenceCount];

            Array.Copy(existingFences, fences, existingFences.Length);
            Array.Copy(newFences, 0, fences, existingFences.Length, newFences.Length);

            
            Validate();

            TryGetPastures(farmyard, out pastures);
        }


        public bool Valid
        {
            get;
            private set;
        }

        


        private void TryGetPastures(Farmyard farmyard, out List<int[]> pastures){

            var grid = farmyard.Grid;
            pastures = new List<Int32[]>();

            foreach (var index in pastureIndices) {
                
                var group = new List<int>();
                for (var i = 0; i < grid.Length; i++) {
                    if (this.grid[i] == index) {
                        group.Add(i);
                    }
                }

                // Verify nothing is occupying the proposed pasture slots
                var validPasture = true;
                foreach (var i in group) {
                    var x = i % Farmyard.WIDTH;
                    var y = (int)(i / Farmyard.WIDTH);
                

                    if (!(grid[x,y] is Empty)) {
                        Valid = false;
                        return;
                    }
                    
                }

                if(validPasture)
                    pastures.Add(group.ToArray());
            }
        }



        private void Validate(){

            this.Valid = true;

            foreach (var i in this.fences) {
                if (i < 0 || i >= Farmyard.FENCES_WIDTH * Farmyard.FENCES_HEIGHT)
                {
                    this.Valid = false;
                    return;
                }
                    

                // If the first fence index found is vertical the yard can not be valid.
                var y = (int)(i / Farmyard.FENCES_WIDTH);
                if (y % 2 != 0) {
                    this.Valid = false;
                    continue;
                }

                this.runs.Add(new Run(new RunSegment(i, Direction.RIGHT)));
                break;

            }



            for (var r = 0; r < this.runs.Count; r++) {
                var run = this.runs[r];
                if (run != null) {
                    var head = run.Head;
                    if (this.SegmentClaimed(head.Index, run)) {
                        this.runs.Remove(run);
                        r--;
                        continue;
                    }
                    if (!this.ExecuteRun(r)) {
                        this.Valid = false;
                    }
                
                }
            }


            foreach (var fence in fences) {

                if (!this.SegmentClaimed(fence, null)) {
                    this.Valid = false;
                }
            }

            var pastures = new List<int>();
            var highestIndex = 0;
            for (var i = 0; i < grid.Length; i++) {
                if (grid[i] == 0) {
                    FloodFill(i, ++highestIndex);
                }
            }

            for (var i = 0; i < grid.Length; i++) {
                if(!pastures.Contains(grid[i]) && !outside[grid[i]])
                    pastures.Add(this.grid[i]);
            }

            this.pastureIndices = pastures;        
        }


        private bool ExecuteRun(int runIndex) {
            var run = this.runs[runIndex];
            var segment = run.Head;
            var newRuns = new List<Run>();
            while (true) {
                var neighbors = segment.Neighbors;
                for (var n = neighbors.Count - 1; n >= 0; n--) {
                    if (!this.fences.Contains(neighbors[n].Index))
                        neighbors.RemoveAt(n);
                }

                // if no neighbors the fences can't be valid
                if (neighbors.Count == 0) {
                    return false;
                }

                RunSegment matchingSegment;
                if ((matchingSegment = run.MatchingSegment(neighbors[0].Index)) != null) {
                    if (matchingSegment.Direction != neighbors[0].Direction)
                        return false;

                    var runs = new List<Run>();
                    for (var i = 0; i <= runIndex; i++)
                        runs.Add(this.runs[i]);

                    foreach (var i in newRuns)
                        runs.Add(i);

                    for (var i = runIndex + 1; i < this.runs.Count; i++) {
                        runs.Add(this.runs[i]);
                    }
                    this.runs = runs;

                    return true;
                }

                segment = neighbors[0];
                run.AddSegment(neighbors[0]);

                if (neighbors.Count > 1 && !SegmentClaimed(neighbors[1].Index, null))
                    newRuns.Add(new Run(neighbors[1]));

                if (neighbors.Count > 2 && !SegmentClaimed(neighbors[2].Index, null))
                    newRuns.Add(new Run(neighbors[2]));
            }
        }

        private bool SegmentClaimed(int index, Run owningRun){
            foreach( var run in runs){
                if(run != owningRun && run.ContainsSegmentIndex(index)){
                    return true;
                }
            }
            return false;
        }





        private void FloodFill(int index, int value) {
            grid[index] = value;

            var neighbors = GetUnfencedNeighbors(index, value);

            foreach (var n in neighbors) {
                if (grid[n] == 0)
                    FloodFill(n, value);
            }
        }

        private List<int> GetUnfencedNeighbors(int index, int value) {
            var neighbors = new List<int>();
            var x = index % Farmyard.WIDTH;
            var y = (int)(index / Farmyard.WIDTH);

            if (!IsFenced(index, Direction.LEFT)) {
                if (x != 0)
                    neighbors.Add(index - 1);
                else
                    this.outside[value] = true;
            }

            if (!IsFenced(index, Direction.RIGHT)) {
                if (x != Farmyard.WIDTH - 1)
                    neighbors.Add(index + 1);
                else
                    this.outside[value] = true;
            }

            if (!IsFenced(index, Direction.UP)) {
                if (y != 0)
                    neighbors.Add(index - Farmyard.WIDTH);
                else
                    this.outside[value] = true;
            }

            if (!IsFenced(index, Direction.DOWN)) {
                if (y != Farmyard.HEIGHT - 1)
                    neighbors.Add(index + Farmyard.WIDTH);
                else
                    this.outside[value] = true;
            }

            return neighbors;
        }

        private bool IsFenced(int plot, Direction direction) {
            var fIndex = GetFenceIndex(plot, direction);
            return fences.Contains(fIndex);
        }

        private int GetFenceIndex(int plot, Direction direction) {
            var x = plot % Farmyard.WIDTH;
            var y = (int)(plot / Farmyard.WIDTH);


            switch (direction) {
                case Direction.UP:
                    return plot + (y * (Farmyard.FENCES_WIDTH + 1));
                case Direction.DOWN:
                    return ((y * 2 + 2) * Farmyard.FENCES_WIDTH) + x;
                case Direction.LEFT:
                    return (y * 2 + 1) * Farmyard.FENCES_WIDTH + x;
                case Direction.RIGHT:
                    return (y * 2 + 1) * Farmyard.FENCES_WIDTH + x + 1;
            }
            return -1;
        }



        private List<int> pastureIndices = new List<int>();
        private List<Run> runs = new List<Run>();
        private int[] fences;
        private int[] grid = new int[Farmyard.WIDTH * Farmyard.HEIGHT];
        private bool[] outside = new bool[Farmyard.WIDTH * Farmyard.HEIGHT];

        private class Run
        {

            public Run(RunSegment head)
            {
                this.Head = head;
            }

            public void AddSegment(RunSegment segment){
                segments.Add(segment);
            }

            /// <summary>
            /// Checks if a segment with the specified index
            /// exists in this run
            /// </summary>
            /// <param name="segment"></param>
            /// <returns></returns>
            public bool ContainsSegmentIndex(int index){
                for(var i=0;i<segments.Count;i++){
                    if(segments[i].Index == index)
                        return true;
                }
                return false;
            }

            public RunSegment MatchingSegment(int index){
                for(var i=0;i<segments.Count;i++){
                    if(segments[i].Index == index)
                        return segments[i];
                }
                return null;
            }

            public RunSegment[] Segments
            {
                get { return segments.ToArray(); }
            }

            public RunSegment Head
            {
                get;
                private set;
            }
            private List<RunSegment> segments = new List<RunSegment>();
        }

        private class RunSegment
        {
            public RunSegment(int index, Direction direction)
            {
                Direction = direction;
                Index = index;
                X = index % Farmyard.FENCES_WIDTH;
                Y = (int)(index / Farmyard.FENCES_WIDTH);
            }

            public List<RunSegment> Neighbors{
                get{
                    var neighbors = new List<RunSegment>();

                    if (Y % 2 == 0) {
                        if (Direction == Direction.RIGHT) {
                            if (Y < Farmyard.FENCES_HEIGHT - 1) neighbors.Add(new RunSegment(Index + Farmyard.FENCES_WIDTH + 1, Direction.DOWN));
                            if (X < Farmyard.FENCES_WIDTH - 2) neighbors.Add(new RunSegment(Index + 1, Direction.RIGHT));
                            if (Y > 0) neighbors.Add(new RunSegment(Index - Farmyard.FENCES_WIDTH + 1, Direction.UP));

                        }
                        else {
                            if (Y > 0) neighbors.Add(new RunSegment(Index - Farmyard.FENCES_WIDTH, Direction.UP));
                            if (X > 0) neighbors.Add(new RunSegment(Index - 1, Direction.LEFT));
                            if (Y < Farmyard.FENCES_HEIGHT - 1) neighbors.Add(new RunSegment(Index + Farmyard.FENCES_WIDTH, Direction.DOWN));
                        }
                    }
                    else {
                        if (Direction == Direction.UP) {
                            if (X < Farmyard.FENCES_WIDTH - 1) neighbors.Add(new RunSegment(Index - Farmyard.FENCES_WIDTH, Direction.RIGHT));
                            if (Y > 1) neighbors.Add(new RunSegment(Index - Farmyard.FENCES_WIDTH * 2, Direction.UP));
                            if (X > 0) neighbors.Add(new RunSegment(Index - Farmyard.FENCES_WIDTH - 1, Direction.LEFT));
                        }
                        else {
                            if (X > 0) neighbors.Add(new RunSegment(Index + Farmyard.FENCES_WIDTH - 1, Direction.LEFT));
                            if (Y < Farmyard.FENCES_HEIGHT - 2) neighbors.Add(new RunSegment(Index + Farmyard.FENCES_WIDTH * 2, Direction.DOWN));
                            if (X < Farmyard.FENCES_WIDTH - 1) neighbors.Add(new RunSegment(Index + Farmyard.FENCES_WIDTH, Direction.RIGHT));
                        }
                    }

                    return neighbors;
                }
            }

            public Direction Direction
            {
                get;
                private set;
            }

            public int Index
            {
                get;
                private set;
            }

            public int X
            {
                get;
                private set;
            }

            public int Y
            {
                get;
                private set;
            }
        }
        private enum Direction
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

    
    
    }
}