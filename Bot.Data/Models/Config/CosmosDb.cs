using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Data.Models.Config
{
    public class CosmosDb
    {
        public string Key { get; set; }
        public string Account { get; set; }
        public string Database { get; set; }
        public string Container { get; set; }
    }
}
