using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class DatabaseManager
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public DatabaseManager()
        {
            if (connection == null)
            {
                Initialize();
            }
        }

        public DatabaseManager(MySqlConnection connection) : this()
        {
            this.connection = connection;
        }

        //Converts MySQL query into JSON object
        public List<JObject> RetrieveQueryString(string query)
        {
            var data = new List<JObject>();
            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    var ob = new JObject();
                    for (int index = 0; index < dataReader.FieldCount; index++)
                    {
                        ob.Add(dataReader.GetName(index), JToken.FromObject(dataReader.GetValue(index)));
                    }
                    data.Add(ob);
                }

                dataReader.Close();

                CloseConnection();
            }

            return data;
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Initialize()
        {
            server = "localhost";
            database = "sneeto";
            uid = "root";
            password = "password";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";" +
            "SslMode=none";

            connection = new MySqlConnection(connectionString);
        }

        public void ExecuteQueryString(string query)
        {
            //open connection
            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                CloseConnection();
            }
        }
    }
}
