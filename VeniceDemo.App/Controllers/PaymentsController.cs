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
    public class PaymentsController : Controller
    {
        private readonly VeniceDBContext _context;

        public PaymentsController(VeniceDBContext context)
        {
            _context = context;
        }

        // GET: Payments/Details/5
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

        // GET: Payments/Create
        public IActionResult Create(double amount = 0)
        {
            ViewBag.Debt = amount;

            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
