﻿using Newtonsoft.Json;
using SneetoApplication.Data_Structures;
using SneetoApplication.GPT2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class Brain
    {
        public static Dictionary<string, string> configuration;
        public static List<string> badWords;
        public static List<string> smallWords;
        public static List<string> ignores;
        public static readonly string USE_DATABASE = "useDatabase";
        public static readonly string config = "configuration.json";
        public static readonly string badWordsFile = "badWords.txt";
        public static readonly string smallWordsFile = "smallWords.txt";
        public static readonly string COMMANDCHAR = "commandChar";
        public static readonly string GPT2ENDTOKEN = "<|endoftext|>";

        public static Form1 form;
        public ConcurrentQueue<OnMessageReceivedArgs> messagesToProcess;
        public List<QueuedMessage> queuedMessages;
        public Brain(Form1 form1)
        {
            form = form1;
            messagesToProcess = new ConcurrentQueue<OnMessageReceivedArgs>();
            queuedMessages = new List<QueuedMessage>();
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);
            badWords = Utilities.Utilities.loadListFromTextFile(badWordsFile);
            smallWords = Utilities.Utilities.loadListFromTextFile(smallWordsFile);
            LoadIgnores();

            Instance = this;
        }

        private void LoadIgnores()
        {
            ignores = new List<string>();
            var ignoreJson = (Dictionary<string, object>)Utilities.Utilities.loadDictionaryFromJsonFile<string, object>("ignores.json");

            try
            {
                var names = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(ignoreJson["usersList"].ToString());

                foreach (var name in names)
                {
                    ignores.Add(name["name"].Trim().ToLower());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static Brain Instance { get; set; }

        public void Update()
        {
            while (messagesToProcess.TryDequeue(out OnMessageReceivedArgs value))
            {
                ProcessMessage(value);
            }

            List<QueuedMessage> messagesToRemove = new List<QueuedMessage>();

            foreach(var message in queuedMessages)
            {
                long elapsedTicks = DateTime.Now.Ticks - message.TimeSent;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                if (elapsedSpan.TotalSeconds > message.Delay)
                {
                    messagesToRemove.Add(message);
                    var channel = message.Event != null ? message.Event.ChatMessage.Channel : message.CommandEvent.Command.ChatMessage.Channel;
                    TwitchChatClient.Instance.sendMessage(channel, message.Sentence);
                    UIManager.Instance.SendMessage(channel, message.Sentence);
                }
            }

            foreach(var message in messagesToRemove)
            {
                queuedMessages.Remove(message);
            }

            if (GPT2PythonThread.outputFromPython.TryDequeue(out GPT2Message gpt2message))
            {
                ProcessGeneratedMessage(gpt2message);
            }
        }

        private void ProcessMessage(OnMessageReceivedArgs args)
        {
            try
            {
                if (ignores.Contains(args.ChatMessage.Username.Trim().ToLower())) return;

                if (IsValidSentence(args.ChatMessage.Message))
                {
                    ChannelMemoryManager.Instance.UpdateMemoryWithMessage(args.ChatMessage.Channel, args.ChatMessage.Message);
                }

                Utilities.Utilities.AppendMessageToLog(new MessageLog
                {
                    channel = args.ChatMessage.Channel.ToLower(),
                    time = DateTime.Now.Ticks.ToString(),
                    user = args.ChatMessage.Username.ToLower(),
                    message = args.ChatMessage.Message
                });

                ChannelMemoryManager.Instance.AddSentence(args.ChatMessage.Channel.ToLower(), args.ChatMessage.Message);

                var channel = ChannelManager.Instance.GetChannel(args);
                if (!channel.CanSpeak() || (Utilities.Utilities.RandomOneToNumber(100) < channel.chance && !args.ChatMessage.Message.Trim().ToLower().Contains(TwitchCredentials.Instance.twitchUsername.Trim().ToLower()))) return;

                UIManager.Instance.printMessage("Sent message to be gathered for: " + args.ChatMessage.Message);
                GPT2PythonThread.inputToPython.Enqueue(new GPT2Message
                {
                    inputText = GatherInput(args),
                    args = args,
                    startTime = GetTimeMilliseconds()
                });
                //var sentence = GenerateSentence(value);
                //if (sentence == null || sentence.Trim().ToLower() == "") return;

                //ChannelMemoryManager.Instance.AddSentence(value.ChatMessage.Channel.ToLower(), sentence);

                //queuedMessages.Add(new QueuedMessage
                //{
                //    Event = value,
                //    Sentence = sentence,
                //    TimeSent = DateTime.Now.Ticks,
                //    Delay = 0
                //});

                //channel.SetSpeakTime();
            }
            catch (Exception e)
            {
                UIManager.Instance.printMessage($"Exception while processing message, stack trace: {e}.");
                Utilities.Utilities.WriteLineToFile(e.StackTrace, "log.txt");
            }
        }
        private void ProcessGeneratedMessage(GPT2Message message)
        {
            var channel = ChannelManager.Instance.GetChannel(message.args);
            var sentence = getRandomSentence(message.outputText);
            if (sentence == null || sentence.Trim().ToLower() == "") return;

            UIManager.Instance.printMessage("Created message in " + (GetTimeMilliseconds() - message.startTime) + "ms.");
            ChannelMemoryManager.Instance.AddSentence(message.args.ChatMessage.Channel.ToLower(), sentence);

            queuedMessages.Add(new QueuedMessage
            {
                Event = message.args,
                Sentence = sentence,
                TimeSent = DateTime.Now.Ticks,
                Delay = 0
            });

            channel.SetSpeakTime();
        }

        private string GatherInput(OnMessageReceivedArgs args)
        {
            var wordList = args.ChatMessage.Message;

            var startTime = GetTimeMilliseconds();

            var runner = GPT2Runner.Instance;
            var inputSentences = String.Join("\n", ChannelMemoryManager.Instance.Channels[args.ChatMessage.Channel.ToLower()].GetMemorySentences().OrderBy(o => o.TimeSent).Select(it => it.Text));
            while (inputSentences.Length > 2000)
            {
                var inputList = inputSentences.Split('\n').ToList();
                inputList.RemoveAt(0);
                inputSentences = String.Join("\n", inputList);
            }
            return inputSentences + "\n";
        }

        private string GenerateSentence(OnMessageReceivedArgs value)
        {
            var wordList = value.ChatMessage.Message;

            var startTime = GetTimeMilliseconds();

            var runner = GPT2Runner.Instance;
            var inputSentences = String.Join("\n", ChannelMemoryManager.Instance.Channels[value.ChatMessage.Channel.ToLower()].GetMemorySentences().Select(it => it.Text));
            while (inputSentences.Length > 2000)
            {
                var inputList = inputSentences.Split('\n').ToList();
                inputList.RemoveAt(0);
                inputSentences = String.Join("\n", inputList);
            }
            inputSentences = inputSentences + "\n";
            var messages = runner.Generate_Sentence(inputSentences);

            var message = getRandomSentence(messages);

            Utilities.Utilities.WriteLineToFile($"Chose sentence: {message}", "generated_sentences.log");

            UIManager.Instance.printMessage("Created message in " + (GetTimeMilliseconds() - startTime)  + "ms.");

            return message;
        }

        private string getRandomSentence(string messages)
        {
            if (messages == null) return null;
            var listMessages = messages.Split('\r', '\n').ToList();
            listMessages = listMessages
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToList();

            //Log for testing
            Utilities.Utilities.WriteLineToFile($"Generated sentences: ", "generated_sentences.log");
            listMessages.ForEach(it => { Utilities.Utilities.WriteLineToFile(it, "generated_sentences.log"); });
            

            var messagesToRemove = new List<string>();
            var regex = "(={40}.SAMPLE.\\d.={40}|={80})";
            var firstSample = false;
            listMessages.ForEach(it => {
                if (Regex.Match(it, regex).Success) {
                    messagesToRemove.Add(it);
                    firstSample = true;
                    return;
                }
                if (!IsValidSentence(it))
                {
                    messagesToRemove.Add(it);
                    return;
                }
                //if (!it.Contains(GPT2ENDTOKEN))
                //{
                //    messagesToRemove.Add(it);
                //    return;
                //}
                if (firstSample)
                {
                    firstSample = false;
                    return;
                }
                messagesToRemove.Add(it);
            });

            messagesToRemove.ForEach(it => _ = listMessages.Remove(it));

            var messagesToReturn = new List<string>();
            listMessages.ForEach(it => messagesToReturn.Add(it.Split(new string[] { GPT2ENDTOKEN }, StringSplitOptions.None)[0]));

            return messagesToReturn.Count <= 0 ? null : messagesToReturn[Utilities.Utilities.RandomZeroToNumberMinusOne(messagesToReturn.Count)];
        }

        private void ProcessCommand(OnMessageReceivedArgs value)
        {
            
        }

        public long GetTimeMilliseconds() => (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public bool IsValidSentence(string sentence)
        {
            foreach (var badWord in Brain.badWords)
            {
                if (badWord.Length == 0 || badWord[0] == '#') continue;

                if (badWord[0] == '*')
                {
                    var temp = DoesContainAnyFormOfString(sentence, badWord.Substring(1));
                    if (temp) return false;
                }
                else if (DoesContainWord(sentence, badWord))
                {
                    return false;
                }
            }

            return true;
        }

        public bool DoesContainWord(string sentence, string word)
        {
            var wordLower = word.ToLower().Trim();
            var match = sentence.Split(' ').Where(w => w.ToLower().Trim().Equals(wordLower)).ToList();
            return match.Count > 0;
        }

        public bool DoesContainAnyFormOfString(string sentence, string word)
        {
            var wordLower = word.ToLower().Trim();
            var match = sentence.Split(' ').Where(w => w.ToLower().Trim().Contains(wordLower)).ToList();
            return match.Count > 0;
        }
    }
}
