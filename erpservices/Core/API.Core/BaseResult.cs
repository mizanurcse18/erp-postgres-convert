using System;
using System.Runtime.CompilerServices;

namespace API.Core
{
	public class BaseResult
	{
		public string Message
		{
			get;
			set;
		}

		public string ResponseCode
		{
			get;
			set;
		}

		public int StatusCode
		{
			get;
			set;
		}

		public BaseResult()
		{
		}
	}
}