﻿using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class AnimalChoiceAction: RoundAction
    {
        public AnimalChoiceAction(AgricolaGame game, int id)
            :base(game, id)
        { 

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var choiceData = (AnimalChoiceActionData)data;

            if (choiceData.Option == AnimalResource.Cattle && player.PersonalSupply.Food < 1)
                return false;

            var animalData = new Dictionary<AnimalResource, int>();
            animalData[choiceData.Option] = 1;

            return ActionService.CanAssignAnimals(player, choiceData.AnimalData, player.Farmyard.AnimalManager, animalData);
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var choiceData = (AnimalChoiceActionData)data;
            if (choiceData.Option == AnimalResource.Sheep)
                player.PersonalSupply.Food++;
            else if (choiceData.Option == AnimalResource.Cattle)
                player.PersonalSupply.Food--;

            ActionService.AssignAnimals(player, choiceData.AnimalData, ResultingNotices);
        }
    }
}