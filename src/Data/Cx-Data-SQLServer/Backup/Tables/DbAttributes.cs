using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    public record DbAttributes
    {
        public int id { get; set; }
        public bool active { get; set; }

        public string identifier { get; set; }
        public string? field1 { get; set; }
        public string? field2 { get; set; }
        public string? field3 { get; set; }
        public string? field4 { get; set; }
    }
}
