using Core.AppContexts;
using Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Core
{
    [ApiPathValidationAttribute]
    public class BaseController : ControllerBase
	{
		protected ObjectResult Success()
		{
			return Ok(new { Success = true });
		}

        protected ObjectResult Success(string status,string msg,dynamic obj)
        {
            return Ok(new { status, msg,data = obj});
        }
        protected ObjectResult Success(string status, string msg)
        {
            return Ok(new { status, msg });
        }

        protected ObjectResult Success(string msg)
		{
			return Ok(new { Success = true, message = msg });
		}

		protected ObjectResult Unsuccess()
		{
			return Ok(new { Success = false });
		}

		protected ObjectResult Unsuccess(string msg)
		{
			return Ok(new { Success = false, message = msg });
		}

		protected ObjectResult Error()
		{
			return Error(string.Empty);
		}

		protected ObjectResult Error(string msg)
		{
			return Ok(new { Error = true, msg });
		}

        protected ObjectResult Error(string status, string msg)
        {
            return Ok(new { status, msg });
        }
        protected ObjectResult BadRequestResult(string message = "")
		{
			return CreateResult(400, "BAD_REQUEST", message, null);
		}

		protected ObjectResult ConnectErrorResult(string message = "")
		{
			return CreateResult(502, "CONNECT_ERROR", message, null);
		}

		protected ObjectResult ConnectTimeoutResult(string message = "")
		{
			return CreateResult(504, "CONNECT_TIMEOUT", message, null);
		}

		protected ObjectResult CreatedResult(object result = null)
		{
			return CreateResult(201, "CREATED", "Record created successfully.", result);
		}

		private ObjectResult CreateResult(int statusCode, string responseCode, string message = "", object result = null)
		{
			if (result.IsNull())
			{
				return new ObjectResult(new BaseResult()
				{
					StatusCode = statusCode,
					ResponseCode = responseCode,
					Message = message
				});
			}
			return new ObjectResult(new OkResult()
			{
				StatusCode = statusCode,
				ResponseCode = responseCode,
				Result = result,
				Message = message
			});
		}

		protected ObjectResult DeletedResult()
		{
			return CreateResult(204, "DELETED", "Record deleted successfully.", null);
		}

		protected ObjectResult FailureResult(string message = "")
		{
			return CreateResult(502, "FAILURE", message, null);
		}

		protected ObjectResult NoContentResult(string message = "OK")
		{
			return CreateResult(204, "NO_CONTENT", message, null);
		}

		protected ObjectResult NotFoundResult(string message = "")
		{
			return CreateResult(404, "NOT_FOUND", message, null);
		}

		protected ObjectResult OkResult(object result = null)
		{
			return CreateResult(200, "OK", "Success", result);
		}

		protected ObjectResult DirectOkResult(object result = null)
		{
			return new ObjectResult(new OkResult()
			{
				Result = result
			});
		}

		protected ObjectResult ServerErrorResult(string message = "")
		{
			return CreateResult(500, "INTERNAL_ERROR", message, null);
		}

		protected ObjectResult ServiceUnavailableResult(string message = "")
		{
			return CreateResult(503, "SERVICE_UNAVAILABLE", message, null);
		}

		protected ObjectResult UnauthorizedResult(string message = "")
		{
			return CreateResult(401, "UNAUTHORIZED", message, null);
		}

		protected ObjectResult UpdatedResult(object result = null)
		{
			return CreateResult(205, "UPDATED", "Record updated successfully.", result);
		}

		protected ObjectResult ValidationResult(string message = "")
		{
			return CreateResult(422, "VALIDATION", message, null);
		}

		protected bool HasViewPrivilege()
		{
			return AppContexts.User.CanRead;
		}
	}
}
