using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace UpdateNight.Grabbers
{
    class Aes
    {
        // cache, maybe ?
        public static Dictionary<string, string> keys = new Dictionary<string, string>();
        private static bool _fetched = false;
        private static bool newKey = false;

        public static Task WaitUntilNewKey()
        {
            if (_fetched) return Task.CompletedTask;

            while (!newKey)
            {
                Response resa = Grab();
                if (resa.Status != 200) continue;
                AES res = resa.Data;
                if (res.Version == Global.version)
                {
                    newKey = true;
                    Console.Write($"[{Global.BuildTime()}] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[AES Grabber] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Grabbed AES for ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Global.version);
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
                if (!newKey) Thread.Sleep(1000);
            }

            return Task.CompletedTask;
        }

        public static Response Grab()
        {
            var request = new RestClient("https://fortnite-api.com/v2/aes");
            var response = request.Execute(new RestRequest());

            Response res = JsonConvert.DeserializeObject<Response>(response.Content);
            keys = res.Data.DynamicKeys.ToDictionary(v => "FortniteGame/Content/Paks/" + v.PakName, v => v.Key);
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
