using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scalog.Models
{
    internal interface ILogger
    {
        public void LogInfo(string message, string type);
        public Task LogInfoAsync(string message, string type);
        public void LogError(string message, string type);
        public Task LogErrorAsync(string message, string type);
    }
}
