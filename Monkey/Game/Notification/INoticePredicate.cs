using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game.Notification
{
    /// <summary>
    /// Interface to derive notification predicates from
    /// so different types of predicates can be grouped
    /// and sent with a single notification, allowing for
    /// compound results such as "PlayerX builds 4 fences
    /// surrounding 1 pasture".
    /// </summary>
    public interface INoticePredicate
    {

        string PredicateType
        {
            get;
        }
    }
}