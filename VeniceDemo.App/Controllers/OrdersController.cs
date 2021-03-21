using System;
using System.Collections.Generic;
using System.Globalization;
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
		private string _customerId;

		public OrdersController(VeniceDBContext context)
		{
			_context = context;
		}

		// GET: Orders
		[Authorize]
		public async Task<IActionResult> Index()
		{
			_customerId = User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value;

			var orders = await GetOrdersData();

			var payments = await GetPaymentsData();

			ViewBag.CustomerPayments = payments;

			ViewBag.WeeklyData = GetWeeklyData(orders, payments);

			return View(orders);
		}

		private async Task<List<Payment>> GetPaymentsData()
		{
			await _context.Payments.LoadAsync();

			var customerPayments = _context.Payments
				.Where(p => p.CustomerId + "" == _customerId);

			var payments = await customerPayments.ToListAsync();

			ViewBag.TotalAmountPaid = Payment.GetTotalAmountPaid(payments);

			return payments;
		}

		private async Task<List<Order>> GetOrdersData()
		{
			var customerOrders = _context.Orders
				.Where(o => o.CustomerId + "" == _customerId)
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza);

			var orders = await customerOrders.ToListAsync();

			ViewBag.TotalOrdersCost = Order.GetTotalOrdersCost(orders);

			return orders;
		}

		private Dictionary<string, (double ordered, double paid, double delta)> GetWeeklyData(List<Order> orders, List<Payment> payments)
		{
			Dictionary<string, (double ordered, double paid, double delta)> weeklyData = new();

			var weeklyOrders = Order.GetWeeklyOrdersData(orders);

			var weeklyPayments = Payment.GetWeeklyPaymentsData(payments);

			List<string> allWeekKeys = GetAllWeekKeys(new List<string>(weeklyOrders.Keys), new List<string>(weeklyPayments.Keys));

			foreach (string weekKey in allWeekKeys)
			{
				double ordered = weeklyOrders.ContainsKey(weekKey) ? weeklyOrders[weekKey] : 0;

				double paid = weeklyPayments.ContainsKey(weekKey) ? weeklyPayments[weekKey] : 0;

				double delta = ordered - paid;

				weeklyData.Add(weekKey, (ordered, paid, delta));
			}

			return weeklyData;
		}

		private static List<string> GetAllWeekKeys(List<string> list1, List<string> list2)
		{
			List<string> allWeeklyKeys = new();

			foreach (List<string> list in new[] { list1, list2 })
			{
				foreach (string key in list)
				{
					if (!allWeeklyKeys.Contains(key))
					{
						allWeeklyKeys.Add(key);
					}
				}
			}

			return allWeeklyKeys;
		}

		// GET: Orders/Details/5
		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			Order order = await _context.Orders
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

			Order order = await _context.Orders
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

			Order order = await _context.Orders
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
			Order order = await _context.Orders.FindAsync(id);

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
