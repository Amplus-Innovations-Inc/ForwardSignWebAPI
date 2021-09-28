using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForwardSignWebAPI.Models
{
	public class UploadFileModel
	{
		public string Token { get; set; }
		public string WoID { get; set; }
		public IFormFile File { get; set; }
	}
}
