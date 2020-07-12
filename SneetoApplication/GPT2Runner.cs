using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class GPT2Runner
    {
        private static GPT2Runner runner;
        public static readonly string config = "configuration.json";
        public static Dictionary<string, string> configuration;

        public static GPT2Runner Instance
        {
            get
            {
                if (runner == null)
                {
                    runner = new GPT2Runner();
                }
                return runner;
            }
            set
            {
                runner = value;
            }
        }

        public GPT2Runner()
        {
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);
        }

        public string Generate_Sentence(string input)
        {
            if (input == null || input.Length == 0)
            {
                UIManager.Instance.printMessage("Tried to generate a message with no input.");
                return null;
            }
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                var pythonPath = configuration["gpt2Path"];
                start.WorkingDirectory = pythonPath;
                start.FileName = configuration["pythonExecutable"];
                start.Arguments = $"interactive_conditional_samples.py --input \"{input}\"";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        return reader.ReadToEnd();
                    }
                }
            } catch (Exception e)
            {
                UIManager.Instance.printMessage($"Tried to generate a message wbut threw an exception: {e}");
                return null;
            }
        }
    }
}
