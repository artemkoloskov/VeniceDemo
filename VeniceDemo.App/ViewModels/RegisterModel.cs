using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VeniceDemo.App.ViewModels
{
	/// <summary>
	/// Болаванка для формы регистрации клиента. собирает в себя регистрационные данные,
	/// по которым контроллер аккаунтов создаст новго клиента и откроет его сессию. Или сообщит, 
	/// что такого клиент уже существует.
	/// </summary>
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
