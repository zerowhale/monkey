using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game
{
    public class RegisteredGame
    {
        public RegisteredGame(string name, string viewPath, Type type, int maxPlayers, bool colorSelectionEnabled, List<GameCreationProperty> creationProperties)
        {
            Name = name;
            ViewPath = viewPath;
            Type = type;
            MaxPlayers = maxPlayers;
            ColorSelectionEnabled = colorSelectionEnabled;
            CreationProperties = creationProperties;
        }

        public int MaxPlayers
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string ViewPath
        {
            get;
            private set;
        }

        public bool ColorSelectionEnabled
        {
            get;
            private set;
        }

        [JsonIgnore]
        public Type Type
        {
            get;
            private set;
        }

        public List<GameCreationProperty> CreationProperties
        {
            get;
            private set;
        }
    }
    
}