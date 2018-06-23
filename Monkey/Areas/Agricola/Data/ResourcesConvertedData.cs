using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class ResourcesConvertedData: ResourceConversion
    {
        public ResourcesConvertedData(int id, Resource inType, int inAmount, int? inLimit, Resource outType, int outAmount, int amountConverted)
            :base(id, inType, inAmount, inLimit, outType, outAmount)
        {
            AmountConverted = amountConverted;
        }

        /// <summary>
        /// Builds a resource data object from a resource conversion and an input amount
        /// </summary>
        /// <param name="resourceConversion">The resource conversion being applied</param>
        /// <param name="amountConverted">The amount of input resource being converted</param>
        /// <returns></returns>
        public static ResourcesConvertedData FromResourceConversion(ResourceConversion resourceConversion, int amountConverted){
            return new ResourcesConvertedData(
                resourceConversion.Id,
                resourceConversion.InType,
                resourceConversion.InAmount,
                resourceConversion.InLimit,
                resourceConversion.OutType,
                resourceConversion.OutAmount,
                amountConverted);
        }

        /// <summary>
        /// The amount of the input resource being converted by the player
        /// </summary>
        public readonly int AmountConverted;

    }
}