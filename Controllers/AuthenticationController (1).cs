﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AutoPartsHub.Controllers
{
	public class AuthenticationController : Controller
	{
		private readonly AutoPartsHubContext _UserContext;
		public AuthenticationController(AutoPartsHubContext context )
		{
			_UserContext = context;
		}


        [HttpPost]
        public async Task<IActionResult> Login(Login loginModel)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                {
                    throw new Exception("Email and Password are required.");
                }

                var user = await _UserContext.TblUsers
                    .Where(x => (x.MDelete == false || x.MDelete == null)
                        && x.Email.ToLower() == loginModel.Email.ToLower()
                        && x.Password == loginModel.Password)
                    .Include(x => x.Roll)
                    .FirstOrDefaultAsync();

                if (user == null || user.UserId <= 0)
                {
                    throw new Exception("Email or Password is incorrect");
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Roll.RollName),
            new Claim("RoleId", user.RollId.ToString()),
        };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = loginModel.RememberMe,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                var role = user.RollId;

                if (role == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (role == 2)
                {
                    return RedirectToAction("Index", "Home");
                }

                throw new Exception("Some things went wrong");
            }
            catch (Exception exp)
            {
                loginModel.Password = ""; // Clear the password for security
                loginModel.InvalidMessage = exp.Message;
                loginModel.isInvalid = true;
                return View(loginModel);
            }
        }


      

		[HttpPost]
		public async Task<IActionResult> Registeration(Registeration registerationModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var existingUser = await _UserContext.TblUsers
						.Where(x => x.Email.ToLower() == registerationModel.Email.ToLower())
						.FirstOrDefaultAsync();

					if (existingUser != null)
					{
						throw new Exception("User already exists with the same email.");
					}

					var newUser = new TblUser
					{
						UserName = registerationModel.Username,
						Email = registerationModel.Email,
						Password = registerationModel.Password,
						PhoneNumber = registerationModel.PhoneNumber,
						RollId = 2 // Set role to 2 for non-admin users
					};

					_UserContext.TblUsers.Add(newUser);
					await _UserContext.SaveChangesAsync();

					return RedirectToAction("Index", "Home");
				}

				throw new Exception("Invalid registration details");
			}
			catch (Exception exp)
			{
				registerationModel.Password = "";
				//registerationModel.ConfirmPassword = "";
				return Json(new { success = false, message = exp.Message });
			}
		}

		[Route("forgotpassword")]
		public IActionResult ForgotPassword()
		{
			return View();
		}
        [Route("Forbidden")]
        public IActionResult Forbidden()
		{
			return View();
		}

		[Route("Logout")]
		public async Task<IActionResult> LogoutAsync()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index","Home");
		}
	}
}
