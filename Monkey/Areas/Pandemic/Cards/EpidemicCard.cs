using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Cards
{
    public class EpidemicCard: ICard
    {
        public EpidemicCard()
        {
            
        }

        public int Id
        {
            get { return 100; }
        }

        public string Name
        {
            get
            {
                return "Epidemic";
            }
        }
    }
}