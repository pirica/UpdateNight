using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace UpdateNight.Helpers
{
    class Auth
    {
        public static string token = "";
        private static readonly HttpClient Client = new HttpClient();
        private static readonly string url = "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token";

        public static async Task<string> GetAuthToken()
        {
            if (!string.IsNullOrEmpty(token)) return token;
            if (Global.DeviceAuth == null)
            {
                Global.Print(ConsoleColor.Green, "AES Grabber", "\"deviceauth.json\" could not be found, therefor we cannot auth to get dynamic keys");
                return null;
            }

            DeviceAuth device = Global.DeviceAuth;
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "device_auth"}, {"token_type", "eg1"},
                    {"account_id", device.AccountId}, {"device_id", device.DeviceId}, {"secret", device.Secret},

                })
            };
            request.Headers.Add("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=");
            using var response = await Client.SendAsync(request).ConfigureAwait(false);

            string data = await response.Content.ReadAsStringAsync();
            token = JsonConvert.DeserializeObject<Oauth>(data).AccessToken;
            return token;
        }

        public static async Task<Oauth> GetManifestToken()
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"}, {"token_type", "eg1"}
                })
            };
            request.Headers.Add("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=");
            using var response = await Client.SendAsync(request).ConfigureAwait(false);

            string data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Oauth>(data);
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

    public class DeviceAuth
    {
        [JsonProperty("accountId")] public string AccountId { get; set; }
        [JsonProperty("deviceId")] public string DeviceId { get; set; }
        [JsonProperty("secret")] public string Secret { get; set; }
    }
}
