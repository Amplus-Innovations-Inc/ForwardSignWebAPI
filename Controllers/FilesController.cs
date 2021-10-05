using ForwardSignWebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using static ForwardSignWebAPI.Models.SyncedToolHandler;

namespace ForwardSignWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : Controller
	{
		private readonly IOptions<MyConfig> config;
		private readonly ILogger _logger;
		private string _token;

		public FilesController(IOptions<MyConfig> config, ILogger<FilesController> logger)
		{
			this.config = config;
			_logger = logger;
			_token = "";
		}

		[HttpPost("uploadFile")]
		public async Task<ActionResult<String>> UploadFile([FromForm] UploadFileModel uploadFileModel)
		{
			_logger.LogInformation("Log message in the UploadFile() method");
			if (uploadFileModel.Token == config.Value.APIToken)
			{
				try
				{
					_token = await GetAccessToken(config.Value);
					int id = await ExistWorkOrderName(config.Value, _token, uploadFileModel.WoID);
					if (id == 0)
					{
						// create a folder first
						id = await CreateFolder(config.Value, _token, uploadFileModel.WoID);
					}

					if (id != 0)
					{
						if (await UploadAFile(config.Value, _token, id, uploadFileModel.File))
						{
							return Ok("SUCCESS");
						}
						else
						{
							return this.StatusCode(StatusCodes.Status500InternalServerError, "Upload file is failed");
						}
					}
					else
					{
						return this.StatusCode(StatusCodes.Status500InternalServerError, "Cannot create/find a folder to contain the uploaded file");
					}
				}
				catch (Exception e)
				{
					return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
				}
			}
			return this.StatusCode(StatusCodes.Status401Unauthorized, "Invalid token");
		}

		[HttpPost("uploadExtraFile/{token}/{folder}")]
		public async Task<ActionResult<String>> UploadExtraFile(string token, string folder, [FromForm] ExtraUploadFileModel uploadFileModel)
		{
			_logger.LogInformation("Log message in the UploadExtraFile() method");
			if (token == config.Value.APIToken)
			{
				try
				{
					_token = await GetAccessToken(config.Value);
					int id = await ExistWorkOrderName(config.Value, _token, folder);
					if (id == 0)
					{
						// create a folder first
						id = await CreateFolder(config.Value, _token, folder);
					}

					if (id != 0)
					{
						if (await UploadAFile(config.Value, _token, id, uploadFileModel.File))
						{
							return Ok("SUCCESS");
						}
						else
						{
							return this.StatusCode(StatusCodes.Status500InternalServerError, "Upload file is failed");
						}
					}
					else
					{
						return this.StatusCode(StatusCodes.Status500InternalServerError, "Cannot create/find a folder to contain the uploaded file");
					}
				}
				catch (Exception e)
				{
					return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
				}
			}
			return this.StatusCode(StatusCodes.Status401Unauthorized, "Invalid token");
		}

		[HttpPost("uploadFileUsingFormData/{token}/{folder}")]
		public async Task<ActionResult<String>> UploadFileUsingFormData(string token, string folder)
		{
			_logger.LogInformation("Log message in the  UploadFileUsingFormData() method");
			try
			{
				var files = Request.Form.Files;

				foreach (IFormFile file in files)
				{
					_logger.LogInformation($"File.length: {file.Length}");
					if (file.Length == 0)
						continue;

					_logger.LogInformation($"File.Name: {file.FileName}");
					string tempFilename = Path.Combine(Path.GetTempPath(), file.FileName);
					_logger.LogInformation($"Saved file to: {tempFilename}");

					using (var fileStream = new FileStream(tempFilename, FileMode.Create))
					{
						_logger.LogInformation($"Copying File....");
						file.CopyTo(fileStream);
					}
				}
				return new OkObjectResult("Yes");
			}
			catch (Exception ex)
			{
				_logger.LogError("Error:" + ex.Message);
				return new BadRequestObjectResult(ex.Message);
			}
		}
	}
}
