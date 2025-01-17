using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class Visitor
    {
        [Key]
        public int Id { get; set; }

        public string SessionId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public bool PolicyAccepted { get; set; } = false;
    }
}
