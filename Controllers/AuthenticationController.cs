using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using aspnetcoregraphql.Auth;
using aspnetcoregraphql.Models.Entities;
using System.Net.Http;

using aspnetcoregraphql.Data.Repositories;

namespace aspnetcoregraphql.Controllers
{
    [Route("auth")]
    [Produces("application/json")]
    public class AuthenticationController : Controller
    {
        [HttpPost]
        public IActionResult Post(string username, string password)
        {
            var auth = new AuhtenticationManager();
            if (auth.CheckUser(username, password))
            {
                var token = AuhtenticationManager.GenerateToken(username, 30);
                return Json(token);
            }
            return BadRequest("Wrong Username or Password");
        }
        
        [HttpGet]
        public IActionResult Validate(string token, string username)
        {
            bool exists = new UserRepository().GetUser(username) != null;

            if (!exists) return NotFound("Wrong Username or Password");
            
            string tokenUsername = AuhtenticationManager.ValidateToken(token);

            if (username.Equals(tokenUsername))
                return Ok("Username is valid");

            return BadRequest("Invalid Data");
        }
       
    }
}