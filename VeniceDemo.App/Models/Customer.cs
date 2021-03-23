using System;
using System.Collections.Generic;

#nullable disable

namespace VeniceDemo.App.Models
{
	/// <summary>
	/// Клиент и одновременно аккаунт клиента
	/// </summary>
	public partial class Customer
	{
		public Customer()
		{
			Orders = new HashSet<Order>();

			Payments = new HashSet<Payment>();
		}

		public long Id { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }

		public virtual ICollection<Order> Orders { get; set; }
		public virtual ICollection<Payment> Payments { get; set; }

		public string FullName => $"{FirstName} {SecondName}";
	}
}
