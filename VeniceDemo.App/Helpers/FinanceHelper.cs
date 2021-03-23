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
	public static class FinanceHelper
	{
		public static double GetTotalOrdersCost(List<Order> orders)
		{
			double totalCost = 0;

			foreach (Order order in orders)
			{
				totalCost += order.TotalCost;
			}

			return totalCost;
		}

		public static double GetTotalAmountPaid(List<Payment> payments)
		{
			double totalAmount = 0;

			foreach (Payment payment in payments)
			{
				totalAmount += payment.Amount;
			}

			return totalAmount;
		}

		public static async Task<List<Order>> GetOrdersData(VeniceDBContext context, string customerId)
		{
			var customerOrders = context.Orders
				.Where(o => o.CustomerId + "" == customerId)
				.Include(o => o.OrderPizzas)
					.ThenInclude(op => op.Pizza);

			var orders = await customerOrders.ToListAsync();

			return orders;
		}

		public static async Task<List<Payment>> GetPaymentsData(VeniceDBContext context, string customerId)
		{
			var customerPayments = context.Payments
				.Where(p => p.CustomerId + "" == customerId);

			var payments = await customerPayments.ToListAsync();

			return payments;
		}

		public static async Task<Dictionary<string, (double ordered, double paid, double delta)>> GetWeeklyData(VeniceDBContext context, string customerId)
		{
			var orders = await GetOrdersData(context, customerId);

			var payments = await GetPaymentsData(context, customerId);

			return GetWeeklyData(orders, payments);
		}

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

		public static Dictionary<string, double> GetWeeklyPaymentsData(List<Payment> payments)
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

		public static int GetWeekNumber(DateTime date)
		{
			CultureInfo cultureInfo = new("ru-RU");

			Calendar calendar = cultureInfo.Calendar;

			return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
		}

		public static string GetWeekKey(DateTime date)
		{
			return $"{GetWeekNumber(date)} {date.Year}";
		}

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

		private static Dictionary<string, double> GetWeeklyOrdersData(List<Order> orders)
		{
			Dictionary<string, double> weeklyOrdersData = new();

			CultureInfo cultureInfo = new("ru-RU");

			Calendar calendar = cultureInfo.Calendar;

			foreach (Order order in orders)
			{
				if (DateTime.TryParse(order.DateCreated, out DateTime dateCreated))
				{
					int weekNumber = calendar.GetWeekOfYear(dateCreated, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

					string weekKey = $"{weekNumber} {dateCreated.Year}";

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
	}
}
