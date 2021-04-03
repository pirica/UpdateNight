using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateNight.Exceptions;
using UsmapNET.Classes;

namespace UpdateNight.Grabbers
{
    class MappingsGrabber
    {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<Mapping[]> GrabInfo()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://benbotfn.tk/api/v1/mappings");
            HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                int _try = 0;
                Global.Print(ConsoleColor.Yellow, "Mappings Grabber~Warn", $"Request failed with status {(int)response.StatusCode}");

                while (response.StatusCode != HttpStatusCode.OK)
                {
                    Global.Print(ConsoleColor.Yellow, "Mappings Grabber~Warn", $"Retrying to get mappings (attempt #{_try})");
                    response = await Client.SendAsync(request).ConfigureAwait(false);

                    _try++;
                    if (_try > 100)
                        throw new UpdateNightException($"Tried to grab mappings {_try} times but benbot seems to be down");

                    Thread.Sleep(1500); // 1.5 second
                }
            }

            string data = await response.Content.ReadAsStringAsync();
            Mapping[] res = JsonConvert.DeserializeObject<Mapping[]>(data);
            return res;
        }

        public static async Task Grab(Mapping mapping)
        {
            byte[] buffer = await Client.GetByteArrayAsync(mapping.Url).ConfigureAwait(false);

            Usmap usmap = new Usmap(buffer, new UsmapOptions
            {
                OodlePath = Path.Combine(Global.CurrentPath, "oo2core_8_win64.dll")
            });
            Global.Usmap = usmap;
        }
    }

    public class Mapping
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("uploaded")]
        public string Uploaded { get; set; }

        [JsonProperty("meta")]
        public Metadata Meta { get; set; }
    }
    public class Metadata
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("compressionMethod")]
        public string CompressionMethod { get; set; }
    }
}
