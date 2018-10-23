using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace WebApi.Entities
{
    public class User
    {
        public int Id { get; set; }
       [Required]
        public string Username { get; set; }
        public string Password { get; set; }
        public string Country {get; set;}
        public string Email {get; set;}
      //  public string Token { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}