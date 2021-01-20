using System;
using EpicManifestParser.Objects;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;

namespace UpdateNight.Grabbers
{
    class ManifestGrabber
    {
        private static Oauth _token = null;
        private static ManifestInfo _manifestinfo = null;
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<ManifestInfo> GrabInfo()
        {
            if (_token == null) await GetOauthTokenAsync();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, " https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live");
            request.Headers.Add("Authorization", $"bearer {_token.AccessToken}");
            var response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Request Failed with {(int) response.StatusCode}");
            Stream data = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            
            var manifestInfo = new ManifestInfo(data);
            _manifestinfo = manifestInfo;
            return manifestInfo;
        }

        public static async Task<Manifest> Grab()
        {
            if (_manifestinfo == null) await GrabInfo();

            var manifestData = await _manifestinfo.DownloadManifestDataAsync();
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
        
        public static async Task GetOauthTokenAsync()
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"}, {"token_type", "eg1"}
                })
            };
            request.Headers.Add("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=");
            using var response = await Client.SendAsync(request).ConfigureAwait(false);

            string data = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<Oauth>(data);
            _token = res;
        }
    }

    public class Oauth
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }
        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
        [JsonProperty("expires_at")] public DateTime ExpiresAt { get; set; }
        [JsonProperty("token_type")] public string TokenType { get; set; }
        [JsonProperty("client_id")] public string ClientId { get; set; }
        [JsonProperty("internal_client")] public bool InternalClient { get; set; }
        [JsonProperty("client_service")] public string ClientService { get; set; }
    }
}
