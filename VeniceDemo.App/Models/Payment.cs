using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable

namespace VeniceDemo.App.Models
{
	public partial class Payment
	{
		public long Id { get; set; }
		public string DateCreated { get; set; }
		public double Amount { get; set; }
		public long CustomerId { get; set; }

		public virtual Customer Customer { get; set; }

		public static Dictionary<string, double> GetWeeklyPaymentsData(List<Payment> payments)
		{
			Dictionary<string, double> weeklyPaymentsData = new();

			CultureInfo cultureInfo = new CultureInfo("ru-RU");

			Calendar calendar = cultureInfo.Calendar;

			foreach (Payment payment in payments)
			{
				if (DateTime.TryParse(payment.DateCreated, out DateTime dateCreated))
				{
					int weekNumber = calendar.GetWeekOfYear(dateCreated, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

					string weekKey = $"{weekNumber} {dateCreated.Year}";

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

		public static double GetTotalAmountPaid(List<Payment> payments)
		{
			double totalAmount = 0;

			foreach (Payment payment in payments)
			{
				totalAmount += payment.Amount;
			}

			return totalAmount;
		}
	}
}
