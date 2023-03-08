using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SneetoApplication
{
    public class RequestManager
    {
        private ConcurrentQueue<RequestEvent> requestEventsToProcess;

        private static RequestManager requestManager;

        private Dictionary<string, RequestUser> requestUsers;

        private static readonly HttpClient client = new HttpClient();

        private List<SitcomRequest> requestsOutgoing;
        private List<SitcomRequest> requestsIncoming;

        public static readonly string REQUESTOUTGOINGFILE = "requestOutgoing.json";
        public static readonly string REQUESTINFINISHEDFILE = "requestFinished.json";

        public static readonly string REQUESTALREADYPENDINGMESSAGE = "You already have a request pending. Please wait for it to complete";
        public static readonly string REQUESTINAPPROPRIATECONTENT = "That message has some inappropriate language in it, please don't be bad :(";
        public static readonly string REQUESTTORECENTMESSAGE = "You tried to request something to recently again. Please wait to resubmit: ";
        public static RequestManager Instance
        {
            get
            {
                if (requestManager == null)
                {
                    requestManager = new RequestManager();
                }
                return requestManager;
            }
            set
            {
                requestManager = value;
            }
        }

        public RequestManager()
        {
            requestEventsToProcess = new ConcurrentQueue<RequestEvent>();
            requestsOutgoing = new List<SitcomRequest>();
            requestsIncoming = new List<SitcomRequest>();
            requestUsers = new Dictionary<string, RequestUser>();
        }

        public void WriteOutGoingToFile()
        {
            var output = new List<SitcomRequest>();
            foreach (var sitcomRequest in requestsOutgoing)
            {
                if (sitcomRequest.Validated)
                    output.Add(sitcomRequest);
            }
            Utilities.Utilities.WriteToFile(Utilities.Utilities.JsonSerializeObjectList(output), Brain.configuration["oneDrive"], REQUESTOUTGOINGFILE);
        }

        public void Update()
        {
            while (requestEventsToProcess.TryDequeue(out var requestEvent))
            {
                //var command = commands.Where(c => c.name == value.Command.CommandText.ToLower()).FirstOrDefault();
                //if (command != null) command.Execute(value);
                if (!requestUsers.TryGetValue(requestEvent.Name, out var requestUser))
                {
                    requestUsers[requestEvent.Name] = new RequestUser { Name = requestEvent.Name, LastRequest = 0 };
                    requestUser = requestUsers[requestEvent.Name];
                }

                try
                {
                    long elapsedTicks = DateTime.Now.Ticks - requestUser.LastRequest;
                    TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                    if (elapsedSpan.TotalSeconds > long.Parse(Brain.configuration["requestCooldown"]))
                    {
                        if (!requestsOutgoing.Any(it => it.User == requestUser.Name))
                        {
                            var sitcomRequest = new SitcomRequest { User = requestUser.Name, Text = requestEvent.Text, RequestState = RequestState.Unsent, channel = requestEvent.channel};
                            requestsOutgoing.Add(sitcomRequest);
                            var thread = new Thread(async () => await ModerationCheckAsync(sitcomRequest));
                            thread.Start();
                        }
                        else
                        {
                            TwitchChatClient.Instance.sendMessage(requestEvent.channel.name, $"{requestEvent.Name}: {REQUESTALREADYPENDINGMESSAGE}");
                        }
                    }
                    else
                    {
                        TwitchChatClient.Instance.sendMessage(requestEvent.channel.name, $"{requestEvent.Name}: {REQUESTALREADYPENDINGMESSAGE}" + (long.Parse(Brain.configuration["requestCooldown"]) - elapsedSpan.TotalSeconds) + "s");
                    }
                }
                catch (Exception e)
                {
                    UIManager.Instance.printMessage($"Exception while processing message, stack trace: {e}.");
                }
            }

            WriteOutGoingToFile();
        }
        private static async Task ModerationCheckAsync(SitcomRequest sitcomRequest)
        {

            //var request = new ModerationsRequest(sitcomRequest.Text);

            //var jsonContent = JsonSerializer.Serialize(request);
            var values = new Dictionary<string, string>
              {
                  { "input", $"{sitcomRequest.Text}" },
              };

            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Brain.configuration["openai"]}");
            var uri = new Uri("https://api.openai.com/v1/moderations");

            var payload = JObject.FromObject(new { input = sitcomRequest.Text }).ToString();
            var message = new HttpRequestMessage
            {
                RequestUri = uri,
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };

            var httpResponse = await client.SendAsync(message);

            //var response = await client.PostAsync(uri, content);

            var responseString = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ModerationsResponse>(responseString);
            UIManager.Instance.printMessage($"Reponse from open ai: {responseString}");

            if (response.Results.Any(it => it.Flagged))
            {
                TwitchChatClient.Instance.sendMessage(sitcomRequest.channel.name, $"{sitcomRequest.User}: {REQUESTALREADYPENDINGMESSAGE}");
            } else
            {
                sitcomRequest.Validated = true;
                sitcomRequest.RequestState = RequestState.Waiting;
                UIManager.Instance.printMessage($"Marked request as complete: {sitcomRequest.User}");
            }
        }
        internal void CreateRequestEvent(string username, string text, Channel channel)
        {
            var temp = new RequestEvent { Text = text, Name = username,  TimeSent = DateTime.Now.Ticks, channel = channel};
            requestEventsToProcess.Enqueue(temp);
        }
    }
    public class RequestEvent
    {
        public string Name;
        public string Text;
        public long TimeSent;
        public Channel channel;
    }

    public class SitcomRequest
    {
        public string Text;
        public string User;
        public bool Validated;
        public RequestState RequestState;
        public Channel channel;

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                SitcomRequest sr = (SitcomRequest)obj;
                return (Text == sr.Text) && (User == sr.User) && Validated == sr.Validated && RequestState == sr.RequestState;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 854293728;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(User);
            return hashCode;
        }
    }

    public class RequestUser
    {
        public string Name;
        public long LastRequest;
    }

    public enum RequestState
    {
        Unsent,
        Waiting,
        Completed
    }

    public abstract class BaseResponse
    {
        /// <summary>
        /// The server-side processing time as reported by the API.  This can be useful for debugging where a delay occurs.
        /// </summary>
        [JsonIgnore]
        public TimeSpan ProcessingTime { get; internal set; }

        /// <summary>
        /// The organization associated with the API request, as reported by the API.
        /// </summary>
        [JsonIgnore]
        public string Organization { get; internal set; }

        /// <summary>
        /// The request id of this API call, as reported in the response headers.  This may be useful for troubleshooting or when contacting OpenAI support in reference to a specific request.
        /// </summary>
        [JsonIgnore]
        public string RequestId { get; internal set; }
    }

    public sealed class ModerationsResponse : BaseResponse
    {
        [JsonConstructor]
        public ModerationsResponse(string id, string model, IReadOnlyList<ModerationResult> results)
        {
            Id = id;
            Model = model;
            Results = results;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("model")]
        public string Model { get; }

        [JsonProperty("results")]
        public IReadOnlyList<ModerationResult> Results { get; }
    }
    public sealed class ModerationResult
    {
        [JsonConstructor]
        public ModerationResult(Categories categories, Scores scores, bool flagged)
        {
            Categories = categories;
            Scores = scores;
            Flagged = flagged;
        }

        [JsonProperty("categories")]
        public Categories Categories { get; }

        [JsonProperty("category_scores")]
        public Scores Scores { get; }

        [JsonProperty("flagged")]
        public bool Flagged { get; }
    }

    public sealed class Categories
    {
        [JsonConstructor]
        public Categories(bool hate, bool hateThreatening, bool selfHarm, bool sexual, bool sexualMinors, bool violence, bool violenceGraphic)
        {
            Hate = hate;
            HateThreatening = hateThreatening;
            SelfHarm = selfHarm;
            Sexual = sexual;
            SexualMinors = sexualMinors;
            Violence = violence;
            ViolenceGraphic = violenceGraphic;
        }

        [JsonProperty("hate")]
        public bool Hate { get; }

        [JsonProperty("hate/threatening")]
        public bool HateThreatening { get; }

        [JsonProperty("self-harm")]
        public bool SelfHarm { get; }

        [JsonProperty("sexual")]
        public bool Sexual { get; }

        [JsonProperty("sexual/minors")]
        public bool SexualMinors { get; }

        [JsonProperty("violence")]
        public bool Violence { get; }

        [JsonProperty("violence/graphic")]
        public bool ViolenceGraphic { get; }
    }
    public sealed class Scores
    {
        [JsonConstructor]
        public Scores(double hate, double hateThreatening, double selfHarm, double sexual, double sexualMinors, double violence, double violenceGraphic)
        {
            Hate = hate;
            HateThreatening = hateThreatening;
            SelfHarm = selfHarm;
            Sexual = sexual;
            SexualMinors = sexualMinors;
            Violence = violence;
            ViolenceGraphic = violenceGraphic;
        }

        [JsonProperty("hate")]
        public double Hate { get; }

        [JsonProperty("hate/threatening")]
        public double HateThreatening { get; }

        [JsonProperty("self-harm")]
        public double SelfHarm { get; }

        [JsonProperty("sexual")]
        public double Sexual { get; }

        [JsonProperty("sexual/minors")]
        public double SexualMinors { get; }

        [JsonProperty("violence")]
        public double Violence { get; }

        [JsonProperty("violence/graphic")]
        public double ViolenceGraphic { get; }
    }
}
