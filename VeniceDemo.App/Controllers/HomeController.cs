using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VeniceDemo.App.Models;
using VeniceDemo.App.Data;

namespace VeniceDemo.App.Controllers
{
	public class HomeController : Controller
	{
		private readonly VeniceDBContext _context;
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger, VeniceDBContext context)
		{
			_logger = logger;

			_context = context;
		}

		[Authorize]
		public IActionResult Index()
		{
			var customers = _context.Customers.ToList();

			Customer loggedInCustomer = customers.FirstOrDefault(c => c.Id.ToString() == User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value);

			ViewBag.UserName = loggedInCustomer.FullName;

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
