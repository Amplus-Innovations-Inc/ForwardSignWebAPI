using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ForwardSignWebAPI.Models
{
	public class ForwardSignsHandler
	{
		public static async Task<String> GetForwardSignsAccessToken(MyConfig config)
		{
			string token = "";
			string token_type = "";
			try
			{
				var url = config.SalesforceTokenURL;
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
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			return token_type + " " + token;
		}

		public static async Task<byte[]> DownloadFile(MyConfig config, string forwardSignsToken, string fileID)
		{
			byte[] formFiles = new byte[0];
			try
			{
				var url = config.SalesforceFileReq;
				url = url.Replace("{documentID}", fileID);
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add("authorization", forwardSignsToken); //if any
					var response = await client.GetAsync(url);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						var contentStream = response.Content.ReadAsStreamAsync().Result;
						var memoryStream = new MemoryStream();
						contentStream.CopyTo(memoryStream);
						try
						{
							formFiles = memoryStream.ToArray();
						}
						catch (Exception e)
						{
							memoryStream.Dispose();
							throw;
						}
						finally
						{
							memoryStream.Dispose();
						}
					}
					else
					{
						throw new Exception("Cannot download a file from salesforce");
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			return formFiles;
		}
	}
}
