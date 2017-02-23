using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game.Notification
{
    /// <summary>
    /// Used for sending information to clients that can be used to formulate 
    /// human language displays for game events.  For example, outputing to an event log.
    /// </summary>
    public class GameActionNotice
    {
        /// <summary>
        /// Constructor for notices without predicates.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        public GameActionNotice(Object subject, string verb)
            :this(subject, verb, (INoticePredicate)null)
        {

        }

        /// <summary>
        /// Constructor for notices with a single predicate.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="predicate"></param>
        public GameActionNotice(Object subject, string verb, INoticePredicate predicate)
            : this(subject, verb, predicate == null ? null : new List<INoticePredicate>())
        {
            if(predicate != null)
                Predicates.Add(predicate);
        }

        /// <summary>
        /// Constructor for notice.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="predicates"></param>
        public GameActionNotice(Object subject, string verb, List<INoticePredicate> predicates)
        {
            Subject = subject;
            Verb = verb;
            Predicates = predicates;
        }

        /// <summary>
        /// What the message is about.
        /// </summary>
        public Object Subject
        {
            get;
            set;
        }

        /// <summary>
        /// What the message is doing.
        /// </summary>
        public string Verb
        {
            get;
            set;
        }

        /// <summary>
        /// Information related to what the verb is acting upon.
        /// </summary>
        public List<INoticePredicate> Predicates
        {
            get;
            set;
        }
    }
}