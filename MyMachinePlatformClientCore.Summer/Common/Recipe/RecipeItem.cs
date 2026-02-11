using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
     
    public class RecipeItem
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        public RecipeItem()
        {
        }

        public RecipeItem(string key) => this.Key = key;

        public RecipeItem(string key, object data)
        {
            this.Key = key;
            this.DataType = data.GetType().AssemblyQualifiedName;
            this.Content = JsonConvert.SerializeObject(data);
        }

        public T ToObject<T>()
        {
            if (string.IsNullOrEmpty(this.Content)) return default(T);
            try
            {
                return JsonConvert.DeserializeObject<T>(this.Content);
            }
            catch (JsonException)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object ToObject()
        {
            if (string.IsNullOrEmpty(this.Content) || string.IsNullOrEmpty(this.DataType))
                return null;

            try
            {
                var type = Type.GetType(this.DataType);
                if (type == null)
                    return null;

                return JsonConvert.DeserializeObject(this.Content, type);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

}
