using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VeniceDemo.App.ViewModels
{
	public class RegisterModel
	{
		[Required(ErrorMessage = "Не указан логин")]
		public string Login { get; set; }

		public string FirstName { get; set; }

		public string SecondName { get; set; }

		[Required(ErrorMessage = "Не указан пароль")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
