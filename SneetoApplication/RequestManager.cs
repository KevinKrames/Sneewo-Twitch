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
using SneetoApplication.PythonInstances;
using System.IO;

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

        public bool Clearing = false;
        public bool Resetting = false;
        public bool ResetFlag = false;
        bool waitingForValidation = false;
        bool waitingForChatGPT = false;
        bool waitingForTTSGPT = false;

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

        public void ReadIncomingFile()
        {
            if (!File.Exists($"{Brain.configuration["oneDrive"]}{REQUESTINFINISHEDFILE}"))
                return;
            requestsIncoming = JsonConvert.DeserializeObject<List<SitcomRequest>>(Utilities.Utilities.loadFile($"{Brain.configuration["oneDrive"]}{REQUESTINFINISHEDFILE}"));
            var requestsToRemove = new List<SitcomRequest>();
            if (requestsIncoming != null)
            {
                foreach (var req in requestsIncoming)
                {
                    foreach (var req2 in requestsOutgoing)
                    {
                        if (req.File == req2.File)
                        {
                            requestsToRemove.Add(req2);
                        }
                    }
                }
                foreach (var req3 in requestsToRemove)
                {
                    requestsOutgoing.Remove(req3);
                }
            }
        }

        public void Update()
        {
            if (ResetFlag == true)
            {
                ResetFlag = false;
                ResetPythonAndRequests();
            }
            while (Resetting == false && requestEventsToProcess.TryDequeue(out var requestEvent))
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
                            if (requestsOutgoing.Count < 5 && waitingForValidation == false)
                            {
                                waitingForValidation = true;
                                var sitcomRequest = new SitcomRequest { User = requestUser.Name, Text = requestEvent.Text, RequestState = RequestState.Unsent, channel = requestEvent.channel };
                                requestsOutgoing.Add(sitcomRequest);
                                    var thread = new Thread(async () => await ModerationCheckAsync(sitcomRequest));
                                    thread.Start();
                                
                                TwitchChatClient.Instance.sendMessage(requestEvent.channel.name, $"{requestEvent.Name}: request recorded, please wait.");
                            } else
                            {
                                UIManager.Instance.printMessage($"requestsOutgoing:{requestsOutgoing.Count}, waiting for validation:{waitingForValidation}");
                                TwitchChatClient.Instance.sendMessage(requestEvent.channel.name, $"{requestEvent.Name}: Theres too many requests pending, try again later.");
                            }
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

            
            if (Clearing && waitingForChatGPT == false && waitingForValidation == false && waitingForTTSGPT == false)
            {
                var channel = ChannelManager.Instance.Channels?.Values?.ToList()[0];
                requestsOutgoing.Clear();
                Clearing = false;
                TwitchChatClient.Instance.sendMessage(channel.name, $"Requests have been cleared out.");
            }

            if (waitingForChatGPT == false)
            {
                foreach (var sitcomRequest in requestsOutgoing)
                {
                    if (sitcomRequest.RequestState == RequestState.Validated)
                    {
                        waitingForValidation = false;

                        if (Clearing == false)
                        {
                            var chatGPTMessage = new ChatGPTMessage { inputText = sitcomRequest.Text, };
                            ChatGPTPython.inputToPython.Enqueue(chatGPTMessage);
                            sitcomRequest.RequestState = RequestState.WaitingForScript;
                            waitingForChatGPT = true;
                        }
                    }
                }
            }

            if (waitingForTTSGPT == false && ChatGPTPython.outputFromPython.TryDequeue(out var result))
            {
                waitingForChatGPT = false;

                if (Clearing == false)
                {
                    foreach (var sitcomRequest in requestsOutgoing)
                    {
                        
                        if (sitcomRequest.Text == result.inputText)
                        {
                            if (result.outputText.Replace("\n", "").StartsWith("ERROR"))
                            {
                                TwitchChatClient.Instance.sendMessage(sitcomRequest.channel.name, $"{sitcomRequest.User}: Error generating your request, the server might be overloaded.");
                                sitcomRequest.RequestState = RequestState.Failed;
                                waitingForChatGPT = false;
                            } else
                            {
                                sitcomRequest.File = result.outputText.Replace("\n", "");
                                var message = new TTSPythonMessage { inputText = sitcomRequest.File };
                                TTSPython.inputToPython.Enqueue(message);
                                sitcomRequest.RequestState = RequestState.WaitingForVoices;
                                waitingForTTSGPT = true;
                            }
                        }
                    }
                }
            }

            if (waitingForTTSGPT && TTSPython.outputFromPython.TryDequeue(out var ttsresult))
            {
                waitingForTTSGPT = false;
                if (Clearing == false)
                {
                    var currentRequest = false;
                    foreach (var sitcomRequest in requestsOutgoing)
                    {
                        currentRequest = false;
                        var lines = ttsresult.outputText.Split('\n');
                        foreach (var line in lines)
                        {
                            if (currentRequest && line.Replace("\n", "").StartsWith("ERROR"))
                            {
                                TwitchChatClient.Instance.sendMessage(sitcomRequest.channel.name, $"{sitcomRequest.User}: Error generating your request, the server might be overloaded.");
                                sitcomRequest.RequestState = RequestState.Failed;
                                waitingForTTSGPT = false;
                            }
                            if (sitcomRequest.File != null && line.Replace("\n", "").Contains(sitcomRequest.File.Replace(".txt", "")))
                            {
                                currentRequest = true;
                                sitcomRequest.File = sitcomRequest.File.Replace(".txt", "");
                                sitcomRequest.RequestState = RequestState.Processed;
                            }
                        }
                    }
                }
            }
            var requestsToRemove = new List<SitcomRequest>();
            foreach (var sitcomRequest in requestsOutgoing)
            {
                if (sitcomRequest.RequestState == RequestState.Failed)
                {
                    requestsToRemove.Add(sitcomRequest);
                }
            }
            foreach (var sitcomRequest in requestsToRemove) {
                waitingForValidation = false;
                requestsOutgoing.Remove(sitcomRequest); 
            }

            WriteOutGoingToFile();
            ReadIncomingFile();
        }

        public void ResetPythonAndRequests()
        {
            Resetting = true;
            Clearing = true;
            if (TTSPython.HasExited() == false)
            {
                TTSPython.StopPythonThread();
            }
            if (ChatGPTPython.HasExited() == false)
            {
                ChatGPTPython.StopPythonThread();
            }
            while(true)
            {

                if (ChatGPTPython.HasExited() == true && TTSPython.HasExited() == true)
                {
                    TTSPython.StartPythonThread();
                    ChatGPTPython.StartPythonThread();
                }
                if (ChatGPTPython.isInitialized && TTSPython.isInitialized) {
                    requestsOutgoing.Clear();
                    waitingForChatGPT = false;
                    waitingForTTSGPT = false;
                    waitingForValidation = false;
                    Resetting = false;
                    Clearing = false;
                    TwitchChatClient.Instance.sendMessage("Toomanyjims", $"Servers rebooted due to an error, requests have been cleared out.");
                    break;
                }
            }
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
            try
            {
                var httpResponse = await client.SendAsync(message);

                //var response = await client.PostAsync(uri, content);

                var responseString = await httpResponse.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ModerationsResponse>(responseString);
                UIManager.Instance.printMessage($"Reponse from open ai: {responseString}");

                if (response.Results.Any(it => it.Flagged) || Brain.Instance.IsValidSentence(sitcomRequest.Text) == false)
                {
                    TwitchChatClient.Instance.sendMessage(sitcomRequest.channel.name, $"{sitcomRequest.User}: Failed processing your request due to the message being flagged as inappropriate");
                    sitcomRequest.RequestState = RequestState.Failed;
                }
                else
                {
                    sitcomRequest.Validated = true;
                    sitcomRequest.RequestState = RequestState.Validated;
                    UIManager.Instance.printMessage($"Marked request as validated: {sitcomRequest.User}");
                }
            } catch (Exception e)
            {
                RequestManager.Instance.waitingForValidation = false;
                UIManager.Instance.printMessage($"Failed to validate cuz of http requests {e}");
                sitcomRequest.Validated = true;
                sitcomRequest.RequestState = RequestState.Validated;
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
        public string File;

        public override bool Equals(object obj)
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
        Validated,
        WaitingForScript,
        WaitingForVoices,
        Processed,
        Completed,
        Failed
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
