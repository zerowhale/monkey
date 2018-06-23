using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Data
{
    /// <summary>
    /// A description of conversion rate between an input resource and an output resource
    /// </summary>
    public class ResourceConversion
    {
        public ResourceConversion(XElement definition)
        {
            InAmount = (int)definition.Attribute("InAmount");
            InType = (Resource)Enum.Parse(typeof(Resource), (string)definition.Attribute("InType"));
            InLimit = definition.Attribute("InLimit") == null ? null : (int?)definition.Attribute("InLimit");
            OutAmount = (int)definition.Attribute("OutAmount");
            OutType = (Resource)Enum.Parse(typeof(Resource), (string)definition.Attribute("OutType"));
        }

        public ResourceConversion(int id, Resource inType, int inAmount, int? inLimit, Resource outType, int outAmount)
        {
            Id = id;
            InType = inType;
            InAmount = inAmount;
            InLimit = inLimit;
            OutType = outType;
            OutAmount = outAmount;
        }

        /// <summary>
        /// Create a ResourceConversion from an xml definition
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static ResourceConversion Create(XElement definition)
        {
            return new ResourceConversion(definition);
        }
        
        /// <summary>
        ///  An int that identifies this resource conversion in some way.  Often it will be the
        ///  card id that the resource conversion came from.
        /// </summary>
        public int Id;

        /// <summary>
        /// The type of input resource
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))] 
        public readonly Resource InType;

        /// <summary>
        /// The amount of input resource
        /// </summary>
        public readonly int InAmount;

        /// <summary>
        /// A limit to how many resources can be input in a single transaction.
        /// Null for no limit.
        /// </summary>
        public readonly int? InLimit;

        /// <summary>
        /// The type of output resource
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))] 
        public readonly Resource OutType;

        /// <summary>
        /// The amount of output resource per input resource supplied
        /// </summary>
        public readonly int OutAmount;
    }
}