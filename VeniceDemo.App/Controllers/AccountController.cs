using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeniceDemo.App.Data;
using VeniceDemo.App.Models;
using VeniceDemo.App.ViewModels;


namespace VeniceDemo.App.Controllers
{
	public class AccountController : Controller
	{
		private readonly VeniceDBContext _context;

		public AccountController(VeniceDBContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginModel userToLogin)
		{
			List<Customer> customers = await _context.Customers.ToListAsync();

			Customer user = customers.Find(c => c.Login == userToLogin.Login && c.Password == userToLogin.Password);

			if (user is not null)
			{
				await Authenticate(user);

				return RedirectToAction("Index", "Home");
			}

			return Redirect("/Accout/Error");
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterModel userToRegister)
		{
			if (ModelState.IsValid)
			{
				Customer user = await _context.Customers.FirstOrDefaultAsync(u => u.Login == userToRegister.Login);

				if (user is null)
				{
					Customer newUser = new Customer() 
					{ 
						Login = userToRegister.Login, 
						Password = userToRegister.Password, 
						FirstName = userToRegister.FirstName, 
						SecondName = userToRegister.SecondName 
					};

					_context.Customers.Add(newUser);

					await _context.SaveChangesAsync();

					await Authenticate(newUser); // аутентификация

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("", "Пользователь уже существует");
				}
			}

			return View(userToRegister);
		}

		private async Task Authenticate(Customer user)
		{
			List<Claim> claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
					new Claim("Id", user.Id.ToString()),
					new Claim("FullName", user.FullName)
				};

			ClaimsIdentity claimsIdentity = new(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(claimsIdentity));
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Login", "Account");
		}
	}
}
