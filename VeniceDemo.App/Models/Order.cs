using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable

namespace VeniceDemo.App.Models
{
	/// <summary>
	/// Заказ
	/// </summary>
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

		/// <summary>
		/// Подсчитывает общую сумму заказа, суммирую стоимость всех пицц в заказе
		/// </summary>
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
	}
}
