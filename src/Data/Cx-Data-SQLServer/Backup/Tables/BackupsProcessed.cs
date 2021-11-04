using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    public record BackupsProcessed
    {
        public int id { get; set; }
        public bool Restored { get; set; } = false;
        public int status { get; set; } = 1;
        public DateTime inserted { get; set; } = DateTime.UtcNow;
        public string bak_FileName { get; set; }
        public string database { get; set; }
        public string LDbName { get; set; }
        public string LLogName { get; set; }
        public string? scriptId { get; set; }
    }
}
