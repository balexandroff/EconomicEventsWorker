using System.Text.Json.Serialization;

namespace EconomicEventsWorker.Models
{
    public class FredObservationResponse
    {
        [JsonPropertyName("observations")]
        public List<FredObservationRecordResponse> Observations { get; set; }
    }

    public class FredObservationRecordResponse
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
