using Newtonsoft.Json;
using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SneetoApplication.Utilities
{
    public static class Utilities
    {
        public static Random random = new Random();
        public static string jsonSerialize(Object ob)
        {
            return JsonConvert.SerializeObject(ob);
        }

        public static Dictionary<string, string> jsonUnserialize(string json)
        {
            if (json == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static Dictionary<string, string> loadDictionaryFromJsonFile(string fileName)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(loadFile(fileName));
        }

        internal static List<string> loadListFromTextFile(string v)
        {
            string data = "";
            var newList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + v);
                var newLine = "";
                while ((newLine = sr.ReadLine()) != null)
                {
                    newList.Add(newLine);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file" + v);
                Console.WriteLine(e.StackTrace);
            }
            return newList;
        }

        public static string loadFile(string fileName)
        {
            string data = "";
            try
            {
                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + fileName);
                data = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file" + fileName);
                Console.WriteLine(e.StackTrace);
            }
            return data;
        }

        public static void WriteLineToFile(string data, string fileName)
        {
            var path = Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + fileName;
            File.AppendAllText(path, data + Environment.NewLine);
        }

        public static int RandomOneToNumber(int number)
        {
            if (number == 0) return 0;
            return (int)(RandomZeroToOne() * (double)number)+1;
        }

        public static double RandomZeroToOne()
        {
            return random.NextDouble();
        }
    }
}