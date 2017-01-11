using AspNetCoreSample.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreSample.Controllers.API
{
    [Route("api/token")]
    public class TokenController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TokenRequest model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest("Username and password must not be empty!");
            }

            if (model.Username != "tim" && model.Password != "secret")
            {
                return BadRequest("Invalid username or password!");
            }

            var dateTime = DateTime.Now.AddHours(1);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds + 60;

            // As an example, AuthService.CreateToken can return Jose.JWT.Encode(claims, YourTokenSecretKey, Jose.JwsAlgorithm.HS256);
            var token = Jose.JWT.Encode(new Dictionary<string, object>()
            {
                { "sub", "tim" },
                { "email", "tim@example.com" },
                { "exp", unixDateTime }
            }, Encoding.UTF8.GetBytes("w4t4l8sC8xa6f5n4S6P7sGTBN8Urgb0D"), Jose.JwsAlgorithm.HS256);

            var identity = new GenericIdentity("tim", "Token");
            var principal = new GenericPrincipal(identity, new string[0]);

            await HttpContext.Authentication.SignInAsync("Auth", principal);

            return Ok(token);
        }
    }
}
