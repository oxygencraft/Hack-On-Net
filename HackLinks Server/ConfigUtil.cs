using Newtonsoft.Json;
using System;
using System.IO;

namespace HackLinks_Server
{
    public static class ConfigUtil
    {
        /// <summary>
        /// Load config from the given path into the given <see cref="ConfigData"/> Object.
        /// Returns <c>true</c> if the file exists or <c>false</c> if it does not.
        /// </summary>
        /// <param name="path">The path to your config file</param>
        /// <param name="conf">ConfigData Object to populate</param>   
        /// <returns>If the file exists</returns>
        public static bool LoadConfig(string path, ConfigData conf)
        {
            if(File.Exists(path))
            {
                // read file into a string and deserialize JSON
                JsonConvert.PopulateObject(File.ReadAllText(path), conf);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save the config from the given <see cref="ConfigData"/> to the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="conf"></param>
        public static void SaveConfig(string path, ConfigData conf)
        {
            // write config to a new file
            File.WriteAllText(path, JsonConvert.SerializeObject(conf, Formatting.Indented));
        }

        public class ConfigData
        {
            public string MySQLServer { get; set; }
            public string Database { get; set; }
            public string UserID { get; set; }
            public string Password { get; set; }
        }
    }


}