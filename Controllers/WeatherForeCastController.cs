using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForwardSignWebAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class WeatherForeCastController : Controller
	{
		public IActionResult Index()
		{
			return Ok("Welcome to ForwardSign WebAPI");
		}
	}
}
