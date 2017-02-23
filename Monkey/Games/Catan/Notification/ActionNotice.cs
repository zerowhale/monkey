using Monkey.Game.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Catan.Notification
{
    public class ActionNotice : GameActionNotice
    {
        public ActionNotice(Object subject, NoticeVerb verb)
            : base(subject, verb.ToString(), (INoticePredicate)null)
        {

        }

        public ActionNotice(Object subject, NoticeVerb verb, INoticePredicate predicate)
            : base(subject, verb.ToString(), predicate == null ? null : new List<INoticePredicate>())
        {
            if (predicate != null)
                Predicates.Add(predicate);
        }

        public ActionNotice(Object subject, NoticeVerb verb, INoticePredicate[] predicates)
            : base(subject, verb.ToString(), predicates == null ? null : new List<INoticePredicate>())
        {
            if (predicates != null)
            {
                foreach(var predicate in predicates)
                    Predicates.Add(predicate);
            }
        }

        public ActionNotice(Object subject, NoticeVerb verb, List<INoticePredicate> predicates)
            :base(subject, verb.ToString(), predicates )
        {

        }

    }
}