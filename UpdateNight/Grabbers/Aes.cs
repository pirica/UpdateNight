using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateNight.Exceptions;

namespace UpdateNight.Grabbers
{
    class Aes
    {
        // cache, maybe ?
        public static Dictionary<string, string> Keys = new Dictionary<string, string>();
        private static readonly HttpClient Client = new HttpClient();

        public static async Task WaitUntilNewKey()
        {
            if (Global.DeviceAuth == null)
            {
                Global.Print(ConsoleColor.Yellow, "AES Grabber", "\"deviceauth.json\" could not be found, therefor we cannot auth to get dynamic keys\n");
                return;
            }

            List<string> res = await Grab();
            foreach (var data in res)
            {
                string[] yeah = data.Split(":");
                if (Keys.ContainsKey(yeah[0])) continue;
                byte[] bytes = Convert.FromBase64String(yeah[1]);
                string key = BitConverter.ToString(bytes).Replace("-", "");
                Keys.Add(yeah[0], key);
            }

            Console.Write($"[{Global.BuildTime()}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[AES Grabber] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Grabbed AES for ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Global.Version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n");

        }

        public static async Task<List<string>> Grab()
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/storefront/v2/keychain");
            request.Headers.Add("Authorization", $"bearer {await Helpers.Auth.GetAuthToken()}");
            var response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new UpdateNightException($"Request failed with {(int) response.StatusCode} status code");

            string data = await response.Content.ReadAsStringAsync();
            List<string> res = JsonConvert.DeserializeObject<List<string>>(data);
            return res;
        }
    }
}
