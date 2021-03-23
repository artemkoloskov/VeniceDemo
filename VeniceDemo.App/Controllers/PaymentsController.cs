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
    /// Контроллер платежей
    /// </summary>
    public class PaymentsController : Controller
    {
        private readonly VeniceDBContext _context;

        public PaymentsController(VeniceDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Страница с деталями платежа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        /// <summary>
        /// Страница создания новго платежа
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public IActionResult Create(double amount = 0)
        {
            ViewBag.Debt = amount;

            return View();
        }

        /// <summary>
        /// Экшн создающий новый платеж на основе данных формы заполненной на странице создания платежа
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateCreated,Amount,CustomerId")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                await _context.Entry(payment).GetDatabaseValuesAsync();
                return RedirectToAction("ThankYou", "Payments", new { paymentId = payment.Id });
            }
            return View(payment);
        }

        [HttpGet]
        public IActionResult ThankYou(long paymentId)
		{
            return View(paymentId);
		}
    }
}
