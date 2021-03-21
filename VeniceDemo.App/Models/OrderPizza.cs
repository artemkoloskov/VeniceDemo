using System;
using System.Collections.Generic;

#nullable disable

namespace VeniceDemo.App.Models
{
	public partial class OrderPizza
	{
		public long Id { get; set; }
		public long OrderId { get; set; }
		public long PizzaId { get; set; }

		public virtual Order Order { get; set; }
		public virtual Pizza Pizza { get; set; }
	}
}
