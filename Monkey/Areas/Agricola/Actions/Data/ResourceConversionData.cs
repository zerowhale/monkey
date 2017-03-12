using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class ResourceConversionData: GameActionData
    {
        /// <summary>
        /// The conversion id
        /// </summary>
        public int Id;

        /// <summary>
        /// The number of conversions being done
        /// </summary>
        public int Count;

        /// <summary>
        /// The type of resource being converted
        /// </summary>
        public Resource InType;

        public int InAmount;

        public Resource OutType;


    }
}