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
	/// <summary>
	/// Контроллер корзины клиента. Корзина хранится в сессии, откуда десериализуется и куда серилизуется JSONом
	/// </summary>
	public class CartController : Controller
	{
		private readonly VeniceDBContext _context;
		public List<Pizza> ShoppingCart { get; set; }

		public CartController(VeniceDBContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Главаня страница корзины
		/// </summary>
		/// <returns></returns>
		public IActionResult Index()
		{
			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			if (ShoppingCart is not null)
			{
				ViewBag.TotalOrderCost = GetTotalOrderCost();
			}
			
			return View(ShoppingCart);
		}

		/// <summary>
		/// подсчитывает полную стоиомсть заказа находящегося в корзине
		/// </summary>
		/// <returns></returns>
		private double GetTotalOrderCost()
		{
			return ShoppingCart.Sum(pizza => pizza.Price);
		}

		/// <summary>
		/// Добавляет пиццу указанного номера в корзину
		/// </summary>
		/// <param name="id">Идентификатор пиццы</param>
		/// <returns></returns>
		public async Task<IActionResult> AddToCart(string id)
		{
			var pizzas = await _context.Pizzas.ToListAsync();

			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			if (ShoppingCart is null) // если корзина пуста, создаем корзину, добавляем пиццу по id, сериализуем корзину и отправляем на хранение в сессию
			{
				ShoppingCart = new List<Pizza>();

				ShoppingCart.Add(pizzas.Find(pizza => pizza.Id.ToString() == id));

				SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);
			}
			else // если корзина пуста, создаем корзину, добавляем пиццу по id, сериализуем корзину и отправляем на хранение в сессию
			{
				ShoppingCart.Add(pizzas.Find(pizza => pizza.Id.ToString() == id));

				SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);
			}

			return RedirectToAction("Index", "Cart");
		}

		/// <summary>
		/// Удаляет пиццу из корзины
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public IActionResult DeleteItem(string id)
		{
			ShoppingCart = SessionHelper.GetObjectFromJson<List<Pizza>>(HttpContext.Session, "cart");

			int index = GetIndexOfPizzaInCart(ShoppingCart, id);

			ShoppingCart.RemoveAt(index);

			SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", ShoppingCart);

			return RedirectToAction("Index", "Cart");
		}

		/// <summary>
		/// Оформляет заказ. Если у клиента нет долга за прошлую неделю:
		/// записывает пиццы из корзины в новый заказ, сохраняет его в бд,
		/// переадресует на страницу Спасибо за заказ.
		/// Если долг есть, отправляет на страницу ошибка, где сообщаетя о долге и
		/// предлагается оплатить долг.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Преобразует корзину в заказ и сохраняет его в бд
		/// </summary>
		/// <param name="shoppingCart"></param>
		/// <param name="customerId"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Страницы Спасибо за заказ. Показывает клиенту номер заказа
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public IActionResult ThankYou(long orderId)
		{
			return View(orderId);
		}

		/// <summary>
		/// Страница ошибки создания заказа. Указывает размер задолженности и предлагает оплатить ее.
		/// </summary>
		/// <param name="debt"></param>
		/// <returns></returns>
		public IActionResult Error(double debt)
		{
			return View(debt);
		}

		/// <summary>
		/// Проверка наличия пиццы в корзине, используется перед удалением. Возвращает 
		/// индекс пиццы в списке пицц.
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="id"></param>
		/// <returns></returns>
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
