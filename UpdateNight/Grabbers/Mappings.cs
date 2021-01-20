using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using UsmapNET.Classes;

namespace UpdateNight.Grabbers
{
    class MappingsGrabber
    {
        public static Mapping[] mapingsinfo = null;
        public static Mapping mapping = null;

        public static Mapping[] GrabInfo()
        {
            var request = new RestClient("https://benbotfn.tk/api/v1/mappings");
            var response = request.Execute(new RestRequest());

            Mapping[] res = JsonConvert.DeserializeObject<Mapping[]>(response.Content);
            mapingsinfo = res;
            return res;
        }

        public static void Grab()
        {
            var request = new RestClient(mapping.Url);
            var response = request.Execute(new RestRequest());

            byte[] buffer = response.RawBytes;

            Usmap usmap = new Usmap(buffer, new UsmapOptions
            {
                OodlePath = Path.Combine(Global.current_path, "oo2core_8_win64.dll")
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
