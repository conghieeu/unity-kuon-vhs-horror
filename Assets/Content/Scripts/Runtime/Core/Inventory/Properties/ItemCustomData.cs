using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public sealed class ItemCustomData
    {
        [TextArea(3, 10)] public string JsonData;

        public JObject GetJson()
        {
            if(!string.IsNullOrEmpty(JsonData))
                return JObject.Parse(JsonData);

            return new JObject();
        }

        public T GetValue<T>(string key)
        {
            JObject json = JObject.Parse(JsonData);
            if (json != null && json.ContainsKey(key))
            {
                JToken value = json[key];
                return value.ToObject<T>();
            }

            return default;
        }

        public void SetValue(string key, object value)
        {
            JObject json = JObject.Parse(JsonData);
            if (json != null && json.ContainsKey(key))
            {
                json[key] = JToken.FromObject(value);
                JsonData = json.ToString();
            }
        }

        public void AddValue(string key, object value)
        {
            JObject json = JObject.Parse(JsonData);
            if (json != null && !json.ContainsKey(key))
            {
                json.Add(key, JToken.FromObject(value));
                JsonData = json.ToString();
            }
        }

        public void RemoveValue(string key)
        {
            JObject json = JObject.Parse(JsonData);
            if (json != null && json.ContainsKey(key))
            {
                json.Remove(key);
                JsonData = json.ToString();
            }
        }

        public void Update(JObject json)
        {
            JsonData = json.ToString();
        }

        public override string ToString() => JsonData;
    }
}