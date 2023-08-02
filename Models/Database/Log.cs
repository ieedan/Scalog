using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scalog.Models.Database
{
    public class Log
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }

        public Log(string? message, string? type, bool useUtc)
        {
            Date = useUtc ? DateTime.UtcNow : DateTime.Now;
            Message = message;
            Type = type;
        }

        public Log() { }

        public override string ToString()
        {
            return $"[{Date}] {Type} - {Message}";
        }
    }
}
