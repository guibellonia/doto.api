namespace Doto.Application.DTOs.Responses
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public BaseResponse() 
        {
            Code = 200;
        }

        public BaseResponse(bool success = true, string? message = null, T? data = default, int code = 200)
        {
            Success = success;
            Code = code;
            Message = message;
            Data = data;
        }

        public static BaseResponse<T> Ok(string? message = null, T? data = default)
        {
            return new (true, message, data);
        }

        public static BaseResponse<T> Fail(string? message = null, T? data = default, int code = 400)
        {
            return new(false, message, data, code);
        }
    }
}
