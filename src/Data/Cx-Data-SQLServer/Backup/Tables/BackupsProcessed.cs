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

        [System.Diagnostics.CodeAnalysis.AllowNull] 
        public string bak_FileName { get; set; }
        
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string database { get; set; }
        
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string LDbName { get; set; }
        
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string LLogName { get; set; }
        public string? scriptId { get; set; }
    }
}
