using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public enum NoticeVerb
    {
        Move,
        DirectFlight,
        ShuttleFlight,
        CharterFlight,
        Treat,
        MedicTreat,
        Build,
        Cure,
        Eradicate,
        ForcedDiscard,
        PassTurn,
        DrawInfectionCard,
        Outbreak,
        InfectionIncreased
    }
}