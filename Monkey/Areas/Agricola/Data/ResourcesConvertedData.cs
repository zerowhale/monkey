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

        public int AmountConverted
        {
            get;
            private set;
        }

    }
}