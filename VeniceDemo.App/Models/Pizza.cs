using System;
using System.Collections.Generic;

#nullable disable

namespace VeniceDemo.App.Models
{
	public partial class Pizza
	{
		public Pizza()
		{
			OrderPizzas = new HashSet<OrderPizza>();
		}

		public long Id { get; set; }
		public string Name { get; set; }
		public double Price { get; set; }
		public double Weight { get; set; }

		public virtual ICollection<OrderPizza> OrderPizzas { get; set; }
	}
}
