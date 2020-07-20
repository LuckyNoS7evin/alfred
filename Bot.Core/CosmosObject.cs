using Newtonsoft.Json;

namespace Bot.Core
{
    public class CosmosObject<T>
    {
        public CosmosObject() 
        {
            Object = typeof(T).ToString();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "object")]
        public string Object { get; set; }
        public T Item { get; set; }
    }
}
