﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [NotMapped]
        public string NewPassword { get; set; } 
        public string PhoneNumber { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string Role { get; set; } = "User";
        public bool PolicyUser { get; set; }

        public bool EmailConfirmed { get; set; } = false;

    }
}
