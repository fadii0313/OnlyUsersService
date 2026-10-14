using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Common
{
    [ApiController]
    public class CustomBaseController : ControllerBase
    {
        /// <summary>
        /// Maps standard ApiResponseCode to RFC compliant HTTP Status Code responses.
        /// </summary>
        protected IActionResult HandleResult(BaseResponseModel response)
        {
            if (response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel
                {
                    ResponseCode = ApiResponseCode.Exception,
                    ResponseMessage = "Something went wrong, please try again."
                });
            }

            if (response.ResponseCode == ApiResponseCode.Validation)
            {
                return BadRequest(response); // HTTP 400
            }

            if (response.ResponseCode == ApiResponseCode.Unauthorized)
            {
                return Unauthorized(response); // HTTP 401
            }

            if (response.ResponseCode == ApiResponseCode.Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, response); // HTTP 403
            }

            if (response.ResponseCode == ApiResponseCode.Fail)
            {
                if (response.ResponseMessage != null &&
                    (response.ResponseMessage.Contains("Failed to create", StringComparison.OrdinalIgnoreCase) ||
                     response.ResponseMessage.Contains("Failed to update", StringComparison.OrdinalIgnoreCase) ||
                     response.ResponseMessage.Contains("Database", StringComparison.OrdinalIgnoreCase)))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, response); // HTTP 500
                }

                if (response.ResponseMessage != null &&
                    (response.ResponseMessage.Contains("Invalid Username or Password", StringComparison.OrdinalIgnoreCase) ||
                     response.ResponseMessage.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
                     response.ResponseMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)))
                {
                    return Unauthorized(response); // HTTP 401
                }

                return NotFound(response); // HTTP 404
            }

            if (response.ResponseCode == ApiResponseCode.Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response); // HTTP 500
            }

            return Ok(response); // HTTP 200
        }
    }
}
