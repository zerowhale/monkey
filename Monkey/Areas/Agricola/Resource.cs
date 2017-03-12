using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    /// <summary>
    /// All the resources available in Agricola
    /// </summary>
    public enum Resource
    {
        Food = 1,
        Wood = 2,
        Clay = 3,
        Reed = 4,
        Stone = 5,
        Grain = 6,
        Vegetables = 7,
        Sheep = 8 | 0x10000000,
        Boar = 9 | 0x10000000,
        Cattle = 10 | 0x10000000
    }

    /// <summary>
    /// The animal resource subset of Resource
    /// </summary>
    public enum AnimalResource
    {
        Sheep = Resource.Sheep,
        Boar = Resource.Boar,
        Cattle = Resource.Cattle
    }
}