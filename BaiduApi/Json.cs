extern alias global2;

using System;
using System.Collections.Generic;
using System.Text;
using global2::Newtonsoft.Json;

namespace BaiduApi
{
	[Serializable]
	class ErrInfo
	{
		[JsonProperty("no")]
		public string no { get; set; }
	}
	
	[Serializable]
	class Data
	{ 
		[JsonProperty("codeString")]
		public string CodeString { get; set; }

		[JsonProperty("rememberedUserName")]
		public string RememberedUserName { get; set; }

		[JsonProperty("token")]
		public string Token { get; set; }
	}

	[Serializable]
	class ResultData
	{
		[JsonProperty("errInfo")]
		public ErrInfo ErrInfo { get; set; }
		
		[JsonProperty("data")]
		public Data Data { get; set; }

	}
}
