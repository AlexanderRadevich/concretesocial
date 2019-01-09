using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Concretesocial.Core
{
    class Program
    {
        const string baseApiURL = "https://concretesocial.io/1.0/";
        private static string clientID = string.Empty;
        private static string clientSecret = string.Empty;
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            Output($"Base API Url: {baseApiURL}");

            while (true)
            {
                try
                {
                    if (string.IsNullOrEmpty(clientID))
                    {
                        Authorize();
                    }
                    else
                    {
                        ListActions();
                    }
                }
                catch (Exception ex)
                {
                    Output(ex.Message, MessageLevel.Warning);
                }
                OutputLine();
            }
        }

        private static void ListActions()
        {
            Output("Press 1 to list active profiles");
            Output("Press 2 to post media");
            Output("Press 3 to exit");
            var selectedAction = Console.ReadKey();
            Console.WriteLine("");
            switch (selectedAction.KeyChar)
            {
                case '1':
                    ListProfiles();
                    break;
                case '2':
                    PostMedia();
                    break;
                case '3':
                    Environment.Exit(0);
                    break;
                default:
                    Output("Unknown action", MessageLevel.Warning);
                    break;
            }
        }

        private static void PostMedia()
        {
            MediaRequestItem newItem = new MediaRequestItem();

            Output("Please enter caption: ");
            newItem.caption = Console.ReadLine();
            Output("Please enter media type (image or video): ");
            newItem.media_type = Console.ReadLine();
            Output("Please enter media url: ");
            newItem.media_url = Console.ReadLine();
            Output("Please enter profile id: ");
            newItem.profiles = new string[1];
            newItem.profiles[0] = Console.ReadLine();
            Output("Please enter comment: ");
            newItem.comment = Console.ReadLine();

            APIResponseItem result = MakeRequest<APIResponseItem>("publish ", newItem);
            if(result.result != null && result.result.Any())
            {
                Output("Result", MessageLevel.Info);
                foreach(var resultItem in result.result)
                {
                    if(string.IsNullOrEmpty(resultItem.media))
                    {
                        Output($"Error: {resultItem.response.error}", MessageLevel.Warning);
                    }
                    else
                    {
                        Output($"Media: {resultItem.media}", MessageLevel.Info);
                    }
                }
            }
        }

        private static void ListProfiles()
        {
            var profiles = MakeRequest<ProfileItem[]>("profiles");
            Output("Result", MessageLevel.Info);
            if (profiles != null && profiles.Any())
            {
                foreach (var profile in profiles)
                {
                    Output(profile, MessageLevel.Info);
                }
            }
            else
            {
                Output("There are no active profiles", MessageLevel.Info);
            }
        }

        private static T MakeRequest<T>(string api, object data = null) where T : class
        {
            client.DefaultRequestHeaders.Accept.Clear();
            if(!client.DefaultRequestHeaders.Contains("client_id"))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Concretesocial.Core TestApp");
                client.DefaultRequestHeaders.Add("client_id", clientID);
                client.DefaultRequestHeaders.Add("client_secret", clientSecret);
            }
            HttpResponseMessage response = null;
            string url = $"{baseApiURL}{api}";
            Output($"Call to: {url}", MessageLevel.Info);

            if (data != null)
            {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                response = client.PostAsync(url, content).Result;
            }
            else
            {
                response = client.GetAsync(url).Result;
            }
           
            Output($"Status Code: {((int)response.StatusCode)}", MessageLevel.Info);
            var responseData = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            else
            {
                throw new Exception($"Cannot process an api request: {responseData}");
            }
        }

        private static void Authorize()
        {
            Output("Please Enter a valid client_id");
            clientID = Console.ReadLine();
            Output("Please Enter a valid client_secret");
            clientSecret = Console.ReadLine();
            
        }

        private static void OutputLine()
        {
            Output("----------------------------------", MessageLevel.Info);
        }

        private static void Output(object data, MessageLevel level)
        {
            Output(data.ToString(), level);
        }

        private static void Output(string message, MessageLevel level = MessageLevel.Default)
        {
            switch (level)
            {
                case MessageLevel.Info: Console.ForegroundColor = ConsoleColor.Green; break;
                case MessageLevel.Warning: Console.ForegroundColor = ConsoleColor.Red; break;
                default: Console.ResetColor(); break;
            }
            Console.WriteLine(message);
        }

        private enum MessageLevel
        {
            Default,
            Warning,
            Info
        }

        #region API Objects

        public class ProfileItem
        {
            public string id { get; set; }
            public int followers_count { get; set; }
            public int follows_count { get; set; }
            public int media_count { get; set; }
            public string custom_id { get; set; }
            public string user { get; set; }

            public override string ToString()
            {
                return $"id: {id}, user: {user}, followers_count: {followers_count}, follows_count: {follows_count}, media_count: {media_count}";
            }
        }

        public class MediaRequestItem
        {
            public string caption { get; set; }
            public string media_type { get; set; }
            public string media_url { get; set; }
            public string[] profiles { get; set; }
            public string comment { get; set; }
        }

        public class APIResponseItem
        {
            public string request_id { get; set; }
            public Result[] result { get; set; }
        }

        public class Result
        {
            public string profile { get; set; }
            public string media { get; set; }
            public Response response { get; set; }
        }

        public class Response
        {
            public Error error { get; set; }
        }

        public class Error
        {
            public string message { get; set; }
            public string error_user_title { get; set; }
            public string error_user_msg { get; set; }

            public override string ToString()
            {
                return $"{message} {Environment.NewLine} {error_user_title} {Environment.NewLine} {error_user_msg}";
            }
        }

        #endregion

    }
}
