using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Entities;
using Microsoft.AspNetCore.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using WebApi.Helpers;
using WebApi.Models;
namespace WebApi.Controllers
{
  // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  //  [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
         private readonly AppSettings _appSettings;
        public UsersController(IUserService userService,IOptions<AppSettings> appSettings)
        {
            _userService = userService;
             _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody]User userParam)
        {
            var user = _userService.Authenticate(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
               
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new {
                Id = user.Id,
                Username = user.Username,
                Token = tokenString
            });
        }
         [HttpPost("register")]
        public IActionResult Post([FromBody]User userParam)
        {
           if(ModelState.IsValid){
                bool result = _userService.PostNote(userParam);
                if (result)
                {
                    return Created($"/Users/{userParam.Id}",userParam);
                }
                else
                {
                    return BadRequest("User already exists, please try .");
                }
            }
            return BadRequest("Invalid Format");
    }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            var users =  _userService.GetAll();
           
            return Ok(users);

        }
         [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user =  _userService.GetById(id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound($"User with {id} not found.");
            }
           // var userDto = _mapper.Map<UserDto>(user);
           // return Ok(user);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
           bool result = _userService.Delete(id);
             if(result){
                return Ok($"User with id : {id} deleted succesfully");
            }
            else{
                return NotFound($"User with {id} not found.");
            }
            
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]User userParam)
        {
            // map dto to entity and set id
         //   var user = _mapper.Map<User>(userDto);
          //  var result = _userService.PostNote(userParam);
            userParam.Id = id;
           // var user = _mapper.Map<User>(userDto);
           // user.Id = id;

            try 
            {
                // save 
                _userService.Update(userParam, userParam.Password);
                return Ok();
            } 
            catch(AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
      
          /*   if(ModelState.IsValid){
                bool result = _userService.Update(userParam, userParam.Password);
                
                if(result){
                    return Created($"/Users/{userParam.Id}",userParam);
                }
                else{
                    return NotFound($"Note with {id} not found.");
                }
            }
            return BadRequest("Invalid Format");*/
        }
    }
}
