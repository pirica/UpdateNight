using System;
using RestSharp;
using EpicManifestParser.Objects;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace UpdateNight.Grabbers
{
    class ManifestGrabber
    {
        private static string token = null;
        private static ManifestInfo manifestinfo = null;

        public static async Task<ManifestInfo> GrabInfo()
        {
            if (token == null) Auth();

            var infoStream = await CreateRequest("https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live");
            var manifestInfo = new ManifestInfo(infoStream);
            manifestinfo = manifestInfo;
            return manifestInfo;
        }

        public static async Task<Manifest> Grab()
        {
            if (manifestinfo == null) await GrabInfo();

            var manifestData = await manifestinfo.DownloadManifestDataAsync();
            // var manifestData = File.ReadAllBytes("C:\\Users\\G-Fire\\source\\repos\\UpdateNight\\bin\\15.20.manifest.bytes");
            var manfiest = new Manifest(manifestData, new ManifestOptions
            {
                ChunkBaseUri = new Uri("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV3/", UriKind.Absolute),
                ChunkCacheDirectory = Directory.CreateDirectory(Path.Combine(Global.current_path, "FortniteChunks"))
            });

#if DEBUG
            Console.Write($"[{Global.BuildTime()}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[Manifest Grabber] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Parsed Manifest of ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(manfiest.BuildVersion);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" in ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(manfiest.ParseTime.ToString("T"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
#endif

            return manfiest;
        }

        private static async Task<Stream> CreateRequest(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"bearer {token}");
            var response = await new HttpClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }

        private static string Auth()
        {
            // thanks sebas for the free code bop bop bop
            var request = new RestClient("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token");
            var response = request.Execute(
                new RestRequest(Method.POST)
                    .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                    .AddHeader("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=")
                    .AddParameter("grant_type", "client_credentials"));

            string res = JsonConvert.DeserializeObject<dynamic>(response.Content)["access_token"];
            token = res;
            return res;
        }
    }
}
