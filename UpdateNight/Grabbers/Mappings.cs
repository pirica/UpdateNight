using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UsmapNET.Classes;

namespace UpdateNight.Grabbers
{
    class MappingsGrabber
    {
        public static Mapping[] mapingsinfo = null;
        public static Mapping mapping = null;
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<Mapping[]> GrabInfo()
        {
         //   var request = new RestClient("https://benbotfn.tk/api/v1/mappings");
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://benbotfn.tk/api/v1/mappings");
            HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Request Failed With {(int) response.StatusCode}, benbot seems to be down");

            string data = await response.Content.ReadAsStringAsync();
            Mapping[] res = JsonConvert.DeserializeObject<Mapping[]>(data);
            mapingsinfo = res;
            return res;
        }

        public static async Task Grab()
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
