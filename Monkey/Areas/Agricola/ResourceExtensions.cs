using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola
{
    public static class ResourceExtensions
    {

        /// <summary>
        /// Returns true if the resource is an AnimalResource type
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsAnimal(this Resource @this)
        {
            return ((int)@this & 0x10000000) > 0;
        }
    }
}