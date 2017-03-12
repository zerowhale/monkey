using BoardgamePlatform.Game.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace BoardgamePlatform.Game
{
    public static class GameRegistry
    {
        static GameRegistry()
        {
            var gamesFolder = System.Web.Hosting.HostingEnvironment.MapPath("/Areas");
            var folders = Directory.GetDirectories(gamesFolder);
            foreach (var folder in folders)
            {
                var file = folder + "\\Game.manifest";
                if (File.Exists(file))
                {
                    var xml = XDocument.Load(file);
                    Register(xml);
                }
            }
        }

        public static Type GetGameType(string gameName)
        {
            return games[gameName].Type;
        }

        public static RegisteredGame[] GetRegisteredGames()
        {
            return games.Values.ToArray();
        }

        public static RegisteredGame GetRegisteredGame(string gameTitle)
        {
            return games[gameTitle];
        }

        private static void Register(XDocument xml){
            var manifest = xml.Descendants("Manifest").First();
            var cls = (string)manifest.Attribute("Type");
            var type = Type.GetType(cls);
            if (type == null)
                throw new InvalidDataException("Failed to load game type:" + cls);

            var name = (string)manifest.Attribute("Name");
            var viewPath = (string)manifest.Attribute("ViewPath");
            var colorSelectionEnabledAttribute = (string)manifest.Attribute("ColorSelectionEnabled");
            var colorSelectionEnabled = (colorSelectionEnabledAttribute != null && colorSelectionEnabledAttribute.ToLower() == "false") ? false : true;
            var maxPlayers = (int)manifest.Attribute("MaxPlayers");
            var props = new List<GameCreationProperty>();
            var xmlProps = manifest.Element("CreationOptions").Elements("Option");


            foreach (var item in xmlProps)
            {
                dynamic data = new ExpandoObject();
                var dataElement = item.Element("Data");
                if(dataElement != null){
                    var elements = dataElement.Elements();
                    ExpandoObjectHelper.Parse(data, dataElement, (new string[] { "Value" }).ToList());
                    data = data.Data;
                }

                var option = new GameCreationProperty(
                   (string)item.Attribute("Id"),
                   (string)item.Attribute("Name"),
                   (string)item.Attribute("Type"),
                   data
                );
                props.Add(option);
            }

            Register(name, viewPath, type, maxPlayers, colorSelectionEnabled, props);
        }

        private static void Register(string name, string viewPath, Type type, int maxPlayers, bool colorSelectionEnabled, List<GameCreationProperty> creationProperties)
        {
            games[viewPath] = new RegisteredGame(name, viewPath, type, maxPlayers, colorSelectionEnabled, creationProperties);
        }

        private static Dictionary<string, RegisteredGame> games = new Dictionary<string, RegisteredGame>();



    }

}