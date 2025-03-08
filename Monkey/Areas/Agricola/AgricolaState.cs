using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Areas.Agricola
{
    public class AgricolaState //: System.Collections.Immutable.ImmutableDictionary<string, object>
    {
        private const string StateKeyInterrupts = "StateKeyInterrupts";
        private const string StateKeyMajorImprovementOwners = "StateKeyMajorImprovementOwners";
        private const string StateKeyRoundActions = "StateKeyRoundActions";
        private const string StateKeyPlayerStates = "StateKeyPlayerStates";
        private const string StateKeyFamilyOnRounds = "StateKeyFamilyOnRounds";
        private const string StateKeyRoundActionStates = "stateKeyRoundActionStates";


        private const string StateKeyCacheResources = "CacheResources";
        private const string StateKeyPlayers = "Players";
        private const string StateKeyDelayedResources = "DelayedResources";
        private const string StateKeyResourcesPerRound = "ResourcesPerRound";
    }
}