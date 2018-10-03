using Newtonsoft.Json;

namespace DataManager.Models
{
    public abstract class BaseEntity
    {
        [JsonProperty(PropertyName = "id")]

        public string Id { get; set; }
    }
}
