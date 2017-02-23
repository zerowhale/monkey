using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    public class InterruptState
    {
        public string Type
        {
            get 
            { 
                return this.GetType().Name.ToString(); 
            }
        }
    }
}