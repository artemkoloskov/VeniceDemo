using System;
using System.Collections.Generic;

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

				foreach (var pizza in OrderPizzas)
				{
					computedTotalcost += pizza.Pizza.Price;
				}

				return computedTotalcost;
			}
		}
	}
}
