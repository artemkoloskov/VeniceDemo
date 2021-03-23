using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VeniceDemo.App.Data;
using VeniceDemo.App.Models;

namespace VeniceDemo.App.Helpers
{
	/// <summary>
	/// Класс вспомогательных методов для подсчета финансовых данных по заказам и платежам
	/// </summary>
	public static class FinanceHelper
	{
		/// <summary>
		/// Подсчитывает полную стоимость заказов из списка заказов
		/// </summary>
		/// <param name="orders"></param>
		/// <returns></returns>
		public static double GetTotalOrdersCost(List<Order> orders)
		{
			double totalCost = 0;

			foreach (Order order in orders)
			{
				totalCost += order.TotalCost;
			}

			return totalCost;
		}

		/// <summary>
		/// Подсчитывает общую сумму платежей из списка платежей
		/// </summary>
		/// <param name="payments"></param>
		/// <returns></returns>
		public static double GetTotalAmountPaid(List<Payment> payments)
		{
			double totalAmount = 0;

			foreach (Payment payment in payments)
			{
				totalAmount += payment.Amount;
			}

			return totalAmount;
		}

		/// <summary>
		/// Загружает из бд список заказов клиента
		/// </summary>
		/// <param name="context"></param>
		/// <param name="customerId"></param>
		/// <returns></returns>
		public static async Task<List<Order>> GetOrdersData(VeniceDBContext context, string customerId)
		{
			var customerOrders = context.Orders
				.Where(o => o.CustomerId + "" == customerId)
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza);

			var orders = await customerOrders.ToListAsync();

			return orders;
		}

		/// <summary>
		/// Загружает из бд список платежей клиента
		/// </summary>
		/// <param name="context"></param>
		/// <param name="customerId"></param>
		/// <returns></returns>
		public static async Task<List<Payment>> GetPaymentsData(VeniceDBContext context, string customerId)
		{
			var customerPayments = context.Payments
				.Where(p => p.CustomerId + "" == customerId);

			var payments = await customerPayments.ToListAsync();

			return payments;
		}

		/// <summary>
		/// Подбивает заказы и платежи клиента в словарь с данными о суммарных платежах и заказах,
		/// группированные по неделям.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="customerId"></param>
		/// <returns></returns>
		public static async Task<Dictionary<string, (double ordered, double paid, double delta)>> GetWeeklyData(VeniceDBContext context, string customerId)
		{
			var orders = await GetOrdersData(context, customerId);

			var payments = await GetPaymentsData(context, customerId);

			return GetWeeklyData(orders, payments);
		}

		/// <summary>
		/// Подбивает списки заказов и платежей в словарь с данными о суммарных платежах и заказах,
		/// группированные по неделям.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="customerId"></param>
		/// <returns></returns>
		public static Dictionary<string, (double ordered, double paid, double delta)> GetWeeklyData(List<Order> orders, List<Payment> payments)
		{
			Dictionary<string, (double ordered, double paid, double delta)> weeklyData = new();

			var weeklyOrders = GetWeeklyOrdersData(orders);

			var weeklyPayments = GetWeeklyPaymentsData(payments);

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

		/// <summary>
		/// Возвращает номер недели, на которую приходится указанная дата
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static int GetWeekNumber(DateTime date)
		{
			CultureInfo cultureInfo = new("ru-RU");

			Calendar calendar = cultureInfo.Calendar;

			return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
		}

		/// <summary>
		/// Возвращает строкой номер недели и год, на которые приходится указанная дата
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static string GetWeekKey(DateTime date)
		{
			return $"{GetWeekNumber(date)} {date.Year}";
		}

		/// <summary>
		/// Находит в словаре с финансовыми данными по неделаям код последней недели, на которую есть данные.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static string GetLastWeekThatHasData(Dictionary<string, (double ordered, double paid, double delta)>.KeyCollection keys)
		{
			int maximum = 0;

			string lastWeekKey = "";

			foreach (string key in keys)
			{
				if (int.Parse(key.Split(' ')[1]) + int.Parse(key.Split(' ')[0]) > maximum)
				{
					maximum = int.Parse(key.Split(' ')[1]) + int.Parse(key.Split(' ')[0]);

					lastWeekKey = key;
				}
			}

			return lastWeekKey;
		}

		/// <summary>
		/// Преобразует список заказов в словарь с сумами заказов на определенную неделю
		/// </summary>
		/// <param name="orders"></param>
		/// <returns></returns>
		private static Dictionary<string, double> GetWeeklyOrdersData(List<Order> orders)
		{
			Dictionary<string, double> weeklyOrdersData = new();

			foreach (Order order in orders)
			{
				if (DateTime.TryParse(order.DateCreated, out DateTime dateCreated))
				{
					string weekKey = GetWeekKey(dateCreated);

					if (weeklyOrdersData.ContainsKey(weekKey))
					{
						weeklyOrdersData[weekKey] += order.TotalCost;
					}
					else
					{
						weeklyOrdersData.Add(weekKey, order.TotalCost);
					}
				}
			}

			return weeklyOrdersData;
		}

		/// <summary>
		/// Преобразует список платежей в словарь с сумами платежей на определенную неделю
		/// </summary>
		/// <param name="orders"></param>
		/// <returns></returns>
		private static Dictionary<string, double> GetWeeklyPaymentsData(List<Payment> payments)
		{
			Dictionary<string, double> weeklyPaymentsData = new();

			foreach (Payment payment in payments)
			{
				if (DateTime.TryParse(payment.DateCreated, out DateTime dateCreated))
				{
					string weekKey = GetWeekKey(dateCreated);

					if (weeklyPaymentsData.ContainsKey(weekKey))
					{
						weeklyPaymentsData[weekKey] += payment.Amount;
					}
					else
					{
						weeklyPaymentsData.Add(weekKey, payment.Amount);
					}
				}
			}

			return weeklyPaymentsData;
		}

		/// <summary>
		/// Собирает из двух списков ключей один список, исключая дубликаты
		/// </summary>
		/// <param name="list1"></param>
		/// <param name="list2"></param>
		/// <returns></returns>
		private static List<string> GetAllWeekKeys(List<string> list1, List<string> list2)
		{
			List<string> keys = new();

			foreach (List<string> list in new[] { list1, list2 })
			{
				foreach (string key in list)
				{
					if (!keys.Contains(key))
					{
						keys.Add(key);
					}
				}
			}

			return keys;
		}
	}
}
