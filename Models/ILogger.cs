using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scalog.Models
{
    internal interface ILogger
    {
        public void LogInfo(object value, string type);
        public Task LogInfoAsync(object value, string type);
        public void LogError(object value, string type, [CallerMemberName] string? name = null);
        public Task LogErrorAsync(object value, string type, [CallerMemberName] string? name = null);
    }
}
