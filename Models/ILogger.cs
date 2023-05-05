using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scalog.Models
{
    internal interface ILogger
    {
        public void LogInfo(string message);
        public Task LogInfoAsync(string message);
        public void LogError(string message);
        public Task LogErrorAsync(string message);
    }
}
