using System;
using System.Runtime.CompilerServices;

namespace API.Core
{
	public class OkResult : BaseResult
	{
		public object Result
		{
			get;
			set;
		}

		public OkResult()
		{
		}
	}
}