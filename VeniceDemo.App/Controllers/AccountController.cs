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
		public async Task<IActionResult> LoginCustomer(Customer customer)
		{
			var customers = await _context.Customers.ToListAsync();

			var user = customers.Find(c => c.Login == customer.Login && c.Password == customer.Password);

			if (!(user is null))
			{
				await Authenticate(user);

				return RedirectToAction("Index", "Home");
			}

			return Redirect("/Accout/Error");
		}

		public async Task Authenticate(Customer user)
		{
			var claims = new List<Claim>
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
