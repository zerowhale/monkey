using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class AssignAnimalsAction: InterruptAction
    {
        public AssignAnimalsAction(AgricolaPlayer player, AnimalResource animalType, int animalCount, List<GameActionNotice> resultingNotices)
            : this(player, new ResourceCache[] { new ResourceCache((Resource)animalType, animalCount) }, resultingNotices)
        {
        }
        public AssignAnimalsAction(AgricolaPlayer player, ResourceCache animalCache, List<GameActionNotice> resultingNotices)
            : this(player, new ResourceCache[] { animalCache }, resultingNotices)
        {
        }

        public AssignAnimalsAction(AgricolaPlayer player, ResourceCache[] animals, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.AssignAnimals, resultingNotices)
        {
            this.Animals = animals;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var animalData = new Dictionary<AnimalResource, int>();
            foreach (var animal in Animals)
            {
                animalData[(AnimalResource)animal.Type] = animal.Count;
            }

            if (!ActionService.CanAssignAnimals(player, (AnimalCacheActionData)data, player.Farmyard.AnimalManager, animalData))
                return false;

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ActionService.AssignAnimals(player, (AnimalCacheActionData)data, ResultingNotices);
        }

        public ResourceCache[] Animals
        {
            get;
            private set;
        }

    
    }
}