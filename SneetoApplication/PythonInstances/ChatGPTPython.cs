using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SneetoApplication.PythonInstances
{
    public class ChatGPTPython
    {
        public static ConcurrentQueue<ChatGPTMessage> inputToPython;
        public static ConcurrentQueue<ChatGPTMessage> outputFromPython;
        public static readonly string config = "configuration.json";
        public static Dictionary<string, string> configuration;
        public static Thread thread;
        public static Process process;
        public static string startCommand = "xyz123zyx4321";
        public static string endCommand = "1234xyz321zyx";
        public static bool isInitialized = false;
        public static void StartPythonThread()
        {
            inputToPython = new ConcurrentQueue<ChatGPTMessage>();
            outputFromPython = new ConcurrentQueue<ChatGPTMessage>();
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);

            ThreadStart work = PythonProcess;
            thread = new Thread(work);
            thread.Start();
        }


        public static void StopPythonThread()
        {
            isInitialized = false;
            if (HasExited() == false)
                process.Kill();
            if (thread.IsAlive)
            {
                thread.Interrupt();
            }
        }

        public static bool HasExited()
        {
            try {
                return process.HasExited;
            }
            catch (Exception exc)
            {
                return true;
            }
        }

        private static void PythonProcess()
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = configuration["ChatGPTDirectory"];
            start.FileName = configuration["pythonExecutable"];
            start.Arguments = $"{configuration["ChatGPTDirectory"]}chatgpt_server.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            start.CreateNoWindow = true;

            isInitialized = false;
            ChatGPTMessage message;
            try
            {
                using (process = Process.Start(start))
                using (StreamWriter writer = process.StandardInput)
                using (StreamReader reader = process.StandardOutput)
                {
                    while (true)
                    {
                        Thread.Sleep(100);
                        if (inputToPython.TryDequeue(out message))
                        {
                            writer.WriteLine(message.inputText);
                            UIManager.Instance.printMessage("ChatGPT written: " + message.inputText);
                        }
                        else if (isInitialized)
                        {
                            continue;
                        }

                        if (process.HasExited)
                        {
                            UIManager.Instance.printMessage($"ChatGPT Python blew up in execution.");
                            process.Kill();
                            RequestManager.Instance.ResetFlag = true;
                            return;
                        }

                        if (isInitialized)
                        {
                            string result = "";
                            string line = "";

                            while (line != endCommand)
                            {
                                line = reader.ReadLine();
                                if (line == endCommand) break;
                                result += line + "\n";
                            }
                            message.outputText = result;
                            outputFromPython.Enqueue(message);
                            UIManager.Instance.printMessage("ChatGPT read: \n" + result + "\n");
                        }
                        else
                        {
                            string line = "";

                            while (line != startCommand)
                            {
                                if (process.HasExited)
                                {
                                    UIManager.Instance.printMessage($"ChatGPT Python blew up in execution.");
                                    process.Kill();
                                    RequestManager.Instance.ResetFlag = true;
                                    return;
                                }

                                line = reader.ReadLine();
                            }
                            UIManager.Instance.printMessage("Initialized ChatGPT python engine.");
                            isInitialized = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UIManager.Instance.printMessage($"Python blew up: {e}");
                Utilities.Utilities.WriteLineToFile(e.StackTrace, "log.txt");
            }
        }
    }
}
public class ChatGPTMessage {
    public string inputText;
    public string outputText;
    public long startTime;
}
