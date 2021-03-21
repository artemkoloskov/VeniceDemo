using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeniceDemo.App.Data;
using VeniceDemo.App.Models;

namespace VeniceDemo.App.Controllers
{
	public class OrdersController : Controller
	{
		private readonly VeniceDBContext _context;

		public OrdersController(VeniceDBContext context)
		{
			_context = context;
		}

		// GET: Orders
		public async Task<IActionResult> Index()
		{
			var veniceDBContext = _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza);

			return View(await veniceDBContext.ToListAsync());
		}

		// GET: Orders/Details/5
		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// GET: Orders/Create
		public IActionResult Create()
		{
			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Login");

			return View();
		}

		// POST: Orders/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,DateCreated,CustomerId,TotalCost")] Order order)
		{
			if (ModelState.IsValid)
			{
				_context.Add(order);

				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Index));
			}

			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Login", order.CustomerId);

			return View(order);
		}

		// GET: Orders/Edit/5
		public async Task<IActionResult> Edit(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza)
				.Where(o => o.Id == id).FirstOrDefaultAsync();

			if (order == null)
			{
				return NotFound();
			}

			ViewData["CustomerFullName"] = order.Customer.FullName;

			return View(order);
		}

		// POST: Orders/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(long id, [Bind("Id,DateCreated,CustomerId,TotalCost")] Order order)
		{
			if (id != order.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(order);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!OrderExists(order.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}

			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Login", order.CustomerId);

			return View(order);
		}

		// GET: Orders/Delete/5
		public async Task<IActionResult> Delete(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// POST: Orders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(long id)
		{
			var order = await _context.Orders.FindAsync(id);

			_context.Orders.Remove(order);

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		private bool OrderExists(long id)
		{
			return _context.Orders.Any(e => e.Id == id);
		}
	}
}
