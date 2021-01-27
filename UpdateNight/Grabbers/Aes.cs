using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateNight.Exceptions;

namespace UpdateNight.Grabbers
{
    class Aes
    {
        // cache, maybe ?
        public static Dictionary<string, string> Keys = new Dictionary<string, string>();
        private static bool _fetched;
        private static bool _newKey;
        private static readonly HttpClient Client = new HttpClient();

        public static async Task WaitUntilNewKey()
        {
            if (_fetched) return;

            while (!_newKey)
            {
                Response resa = await Grab();
                if (resa.Status != 200) continue;
                AES res = resa.Data;
                if (res.Version == Global.Version)
                {
                    _newKey = true;
                    Console.Write($"[{Global.BuildTime()}] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[AES Grabber] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Grabbed AES for ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Global.Version);
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(res.DynamicKeys.Count / 2);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" key{((res.DynamicKeys.Count / 2) > 1 ? "s" : "")})");
#endif
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\n");
                    _fetched = true;
                }
                if (!_newKey) Thread.Sleep(1000);
            }
            
        }

        public static async Task<Response> Grab()
        { 
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://fortnite-api.com/v2/aes");
            var response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new UpdateNightException($"Request failed with {(int) response.StatusCode} status code");
            
            string data = await response.Content.ReadAsStringAsync();
            Response res = JsonConvert.DeserializeObject<Response>(data);
            Keys = res.Data.DynamicKeys.ToDictionary(v => "FortniteGame/Content/Paks/" + v.PakName, v => v.Key);
            return res;
        }
    }

    public class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("data")]
        public AES Data { get; set; }
    }

    public class AES
    {
        [JsonProperty("build")]
        public string Version { get; set; }

        [JsonProperty("mainKey")]
        public string MainKey { get; set; }

        [JsonProperty("dynamicKeys")]
        public List<DynamicAes> DynamicKeys { get; set; }
    }

    public class DynamicAes
    {
        [JsonProperty("pakFilename")]
        public string PakName { get; set; }
        
        [JsonProperty("pakGuid")]
        public string PakGuid { get; set; }
        
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
