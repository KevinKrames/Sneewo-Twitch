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
        public static String jsonSerialize(Object ob)
        {
            return JsonConvert.SerializeObject(ob);
        }

        public static object jsonUnserialize(String json)
        {
            if (json == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject(json);
        }

        public static dynamic loadJsonFile(string fileName)
        {
            string data = "";
            try
            {
                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + fileName);
                data = sr.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file" + fileName);
                Console.WriteLine(e.StackTrace);
            }
            return jsonUnserialize(data);
        }

        public static int RandomOneToNumber(int number)
        {
            return (int)(RandomZeroToOne() * (double)number)+1;
        }

        public static double RandomZeroToOne()
        {
            return random.NextDouble();
        }
    }
}