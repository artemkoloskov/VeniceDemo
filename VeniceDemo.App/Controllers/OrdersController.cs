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
    /// <summary>
    /// Контроллер заказов, который одновременно выступает контроллером личного кабинета клиента.
    /// Функици свомещены в целях экономии времени на разработку.
    /// </summary>
	public class OrdersController : Controller
	{
		private readonly VeniceDBContext _context;
		private string _customerId;

		public OrdersController(VeniceDBContext context)
		{
			_context = context;
		}

		/// <summary>
        /// Страница Личный кабинет. Собирает данные по заказам и платежам клиента,
        /// представляет их в виде списокв, так же подситывает суммы заказов и платежей по неделям
        /// и тоже выводит эту информацию в виде списка. Проверяет на наличе долгов,
        /// дает ссылки на оплату долгов. Позволяет редактировать заказ (в целях тестирования, чтобы
        /// менять дату заказа)
        /// </summary>
        /// <returns></returns>
		[Authorize]
		public async Task<IActionResult> Index()
		{
			_customerId = User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value;

			var orders = await FinanceHelper.GetOrdersData(_context, _customerId);

			ViewBag.TotalOrdersCost = FinanceHelper.GetTotalOrdersCost(orders);

			var payments = await FinanceHelper.GetPaymentsData(_context, _customerId);

			ViewBag.TotalAmountPaid = FinanceHelper.GetTotalAmountPaid(payments);

			ViewBag.CustomerPayments = payments;

			ViewBag.WeeklyData = FinanceHelper.GetWeeklyData(orders, payments); // словарь с суммами закаов и платежей за сгруппированными по неделям

            ViewBag.CurrentWeekKey = FinanceHelper.GetWeekKey(DateTime.Now); // номер текущей недели, чтобы выводить инфоормационно клиенту

            return View(orders);
		}

		/// <summary>
        /// Возвращает страницу с деталями заказа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Страница редактирования заказа. Позволяет изменить дату заказа в целях тестирования
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Страница редактирования заказа. Позволяет изменить дату заказа в целях тестирования
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Провекра существования заказа в БД
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
