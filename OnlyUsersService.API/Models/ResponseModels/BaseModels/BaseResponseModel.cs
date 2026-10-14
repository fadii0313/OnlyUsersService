using System;
using System.Collections.Generic;

namespace OnlyUsersService.API.Models.ResponseModels.BaseModels
{
    public class BaseResponseModel
    {
        public object Data { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public bool Success { get; set; }
        public List<ErrorModel> Errors { get; set; }
        public string ResponseExecutionTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public BaseResponseModel()
        {
            Data = null;
            ResponseCode = ApiResponseCode.Default;
            ResponseMessage = string.Empty;
            Errors = new List<ErrorModel>();
            Success = false;
            StartDateTime = DateTime.UtcNow;
        }

        public BaseResponseModel(bool isSuccess)
        {
            StartDateTime = DateTime.UtcNow;
            if (isSuccess)
                SuccessModel();
            else
                FailedModel();
        }

        public void SuccessModel(object data = null, string message = "Operation completed successfully.")
        {
            Data = data;
            ResponseCode = ApiResponseCode.Success;
            ResponseMessage = message;
            Errors = new List<ErrorModel>();
            Success = true;
            EndDateTime = DateTime.UtcNow;
        }

        public void FailedModel(string message = "Operation failed.", string errorCode = null)
        {
            Data = null;
            ResponseCode = ApiResponseCode.Fail;
            ResponseMessage = message;
            Errors = new List<ErrorModel>();
            if (!string.IsNullOrEmpty(errorCode))
            {
                AddError(message, errorCode);
            }
            Success = false;
            EndDateTime = DateTime.UtcNow;
        }

        public void ValidationModel(string message = "Validation error.")
        {
            Data = null;
            ResponseCode = ApiResponseCode.Validation;
            ResponseMessage = message;
            Errors = new List<ErrorModel>();
            Success = false;
            EndDateTime = DateTime.UtcNow;
        }

        public void UnauthorizedModel(string message = "Unauthorized access.")
        {
            Data = null;
            ResponseCode = ApiResponseCode.Unauthorized;
            ResponseMessage = message;
            Errors = new List<ErrorModel>();
            Success = false;
            EndDateTime = DateTime.UtcNow;
        }

        public void ForbiddenModel(string message = "Access denied. Insufficient permissions.")
        {
            Data = null;
            ResponseCode = ApiResponseCode.Forbidden;
            ResponseMessage = message;
            Errors = new List<ErrorModel>();
            Success = false;
            EndDateTime = DateTime.UtcNow;
        }

        public void ExceptionModel(Exception exception = null)
        {
            Data = null;
            ResponseCode = ApiResponseCode.Exception;
            ResponseMessage = "An unexpected error occurred. Please try again later.";
            Errors = new List<ErrorModel>();
            if (exception != null)
            {
                AddError(exception.Message);
            }
            Success = false;
            EndDateTime = DateTime.UtcNow;
        }

        public void AddError(string errorMessage, string errorCode = null)
        {
            Errors.Add(new ErrorModel
            {
                ErrorCode = errorCode ?? (Errors.Count + 1).ToString(),
                ErrorMessage = errorMessage
            });
        }
    }
}
