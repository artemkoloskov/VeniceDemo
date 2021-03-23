using VeniceDemo.App.Helpers;
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
	public class CartController : Controller
	{
		private readonly VeniceDBContext _context;
		public List<Pizza> ShoppingCart { get; set; }

		public CartController(VeniceDBContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			if (ShoppingCart is not null)
			{
				ViewBag.TotalOrderCost = GetTotalOrderCost();
			}
			
			return View(ShoppingCart);
		}

		private double GetTotalOrderCost()
		{
			return ShoppingCart.Sum(pizza => pizza.Price);
		}

		public async Task<IActionResult> AddToCart(string id)
		{
			var pizzas = await _context.Pizzas.ToListAsync();

			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			if (ShoppingCart is null)
			{
				ShoppingCart = new List<Pizza>();

				ShoppingCart.Add(pizzas.Find(pizza => pizza.Id.ToString() == id));

				SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);
			}
			else
			{
				ShoppingCart.Add(pizzas.Find(pizza => pizza.Id.ToString() == id));

				SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);
			}

			return RedirectToAction("Index", "Cart");
		}

		public IActionResult DeleteItem(string id)
		{
			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			int index = GetIndexOfPizzaInCart(ShoppingCart, id);

			ShoppingCart.RemoveAt(index);

			SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);

			return RedirectToAction("Index", "Cart");
		}

		public async Task<IActionResult> Checkout()
		{
			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			string customerId = User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value;

			var weeklyFinanceData = await FinanceHelper.GetWeeklyData(_context, customerId);

			long orderId;

			if (weeklyFinanceData.ContainsKey(FinanceHelper.GetWeekKey(DateTime.Now)))
			{
				orderId = await CreateNewOrder(ShoppingCart, customerId);

				HttpContext.Session.Remove("cart");

				return RedirectToAction("ThankYou", "Cart", new { orderId = orderId });
			}

			string lastWeekThatHasData = FinanceHelper.GetLastWeekThatHasData(weeklyFinanceData.Keys);

			double debt = lastWeekThatHasData == "" ? 0 : weeklyFinanceData[lastWeekThatHasData].delta;

			if (debt > 0)
			{
				return RedirectToAction("Error", "Cart", new { debt = debt });
			}

			orderId = await CreateNewOrder(ShoppingCart, customerId);

			HttpContext.Session.Remove("cart");

			return RedirectToAction("ThankYou", "Cart", new { orderId = orderId });
		}

		private async Task<long> CreateNewOrder(List<Pizza> shoppingCart, string customerId)
		{
			Order newOrder = new()
			{
				CustomerId = long.Parse(customerId),
				DateCreated = DateTime.Now.ToString()
			};

			_context.Add(newOrder);

			await _context.SaveChangesAsync();

			await _context.Entry(newOrder).GetDatabaseValuesAsync();

			foreach (var pizza in shoppingCart)
			{
				OrderPizza orderPizza = new() { PizzaId = pizza.Id, OrderId = newOrder.Id };

				_context.Add(orderPizza);

				newOrder.OrderPizzas.Add(orderPizza);
			}

			await _context.SaveChangesAsync();

			return newOrder.Id;
		}

		public IActionResult ThankYou(long orderId)
		{
			return View(orderId);
		}

		public IActionResult Error(double debt)
		{
			return View(debt);
		}

		private int GetIndexOfPizzaInCart(List<Pizza> cart, string id)
		{
			for (var i = 0; i < cart.Count; i++)
			{
				if (cart[i].Id.ToString() == id)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
