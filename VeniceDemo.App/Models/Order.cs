using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable

namespace VeniceDemo.App.Models
{
	public partial class Order
	{
		public Order()
		{
			OrderPizzas = new HashSet<OrderPizza>();
		}

		public long Id { get; set; }
		public string DateCreated { get => dateCreated; set => dateCreated = value; }
		public long CustomerId { get; set; }

		public virtual Customer Customer { get; set; }
		public virtual ICollection<OrderPizza> OrderPizzas { get; set; }

		private string dateCreated;

		public virtual double TotalCost
		{
			get
			{
				double computedTotalcost = 0;

				foreach (OrderPizza pizza in OrderPizzas)
				{
					computedTotalcost += pizza.Pizza.Price;
				}

				return computedTotalcost;
			}
		}

		public static Dictionary<string, double> GetWeeklyOrdersData(List<Order> orders)
		{
			Dictionary<string, double> weeklyOrdersData = new();

			CultureInfo cultureInfo = new CultureInfo("ru-RU");

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

		public static double GetTotalOrdersCost(List<Order> orders)
		{
			double totalCost = 0;

			foreach (Order order in orders)
			{
				totalCost += order.TotalCost;
			}

			return totalCost;
		}
	}
}
