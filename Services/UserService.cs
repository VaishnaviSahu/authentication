using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        User GetById(int Id);
        List<User> GetAll();
        bool PostNote(User note);
        void Update(User user, string password=null);
        bool Delete(int id);
        // List<User> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
       //private List<User> _users = new List<User>();
       UserContext not=null;
      
     //   private readonly AppSettings _appSettings;

        public UserService(UserContext _not)
        {
           
            this.not=_not;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = not.use.SingleOrDefault(x => x.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }
         public bool PostNote(User note){
          /* if(not.use.FirstOrDefault(n => n.Id == note.Id) == null){
                not.use.Add(note);
                //PostChecklist(note);
                not.SaveChanges();
                return true;
            }
            else{
                return false;
            }*/
             if (string.IsNullOrWhiteSpace(note.Password))
                return false;

            if (not.use.Any(x => x.Username == note.Username))
                 return false;

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(note.Password, out passwordHash, out passwordSalt);

            note.PasswordHash = passwordHash;
            note.PasswordSalt = passwordSalt;

            not.use.Add(note);
            not.SaveChanges();

            return true;
        }
    public List<User> GetAll()
        {
            // return users without passwords
            Console.WriteLine(not.use);
            return not.use.ToList();

        }
        public User GetById(int id)
        {
            // return users without passwords
            Console.WriteLine(not.use);
            return not.use.ToList().FirstOrDefault( use =>  use.Id == id);

        }
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
         private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
         public bool Delete(int id)
        {
            using(not)
            {
            User retrievedNote = not.use.FirstOrDefault(n => n.Id == id);
            if (retrievedNote != null){
                not.use.Remove(retrievedNote);
                not.SaveChanges();
                return true;
            }
            else{
                return false;
            }
        }

    }
    public void Update(User userParam, string password)
        {
            var user = not.use.Find(userParam.Id);

            if (user == null)
                throw new AppException("User not found");
                

            if (userParam.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (not.use.Any(x => x.Username == userParam.Username))
                    throw new AppException("Username " + userParam.Username + " is already taken");
                    //return false;
            }

            // update user properties
            // if(userParam.FirstName!=null)
            // {
            // user.FirstName = userParam.FirstName;
            // }
            // if(userParam.LastName!=null)
            // {
            // user.LastName = userParam.LastName;
            // }
            if(userParam.Country!=null)
            {
            user.Country = userParam.Country;
            }
            if(userParam.Email!=null)
            {
            user.Email = userParam.Email;
            }


            if(userParam.Username!=null)
            {
            user.Username = userParam.Username;
            }
            if(userParam.Password!=null)
            {
                user.Password=userParam.Password;
            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(userParam.Password))
            { 
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
            }

            not.use.Update(user);
            not.SaveChanges();
        }
}
}