using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable

namespace VeniceDemo.App.Models
{
	/// <summary>
	/// Платеж
	/// </summary>
	public partial class Payment
	{
		public long Id { get; set; }
		public string DateCreated { get; set; }
		public double Amount { get; set; }
		public long CustomerId { get; set; }

		public virtual Customer Customer { get; set; }
	}
}
