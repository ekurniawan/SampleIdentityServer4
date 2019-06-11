using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SampleIdentityServer.Models;
using SampleIdentityServer.Models.AccountViewModels;
using SampleIdentityServer.Models.UserViewModels;

namespace SampleIdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.UserName, FirstName = model.FirstName, LastName = model.LastName, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            string role = "Basic User";

            if (result.Succeeded)
            {
                if (await _roleManager.FindByNameAsync(role) == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("userName", user.UserName));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("firstName", user.FirstName));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("lastName", user.LastName));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("role", role));

                return Ok(new ProfileViewModel(user));
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody]string rolename)
        {
            
            //IdentityResult roleResult;
            var roleExist = await _roleManager.RoleExistsAsync(rolename);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(rolename));
                return Ok("Create role success");
            }
            else
            {
                return BadRequest("Failed to create role");
            }
        }

     
        [HttpPost("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole([FromBody] UserRoleViewModel param)
        {
            var user = await _userManager.FindByNameAsync(param.username);
            try
            {
                await _userManager.AddToRoleAsync(user,param.role);
                return Ok("Role has beed added to user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                //throw new Exception($"Error {ex.Message}");
            }
        }
    }
}