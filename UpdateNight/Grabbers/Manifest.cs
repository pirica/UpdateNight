using System;
using EpicManifestParser.Objects;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using UpdateNight.Exceptions;
using UpdateNight.Helpers;

namespace UpdateNight.Grabbers
{
    class ManifestGrabber
    {
        private static Oauth _token = null;
        private static ManifestInfo _manifestinfo = null;
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<ManifestInfo> GrabInfo()
        {
            if (_token == null) _token = await Auth.GetManifestToken();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live");
            request.Headers.Add("Authorization", $"bearer {_token.AccessToken}");
            var response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new UpdateNightException($"Request Failed with {(int) response.StatusCode}");
            Stream data = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            
            var manifestInfo = new ManifestInfo(data);
            _manifestinfo = manifestInfo;
            return manifestInfo;
        }

        public static async Task<Manifest> Grab()
        {
            if (_manifestinfo == null) await GrabInfo();

            var manifestData = await _manifestinfo.DownloadManifestDataAsync();
            var manfiest = new Manifest(manifestData, new ManifestOptions
            {
                ChunkBaseUri = new Uri("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV3/", UriKind.Absolute),
                ChunkCacheDirectory = Directory.CreateDirectory(Path.Combine(Global.CurrentPath, "FortniteChunks"))
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
    }
}
