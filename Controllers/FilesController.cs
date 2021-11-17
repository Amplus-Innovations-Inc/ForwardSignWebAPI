using ForwardSignWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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

		[HttpPost("send/{token}/{year}/{projectCreator}/{projectName}/{workOrderNumber}/{itemName}/{id}/{fileName}")]
		public async Task<ActionResult<String>> SendFileID(string token, string year, string projectCreator, string projectName, string workOrderNumber, string itemName, string id, string fileName)
		{
			_logger.LogInformation("Log message in the  UploadFileUsingFormData() method");

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
					int yearID = await ExistWorkOrderName(config.Value.WorkOrderID, config.Value, _token, year);

					if (yearID == 0)
					{
						// create a year folder first
						yearID = await CreateFolder(config.Value.WorkOrderID, config.Value, _token, year);
					}
					if (yearID != 0)
					{
						//create projectcreator folder under year
						int projectCreatorID = await ExistWorkOrderName(yearID, config.Value, _token, projectCreator);
						if (projectCreatorID == 0)
						{
							projectCreatorID = await CreateFolder(yearID, config.Value, _token, projectCreator);
						}
						if (projectCreatorID != 0)
						{
							//creator projectname folder under projectcreator
							int projectNameID = await ExistWorkOrderName(projectCreatorID, config.Value, _token, projectName);
							if (projectNameID == 0)
							{
								projectNameID = await CreateFolder(projectCreatorID, config.Value, _token, projectName);
							}
							if (projectNameID != 0)
							{
								//create workOrderNumber folder under projectName
								int workOrderNumberID = await ExistWorkOrderName(projectNameID, config.Value, _token, workOrderNumber);
								if (workOrderNumberID == 0)
								{
									workOrderNumberID = await CreateFolder(projectNameID, config.Value, _token, workOrderNumber);
								}
								if (workOrderNumberID != 0)
								{
									//If item is "null", script won't create item folder but script will copy a file into workOrderNumber folder
									if (itemName.Equals("null", StringComparison.OrdinalIgnoreCase))
									{
										if (await UploadAFile(config.Value, _token, workOrderNumberID, fileBytes, "file", fileName))
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
										//If item isn't null, script will check and create item folder then copy a file into it
										int itemID = await ExistWorkOrderName(workOrderNumberID, config.Value, _token, itemName);
										if (itemID == 0)
										{
											itemID = await CreateFolder(workOrderNumberID, config.Value, _token, itemName);
										}
										if (itemID != 0)
										{
											if (await UploadAFile(config.Value, _token, itemID, fileBytes, "file", fileName))
											{
												return Ok("SUCCESS");
											}
											else
											{
												return this.StatusCode(StatusCodes.Status500InternalServerError, "Upload file is failed");
											}
										}
									}
								}
							}
						}
						else
						{
							return this.StatusCode(StatusCodes.Status500InternalServerError, "Cannot create/find a folder to contain the uploaded file");
						}
					}
				}
				catch (Exception ex)
				{
					return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
				}
			}
			return this.StatusCode(StatusCodes.Status401Unauthorized, "Invalid token");
		}

		[HttpPost("send/{token}/{year}/{projectCreator}/{projectName}/{workOrderNumber}/{itemName}/{id}/{filename}/delete")]
		public async Task<ActionResult<String>> CheckandDeleteFile(string token, string year, string projectCreator, string projectName, string workOrderNumber, string itemName, string filename)
		{
			int fileID;
			_logger.LogInformation("Log message in the  UploadFileUsingFormData() method");

			if (token == config.Value.APIToken)
			{
				try
				{
					string salesforceToken = await ForwardSignsHandler.GetForwardSignsAccessToken(config.Value);

					_token = await GetAccessToken(config.Value);

					int yearID = await ExistWorkOrderName(config.Value.WorkOrderID, config.Value, _token, year);
					int projectCreatorID = await ExistWorkOrderName(yearID, config.Value, _token, projectCreator);
					int projectNameID = await ExistWorkOrderName(projectCreatorID, config.Value, _token, projectName);
					int workOrderNumberID = await ExistWorkOrderName(projectNameID, config.Value, _token, workOrderNumber);
					int itemID = await ExistWorkOrderName(workOrderNumberID, config.Value, _token, itemName);

					//a file is in either item or workorder folders
					if (itemID == 0)
                    {
						fileID = await CheckFile(workOrderNumberID, config.Value, _token, filename);
						if (fileID != 0)
						{
							bool result = await DeleteFile(config.Value, _token, fileID);
						}
					}
					else
                    {
						fileID = await CheckFile(itemID, config.Value, _token, filename);
						if (fileID != 0)
						{
							bool result = await DeleteFile(config.Value, _token, fileID);
						}
					}
					return Ok("Success");
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
