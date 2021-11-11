using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ForwardSignWebAPI.Models
{
	public class SyncedToolHandler
	{
		public static async Task<String> GetAccessToken(MyConfig config)
		{
			string token = "";
			string token_type = "";
			try
			{
				var url = String.Format(config.SyncedToolURL + "oauth/token?grant_type=password&client_id=anchor&username={0}&password={1}",
									HttpUtility.UrlEncode(config.UserName), HttpUtility.UrlEncode(config.Password));

				using (var client = new HttpClient())
				{
					var response = await client.PostAsync(url, null);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						string result = response.Content.ReadAsStringAsync().Result;
						var objects = JObject.Parse(result); // parse as array
						token = objects["access_token"].ToString();
						token_type = objects["token_type"].ToString();
					}
					else
					{
						throw new Exception("Cannot request a token");
					}

				}
					
			}
			catch(Exception e)
			{
				throw new Exception(e.Message);
			}
			return token_type + " " + token;
		}

		public static async Task<Int32> ExistWorkOrderName(int parentID, MyConfig config, string token, string workOrderName)
		{
			int ret = 0;
			try
			{
				var url = String.Format(config.SyncedToolURL + "api/2/files/{0}/folder/{1}/children?include_deleted=false", config.RootSalesforceFilesID, parentID);

				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("Authorization", token);
					var response = await client.GetAsync(url);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						string result = response.Content.ReadAsStringAsync().Result;
						var objects = JObject.Parse(result);
						if(objects != null)
						{
							foreach (JObject item in objects["results"]) // <-- Note that here we used JObject instead of usual JProperty
							{
								string name = item["name"].ToString();
								if (name.Equals(workOrderName, StringComparison.CurrentCultureIgnoreCase))
								{
									ret = int.Parse(item["id"].ToString());
									break;
								}

							}
						}
						else
						{
							throw new Exception("Wrong data format, please contact with administrator");
						}
					} 
					else
					{
						throw new Exception("Cannot read a list of sub-folder");
					}
				}

			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			return ret;
		}

		public static async Task<Int32> CreateFolder(int parentID, MyConfig config, string token, string workOrderName)
		{
			int ret = 0;
			try
			{
				var url = String.Format(config.SyncedToolURL + "api/2/files/{0}/folder/{1}/create_folder", config.RootSalesforceFilesID, parentID);
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("Authorization", token);
					var formContent = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("name", workOrderName),
					});

					var response = await client.PostAsync(url, formContent);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						string result = response.Content.ReadAsStringAsync().Result;
						var objects = JObject.Parse(result); // parse as array
						ret = int.Parse(objects["id"].ToString());
					}
					else
					{
						throw new Exception("Failed to create a sub-folder");
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			return ret;
		}

		public static async Task<Boolean> UploadAFile(MyConfig config, string token, int id, byte[] fileBytes, string file, string fileName)
		{
			bool ret = false;
			try
			{
                var url = String.Format(config.SyncedToolURL + "api/2/files/{0}/folder/{1}/upload", config.RootSalesforceFilesID, id);

                using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("Authorization", token);
					var multipartContent = new MultipartFormDataContent();
					multipartContent.Add(new ByteArrayContent(fileBytes), file, fileName);
					var response = await client.PostAsync(url, multipartContent);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						ret = true;
					}
					else
					{
						throw new Exception("Failed to create a sub-folder");
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			return ret;
		}
	}
}
