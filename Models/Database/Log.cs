using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scalog.Models.Database
{
    public class Log
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public Log(string message, string type)
        {
            Date = DateTime.Now;
            Message = message;
            Type = type;
        }
        public Log()
        {
            
        }
        public override string ToString()
        {
            return $"[{Date.ToString()}] {Type} - {Message}";
        }
    }
}
