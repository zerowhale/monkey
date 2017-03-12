using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game
{
    public class GameCreationProperty
    {
        public GameCreationProperty(string id, string name, string type, dynamic data)
        {
            Id = id;
            Name = name;
            Type = type;
            Data = data;
        }

        public string Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Type
        {
            get;
            private set;
        }

        public dynamic Data
        {
            get;
            private set;
        }
    }
}