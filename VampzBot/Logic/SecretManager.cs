using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VampzBot.Models;

namespace VampzBot.Logic
{
    public static class SecretManager
    {

        public static Secrets _secrets;

        public static void GetSecrets()
        {
            var jsonText = File.ReadAllText("secrets.json");
            _secrets = (Secrets)JsonConvert.DeserializeObject<Secrets>(jsonText);
        }

    }
}
