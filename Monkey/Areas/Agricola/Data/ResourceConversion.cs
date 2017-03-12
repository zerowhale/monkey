using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Data
{
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



        public static ResourceConversion Create(XElement definition)
        {
            return new ResourceConversion(definition);
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

        public int Id;
        [JsonConverter(typeof(StringEnumConverter))] 
        public Resource InType;
        public int InAmount;
        public int? InLimit;
        [JsonConverter(typeof(StringEnumConverter))] 
        public Resource OutType;
        public int OutAmount;
    }
}