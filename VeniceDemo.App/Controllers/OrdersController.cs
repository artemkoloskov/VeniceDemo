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
using VeniceDemo.App.Helpers;
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

			var orders = await FinanceHelper.GetOrdersData(_context, _customerId);

			ViewBag.TotalOrdersCost = FinanceHelper.GetTotalOrdersCost(orders);

			var payments = await FinanceHelper.GetPaymentsData(_context, _customerId);

			ViewBag.TotalAmountPaid = FinanceHelper.GetTotalAmountPaid(payments);

			ViewBag.CustomerPayments = payments;

			ViewBag.WeeklyData = FinanceHelper.GetWeeklyData(orders, payments);

            ViewBag.CurrentWeekKey = FinanceHelper.GetWeekKey(DateTime.Now);

            return View(orders);
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
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,CustomerId,DateCreated")] Order order)
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
            return View(order);
        }

        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
