using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TakeAnimalActionTrigger: GameEventTrigger
    {

        public TakeAnimalActionTrigger(AnimalResource animalType = AnimalResource.Sheep)
            :base()
        {
            this.AnimalType = animalType;
        }

        public TakeAnimalActionTrigger(XElement definition)
            : base(definition)
        {

        }

        public override bool Triggered(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer, GameEventTrigger trigger)
        {
            return base.Triggered(resolvingPlayer, triggeringPlayer, trigger)
                && this.AnimalType == ((TakeAnimalActionTrigger)trigger).AnimalType;
        }


        public AnimalResource AnimalType
        {
            get;
            set;
        }
    
    }
}