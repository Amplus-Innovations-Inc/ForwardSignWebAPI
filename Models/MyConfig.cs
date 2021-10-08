using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForwardSignWebAPI.Models
{
	public class MyConfig
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SyncedToolURL { get; set; }
		public string RootTeamShare { get; set; }
		public string APIToken { get; set; }
		public int RootSalesforceFilesID { get; set; }
		public int WorkOrderID { get; set; }
		public string SalesforceTokenURL { get; set; }
		public string SalesforceFileReq { get; set; }
	}
}
