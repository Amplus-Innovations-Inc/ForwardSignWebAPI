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

		[HttpPost("send/{token}/{folder}/{id}/{filename}")]
		public async Task<ActionResult<String>> SendFileID(string token, string folder, string id, string filename)
		{
			_logger.LogInformation("Log message in the  UploadFileUsingFormData() method");
			_logger.LogInformation("token:" + token);
			_logger.LogInformation("folder:" + folder);
			_logger.LogInformation("id:" + id);
			_logger.LogInformation("filename:" + filename);
			if (token == config.Value.APIToken)
			{
				byte[] fileBytes = null;
				try
				{
					string salesforceToken = await ForwardSignsHandler.GetForwardSignsAccessToken(config.Value);
					if (salesforceToken.Trim() != "")
					{
						fileBytes = await ForwardSignsHandler.DownloadFile(config.Value, salesforceToken, id);
					}

					_token = await GetAccessToken(config.Value);
					int workID = await ExistWorkOrderName(config.Value, _token, folder);
					if (workID == 0)
					{
						// create a folder first
						workID = await CreateFolder(config.Value, _token, folder);
					}

					if (workID != 0)
					{
						if (await UploadAFile(config.Value, _token, workID, fileBytes, "file", filename))
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
				catch (Exception ex)
				{
					return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
				}

			}
			return this.StatusCode(StatusCodes.Status401Unauthorized, "Invalid token");
		}
	}
}
