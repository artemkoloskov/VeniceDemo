using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeniceDemo.App.Data;
using VeniceDemo.App.Models;

namespace VeniceDemo.App.Controllers
{
	/// <summary>
	/// Контроллер меню. Позволяет посмотреть все доступные пиццы и детали по ним
	/// </summary>
	public class PizzasController : Controller
	{
		private readonly VeniceDBContext _context;

		public PizzasController(VeniceDBContext context)
		{
			_context = context;
		}

		// GET: Pizzas
		public async Task<IActionResult> Index()
		{
			return View(await _context.Pizzas.ToListAsync());
		}

		// GET: Pizzas/Details/5
		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var pizza = await _context.Pizzas
				.FirstOrDefaultAsync(m => m.Id == id);
			if (pizza == null)
			{
				return NotFound();
			}

			return View(pizza);
		}
	}
}
