using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using aspnetcoregraphql.Models.Entities;

namespace aspnetcoregraphql.Data.Repositories
{
    public class UserRepository
    {
        public List<User> _users;
        public UserRepository()
        {
            _users = new List<User>() {
                new User()  { 
                    Id = 1, 
                    Email = "ahmed@test.com", 
                    Username = "ahmed", 
                    Password  = "123", 
                    FirstName = "Ahmed", 
                    LastName = "Elbougha",
                    Roles = new Claim[] {new Claim(ClaimTypes.Role, "Manager"), new Claim(ClaimTypes.Role, "User")}
                },
                new User() {
                    Id = 2, 
                    Email = "amal@test.com", 
                    Username = "amal", 
                    Password  = "123", 
                    FirstName = "Amal", 
                    LastName = "Samir",
                    Roles = new Claim[] {new Claim(ClaimTypes.Role, "User")}
                },
                new User() {
                    Id = 3, 
                    Email = "mohamed@test.com", 
                    Username = "mohamed", 
                    Password  = "123", 
                    FirstName = "Mohamed",
                    LastName = "AbdAllah",
                    Roles = new Claim[] {new Claim(ClaimTypes.Role, "Admin")}
                }
            };                
        }
        public User GetUser(string username)
        {
            try
            {
                return _users.First(user => user.Username.Equals(username));
            } catch
            {
                return null;
            } 
        }
        public User GetUser(string username, string password)
        {
            try
            {
                return _users.First(user => user.Username.Equals(username) && user.Password.Equals(password));
            } catch
            {
                return null;
            } 
        }             
    }
}