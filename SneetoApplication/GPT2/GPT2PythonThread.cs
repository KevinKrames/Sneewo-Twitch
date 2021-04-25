using SneetoApplication.GPT2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public static class GPT2PythonThread
    {
        public static ConcurrentQueue<GPT2Message> inputToPython;
        public static ConcurrentQueue<GPT2Message> outputFromPython;
        public static readonly string config = "configuration.json";
        public static Dictionary<string, string> configuration;
        public static Thread thread;
        public static string startCommand = "xyz123zyx4321";
        public static string endCommand = "1234xyz321zyx";
        public static void StartPythonThread()
        {
            inputToPython = new ConcurrentQueue<GPT2Message>();
            outputFromPython = new ConcurrentQueue<GPT2Message>();
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);

            ThreadStart work = PythonProcess;
            thread = new Thread(work);
            thread.Start();
        }

        public static void StopPythonThread()
        {
            if (thread.IsAlive)
                thread.Abort();
        }

        private static void PythonProcess()
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = configuration["gpt2Path"];
            start.FileName = configuration["pythonExecutable"];
            start.Arguments = $"{configuration["gpt2Path"]}interactive_conditional_samples_server.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            start.CreateNoWindow = true;

            var isInitialized = false;
            GPT2Message message;
            try
            {
                using (Process process = Process.Start(start))
                using (StreamWriter writer = process.StandardInput)
                using (StreamReader reader = process.StandardOutput)
                {
                    while (true)
                    {
                        Thread.Sleep(100);
                        if (inputToPython.TryDequeue(out message))
                        {
                            writer.WriteLine(message.inputText);
                            UIManager.Instance.printMessage("written: " + message.inputText);
                        }
                        else if (isInitialized)
                        {
                            continue;
                        }

                        if (process.HasExited)
                        {
                            UIManager.Instance.printMessage($"Python blew up in execution.");
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
                            UIManager.Instance.printMessage("read: \n" + result + "\n");
                        }
                        else
                        {
                            string line = "";

                            while (line != startCommand)
                            {
                                if (process.HasExited)
                                {
                                    UIManager.Instance.printMessage($"Python blew up in execution.");
                                    return;
                                }
                                
                                line = reader.ReadLine();
                            }
                            UIManager.Instance.printMessage("Initialized python engine.");
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
