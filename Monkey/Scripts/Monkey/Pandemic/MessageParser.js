var MessageManager = new (function () {
    this.tickerDisplay = $("#game .ticker");
    
    var tickerQueue = [],
        tickerInterval, delayTimeout,
        tickerLaunchDelay = 5000,
        logWindow;

    this.showLog = function () {
        logWindow = ExternalLog.show();
    }

    this.addNotice = function (notice) {
        var obj = this;
        processNewNotice(notice);
        
        if (!tickerInterval) {
            clearTimeout(delayTimeout);
            delayTimeout = setTimeout(function(){
                obj.fireNextTickerNotice();
            }, 1500);
        }
    }
    
    this.startTickerMessage = function (message) {
        var msg = $("<div>" + message + "</div>");
        this.tickerDisplay.append(msg);
        setTimeout(function () {
            msg.addClass("travel");
        }, 100);
        return msg;
    }

    this.fireNextTickerNotice = function() {
        var sending = tickerQueue.shift(),
            obj = this;
        if (sending === undefined) {
            tickerInterval = null;
            return;
        }

        var msg = getTickerText(sending);
        if (msg == null) {
            this.fireNextTickerNotice();
            return;
        }

        msg = this.startTickerMessage(msg);
        tickerInterval = setInterval(function(){
            if(msg.position().left + msg.width() < (obj.tickerDisplay.width() * .9)){
                clearInterval(tickerInterval);
                obj.fireNextTickerNotice();
            }
        }, 1000);
    }
        
    function log (message) {
        ExternalLog.log(message);
    }





    function getTickerText(notice) {
        switch (notice.Verb) {
            case NoticeVerb.Move:
            case NoticeVerb.DirectFlight:
            case NoticeVerb.CharterFlight:
            case NoticeVerb.ShuttleFlight:
                return getMoveTickerText(notice);

            case NoticeVerb.Treat:
            case NoticeVerb.MedicTreat:
                return getTreatTickerText(notice);

            case NoticeVerb.Build:
                return getBuildTickerText(notice);
               
            case NoticeVerb.Eradicate:
                return getEradicateTickerText(notice);

            case NoticeVerb.Outbreak:
                return getOutbreakTickerText(notice);

            case NoticeVerb.InfectionIncreased:
                return getInfectionIncreasedTickerText(notice);

            case NoticeVerb.Cure:
                return getCureTickerText(notice);
        }
        return null;
    }

    function processNewNotice(notice) {
        logNotice(notice);

        switch (notice.Verb) {
            case NoticeVerb.Move:
            case NoticeVerb.DirectFlight:
            case NoticeVerb.CharterFlight:
            case NoticeVerb.ShuttleFlight:
                return processArrivalNotice(notice);

            case NoticeVerb.Treat:
            case NoticeVerb.MedicTreat:
                return processTreatNotice(notice);

            case NoticeVerb.Build:
                return processBuildNotice(notice);

            default:
                pushNotice(notice);
        }
    }

    function logNotice(notice) {
        switch (notice.Verb) {
            case NoticeVerb.Move:
                return logMove(notice);
            case NoticeVerb.DirectFlight:
                return logDirectFlight(notice);
            case NoticeVerb.CharterFlight:
                return logCharterFlight(notice);
            case NoticeVerb.ShuttleFlight:
                return logShuttleFlight(notice);
            case NoticeVerb.Build:
                return logBuild(notice);
            case NoticeVerb.Treat:
                return logTreat(notice);
            case NoticeVerb.MedicTreat:
                return logMedicTreat(notice);
            case NoticeVerb.PassTurn:
                return logPassTurn(notice);
            case NoticeVerb.Eradicate:
                return logEradicate(notice);
            case NoticeVerb.ForcedDiscard:
                return logForcedDiscard(notice);
            case NoticeVerb.DrawInfectionCard:
                return logDrawInfectionCard(notice);
            case NoticeVerb.InfectionIncreased:
                return logInfectionIncreased(notice);
            case NoticeVerb.Outbreak:
                return logOutbreak(notice);
            case NoticeVerb.Cure:
                return logCure(notice);

            default:
                log("<span style='color:red;'>Missing log message for notice of type " + notice.Verb + "</span>");
                console.error("Notice missing log message: ", notice);
        }   
    }



    function processArrivalNotice(notice) {
        var last = getLastNotice();
        if (subjectsVerbsMatch(notice, last))
            setLastNotice(notice);
        else
            pushNotice(notice);
    }

    function processTreatNotice(notice){
        var last = getLastNotice();
        if (subjectsVerbsMatch(notice, last)
            || (subjectsMatch(notice, last) && hasVerb(last, NoticeVerb.Move) && hasPredicateValue(last, "IdPredicate", "Id", getPredicateValue(notice, "IdPredicate", "Id")))) {
            setLastNotice(notice);
        }
        else
            pushNotice(notice);
    }

    function processBuildNotice(notice) {
        var last = getLastNotice();
        if (subjectsMatch(notice, last) && hasVerb(last, NoticeVerb.Move) && hasPredicateValue(last, "IdPredicate", "Id", getPredicateValue(notice, "BuildResearchStationPredicate", "City")))
            setLastNotice(notice);
        else
            pushNotice(notice);
    }








    function logMove(notice) {
        log(notice.Subject + " moved to " + getCityFromPredicate(notice).Name + ".");
    }

    function logDirectFlight(notice) {
        var city = getCityFromPredicate(notice).Name;
        log(notice.Subject + " flew to " + city + " by discarding " + city + ".");
    }

    function logCharterFlight(notice) {
        var city = getCityFromPredicate(notice).Name;
        var discard = getCityFromPredicate(notice, "CardPredicate").Name;
        log(notice.Subject + " flew to " + city + " by discarding " + discard + ".");
    }

    function logShuttleFlight(notice) {
        var city = getCityFromPredicate(notice).Name;
        log(notice.Subject + " flew to " + city + " using shuttle service.");
    }
    
    function logBuild(notice) {
        var pred = getPredicateByType(notice, "BuildResearchStationPredicate"),
            city = Game.getCity(pred.City).Name,
            discarded = !pred.OperationsExpert,
            how = discarded ? " by discarding " + city : " using Operations Expert role";
        log(notice.Subject + " built a research center in " + city + how + ".");
    }

    function logTreat(notice) {
        var actionsUsed = getPredicateValue(notice, "UsedActionsPredicate", "Used"),
            city = getCityFromPredicate(notice).Name,
            dpreds = getPredicatesByType(notice, "DiseasePredicate"),
            counts = "", total = 0;

        for (var dp in dpreds) {
            var pred = dpreds[dp];
            if (counts != "")
                counts += " and ";
            counts += pred.Count + " " + pred.Disease;
            total += pred.Count;
        }

        log(notice.Subject + " treated " + counts + " disease" + (total > 1 ? "s" : "") + " in " + city + ".");
    }

    function logMedicTreat(notice) {
        var city = getCityFromPredicate(notice).Name,
            pred = getPredicateByType(notice, "DiseasePredicate"),
            type = pred.Disease,
            count = pred.Count;
        log(notice.Subject + " automatically treated " + count + " " + type + " disease" + (count > 1 ? "s" : "") + " in " + city + ".");
    }

    function logPassTurn(notice) {
        var passed = getPredicateValue(notice, "UsedActionsPredicate", "Used");
        log(notice.Subject + " passes remaining " + passed + " actions.");
    }

    function logEradicate(notice) {
        var disease = getPredicateValue(notice, "DiseasePredicate", "Disease");
        log(notice.Subject + " has eradicated the " + disease + " disease!");
    }

    function logForcedDiscard(notice) {
        var ids = getPredicateValue(notice, "IdListPredicate", "Ids"),
            cards = "";

        for (var id in ids) {
            if(cards.length > 0)
                cards = cards + " and ";

            cards += Game.getCardName(ids[id]);
        }
        log(notice.Subject + " discarded " + cards + ".");    
    }

    function logDrawInfectionCard(notice) {
        var city = getCityFromPredicate(notice).Name;
        log(notice.Subject + " drew " + city + " infection card.");
    }

    function logInfectionIncreased(notice) {
        var city = Game.getCity(notice.Subject).Name,
            count = getPredicateValue(notice, "IntPredicate", "Value");
        log("Infection increased by " + count + " in " + city + ".");
    }

    function logOutbreak(notice) {
        var city = Game.getCity(notice.Subject).Name;
        log("Outbreak in " + city + "!");
    }

    function logCure(notice) {
        var cards = "",
            ids = getPredicateValue(notice, "IdListPredicate", "Ids"),
            color = getPredicateValue(notice, "DiseasePredicate", "Disease");

        for (var i in ids) {
            if (cards != "")
                cards += ", ";
            cards += Game.getCity(ids[i]).Name;
        }
            
        log(notice.Subject + " has cured the " + color + " disease by discarding " + cards + ".");
    }






    function getMoveTickerText(notice) {
        var player = Game.getPlayer(notice.Subject),
            city = getCityFromPredicate(notice).Name;
        return player.Role.Name + "s arrive in " + city;
    }

    function getTreatTickerText(notice) {
        var city = getCityFromPredicate(notice).Name;
        return "Diseases treated in " + city;
    }

    function getBuildTickerText(notice) {
        var pred = getPredicateByType(notice, "BuildResearchStationPredicate"),
            city = Game.getCity(pred.City).Name;
        return "Research Center constructed in " + city;
    }

    function getEradicateTickerText(notice) {
        var disease = getPredicateValue(notice, "DiseasePredicate", "Disease");
        return disease + " disease has been eradicated!";
    }

    function getOutbreakTickerText(notice) {
        var city = Game.getCity(notice.Subject).Name;
        return "Disease outbreak in " + city + "!";
    }

    function getInfectionIncreasedTickerText(notice) {
        var city = Game.getCity(notice.Subject).Name;
        return "Disease intensifying in " + city + "!";
    }

    function getCureTickerText(notice) {
        var color = getPredicateValue(notice, "DiseasePredicate", "Disease");
        return "Cure discovered for the " + color + " disease!";
    }




    function subjectsMatch(n1, n2) {
        return n1 != null && n2 != null && n1.Subject == n2.Subject;
    }

    function verbsMatch(n1, n2) {
        return n1 != null && n2 != null && n1.Verb == n2.Verb;
    }

    function subjectsVerbsMatch(n1, n2) {
        return verbsMatch(n1, n2) && subjectsMatch(n1, n2);
    }

    function hasSubject(n, s) {
        return n != null && n.Subject == s;
    }

    function hasVerb(n, v) {
        return n != null && n.Verb == v;
    }

    function hasPredicateValue(n, t, p, val) {
        var vals = getPredicateValues(n, t, p)
        for (var v in vals) {
            if (vals[v] == val)
                return true;

        }
        return false;
    }

    function getPredicateValues(notice, type, property) {
        var preds = getPredicatesByType(notice, type),
            vals = [];
        for (var p in preds) {
            pred = preds[p];
            if (pred && pred[property])
                vals.push(pred[property]);
        }
        return vals;
    }

    function getPredicateValue(notice, type, property) {
        var pred = getPredicateByType(notice, type);
        if (pred && pred[property] !== undefined) 
            return pred[property];
        return null;
    }

    /**
     Gets the first predicate that matches the type
     */
    function getPredicateByType(notice, type) {
        for (var p in notice.Predicates) {
            var pred = notice.Predicates[p];
            if (pred.PredicateType.toLowerCase() == type.toLowerCase())
                return pred;
        }
    }

    /**
     Get all predicates that match the type
     */
    function getPredicatesByType(notice, type) {
        var predicates = [];
        if (notice == null || type == null)
            return predicates;

        for (var p in notice.Predicates) {
            var pred = notice.Predicates[p];
            if (pred.PredicateType.toLowerCase() == type.toLowerCase())
                predicates.push(pred);
        }
        return predicates;
    }

    function getLastNotice() {
        return tickerQueue[tickerQueue.length - 1];
    }
    
    function setLastNotice(notice) {
        tickerQueue[tickerQueue.length - 1] = notice;
    }

    function pushNotice(notice) {
        tickerQueue.push(notice);
    }
  
    function getCityFromPredicate(notice, pred) {
        if (!pred)
            pred = "IdPredicate";
        var id = getPredicateValue(notice, pred, "Id");
       
        return id !== null ? Game.getCity(id) : null;
    }

    

})();










