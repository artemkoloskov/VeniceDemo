using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VeniceDemo.App.ViewModels
{
	/// <summary>
	/// Болаванка для формы входа клиента. собирает в себя аутентификационные данные,
	/// по которым контроллер аккаунтов найдет клиента и откроет его сессию. Или сообщит, 
	/// что такого кдиента нет.
	/// </summary>
	public class LoginModel
	{
		[Required(ErrorMessage = "Не указан логин")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Не указан пароль")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
