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
                AAAAAAAAAAAAAA resa = Grab();
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

        public static AAAAAAAAAAAAAA Grab()
        {
            var request = new RestClient("https://fortnite-api.com/v2/aes");
            var response = request.Execute(new RestRequest());

            AAAAAAAAAAAAAA res = JsonConvert.DeserializeObject<AAAAAAAAAAAAAA>(response.Content);
            keys = res.Data.DynamicKeys.ToDictionary(v => "FortniteGame/Content/Paks/" + v.PakName, v => v.Key);
            /* keys.Clear();
            keys.Add("FortniteGame/Content/Paks/pakchunk1004-WindowsClient", "581D53157B01D82CB0CDBD7A859D8774173C5F516C4D5AB3943C21C893BC3DC7");
            keys.Add("FortniteGame/Content/Paks/pakchunk1008-WindowsClient", "73576107EBD61D6470DD8BD6A6C1D18FD0328FC26376A60E2E7CABD18226C55A"); */
            return res;
        }
    }

    public class AAAAAAAAAAAAAA // aka response
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
