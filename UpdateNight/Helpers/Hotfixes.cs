using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UpdateNight.Exceptions;

namespace UpdateNight.Helpers
{
    class Hotfixes
    {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<Dictionary<string, string>> Grab()
        {
            if (Global.DeviceAuth == null)
            {
                Global.Print(ConsoleColor.Yellow, "HotFixes Grabber", "\"deviceauth.json\" could not be found, therefor we cannot auth to get hotfixes");
                return null;
            }

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/cloudstorage/system");
            request.Headers.Add("Authorization", $"bearer {await Auth.GetAuthToken()}");
            var response = await Client.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new UpdateNightException($"Request cloudstorage failed with {(int)response.StatusCode} status code");

            string data = await response.Content.ReadAsStringAsync();
            List<IniResponse> inis = JsonConvert.DeserializeObject<List<IniResponse>>(data);

            inis = inis.OrderByDescending(f => f.Name).ToList();
            var id = inis.FirstOrDefault(f => f.Name.Contains("_DefaultGame.ini")).Uid;

            using HttpRequestMessage requesta = new HttpRequestMessage(HttpMethod.Get, "https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/cloudstorage/system/" + id);
            requesta.Headers.Add("Authorization", $"bearer {await Auth.GetAuthToken()}");
            var responsea = await Client.SendAsync(requesta).ConfigureAwait(false);
            if (responsea.StatusCode != HttpStatusCode.OK)
                throw new UpdateNightException($"Request cloudstorage file failed with {(int)responsea.StatusCode} status code");

            string content = await responsea.Content.ReadAsStringAsync();

            // https://stackoverflow.com/a/1879470 i love you stackoverflow
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            bool started = false;
            bool ended = false;

            // TODO: add other things but sets, for like challenges and stuff
            Dictionary<string, string> sets = new Dictionary<string, string>();

            while (!reader.EndOfStream)
            {
                if (ended) continue;
                string line = reader.ReadLine();

                if (line.Equals("[/Script/FortniteGame.FortTextHotfixConfig]"))
                {
                    started = true;
                    continue;
                }
                if (!started) continue;

                if (line.Contains("bIsMinimalPatch=True")
                    || !line.StartsWith("+TextReplacements=")
                    || !line.Contains("Namespace=\"CosmeticSets\"")) continue;

                var o = ParseLine(line);
                var parsed = JsonConvert.DeserializeObject<ParsedLine>(o.ToString());
                var text = parsed.TextReplacement;

                if (!sets.ContainsKey(text.Key)) // like, wtf epic
                    sets.Add(text.Key, text.LocalizedStrings.English);
            }

            return sets;
        }

        private static object ParseLine(string line)
        {
            if (line.StartsWith("+")) line = line.Replace("+", "");

            var split = line.Split("=");
            var key = split[0];
            var value = string.Join("=", split[1..]);
            dynamic o = new JObject();
            if (line.StartsWith("((")) // specific for "LocalizedStrings"
            {
                value = line.Remove(line.Length - 1, 1).Remove(0, 1);
                value = value.Remove(value.Length - 1, 1).Remove(0, 1);
                string[] values = value.Split("),(");

                foreach (var data in values)
                {
                    string aaaa = data.Replace("\"", "");
                    string keya = aaaa.Split(",")[0];
                    string valuea = aaaa.Split(",")[1];
                    o[keya] = valuea;
                }
            }
            else if (value.StartsWith("(")) // "array"
            {
                string[] values = value.Remove(value.Length - 1, 1).Remove(0, 1).Split(", ");
                dynamic oa = new JObject();

                foreach (var data in values)
                {
                    string keya = data.Split("=")[0];
                    dynamic valuea = string.Join("=", data.Split("=")[1..]);

                    if (valuea.StartsWith("(("))
                    {
                        valuea = ParseLine(valuea);
                    }
                    else if (valuea.StartsWith("\"")) valuea = valuea.Replace("\"", "");

                    oa[keya] = valuea;
                }

                o[key] = oa;
            }
            else // a "normal" value, like a string
                o[key] = value;

            return o;
        }

        public class IniResponse
        {
            [JsonProperty("uniqueFilename")]
            public string Uid { get; set; }

            [JsonProperty("filename")]
            public string Name { get; set; }
        }

        private class ParsedLine
        {
            [JsonProperty("TextReplacements")]
            public TextReplacements TextReplacement { get; set; }
        }

        private class TextReplacements
        {
            [JsonProperty("Category")]
            public string Category { get; set; }

            [JsonProperty("Namespace")]
            public string Namespace { get; set; }

            [JsonProperty("Key")]
            public string Key { get; set; }

            [JsonProperty("NativeString")]
            public string NativeString { get; set; }

            [JsonProperty("LocalizedStrings")]
            public LocalizedStrings LocalizedStrings { get; set; }
        }

        private class LocalizedStrings // http://www.lingoes.net/en/translator/langcode.htm
        {
            [JsonProperty("ar")]
            public string Arabic { get; set; }

            [JsonProperty("en")]
            public string English { get; set; }

            [JsonProperty("de")]
            public string German { get; set; }

            [JsonProperty("es")]
            public string Spanish { get; set; }

            [JsonProperty("es-419")]
            public string LatinSpanish { get; set; }

            [JsonProperty("fr")]
            public string French { get; set; }

            [JsonProperty("it")]
            public string Italian { get; set; }

            [JsonProperty("ja")]
            public string Japanese { get; set; }

            [JsonProperty("ko")]
            public string Korean { get; set; }

            [JsonProperty("pl")]
            public string Polish { get; set; }

            [JsonProperty("pt-BR")]
            public string PortugueseBrazil { get; set; }

            [JsonProperty("ru")]
            public string Russian { get; set; }

            [JsonProperty("tr")]
            public string Turkish { get; set; }

            [JsonProperty("zh-CN")]
            public string Chinese { get; set; }

            [JsonProperty("zh-Hant")]
            public string Mandarin { get; set; }
        }
    }
}
