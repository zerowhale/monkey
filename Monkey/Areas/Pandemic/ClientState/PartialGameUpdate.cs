using Monkey.Games.Pandemic.Board;
using Monkey.Games.Pandemic.Cards;
using Monkey.Games.Pandemic.State;
using Monkey.Games.Pandemic.State.Interrupts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.ClientState
{
    /// <summary>
    /// Conveys partial update information to the client, used to update the client-side
    /// data objects.  Usage of this assumes a full game update was previously sent to the client
    /// </summary>
    public class PartialGameUpdate: IGameUpdate
    {
        public PartialGameUpdate()
        {
        }

        /// <summary>
        /// Adds a player update object to the list of players being updated.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(PartialPlayerUpdate player)
        {
            if (Players == null)
                Players = new List<PartialPlayerUpdate>();
            Players.Add(player);
        }

        public void AddDrawnPlayerCard(ICard card)
        {
            if (drawnPlayerCards == null)
                drawnPlayerCards = new List<ICard>();
            drawnPlayerCards.Add(card);
        }

        public void AddEradicatedDisease(DiseaseColor color)
        {
            if (eradicatedDiseases == null)
                eradicatedDiseases = new List<DiseaseColor>();
            eradicatedDiseases.Add(color);
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName="DrawnInfectionCard")]
        public int? DrawnInfectionCardId
        {
            get
            {
                return DrawnInfectionCard == null ? null : (int?)DrawnInfectionCard.Id;
            }
        }

        [JsonIgnore]
        public ICard DrawnInfectionCard
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ExiledCardFromInfection
        {
            get;
            set;
        }

        /// <summary>
        /// Player data to update on the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PartialPlayerUpdate> Players
        {
            get;
            private set;
        }

        /// <summary>
        /// The player who's turn it is currently
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActivePlayer
        {
            get;
            set;
        }

        /// <summary>
        /// The active players remaining actions
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ActionsRemaining
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public GameState? State
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GameStateData StateData
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PartialMapUpdate Map
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] DrawnPlayerCards
        {
            get { 
                return drawnPlayerCards == null ? null : drawnPlayerCards.Select(x => x.Id).ToArray(); 
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? InfectionRateCount
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? OutbreakCount
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates the client should empty the infection discard pile.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? EpidemicIntensified
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates a disease has been cured
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DiseaseColor? Cure
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates that one or more diseases were eradicated
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DiseaseColor[] EradicatedDiseases
        {
            get
            {
                return eradicatedDiseases == null ? null : eradicatedDiseases.ToArray();
            }
        }

        /// <summary>
        /// Indicates that the interrupt state has been updated and passes the full interrupt
        /// object to the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public InterruptState Interrupt
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if a trade was accepted by both players.  
        /// False if the trade was rejected by either player,
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? TradeAccepted
        {
            get;
            set;
        }

        /// <summary>
        /// Used to reorder forecast cards on the client, so all other
        /// players can see reordering realtime.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] ForecastCardReorder
        {
            get;
            set;
        }

        /// <summary>
        /// Set to the final order of the forecast once it's submitted.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] ForecastComplete
        {
            get;
            set;
        }

        /// <summary>
        /// Informs players that a discard operation has completed
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? DiscardComplete
        {
            get;
            set;
        }




        private List<ICard> drawnPlayerCards;
        private List<DiseaseColor> eradicatedDiseases;
    }
}