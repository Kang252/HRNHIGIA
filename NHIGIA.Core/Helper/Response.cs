using System.Collections.Generic;

namespace NHIGIA.Core.Helper
{
    public class Response
    {
        public Response(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }

    }

    public class Response<T> : Response
    {
        public Response(bool success, string message, T data) : base(success, message)
        {
            Data = data;
        }
        public T Data { get; set; }
    }

    public class ResponseList<T> : Response
    {
        public ResponseList(bool success, string message, List<T> data) : base(success, message)
        {
            Data = data;
        }
        public ResponseList(bool success, string message, List<T> data, int total) : this(success, message, data)
        {
            Data = data;
            Total = total;
        }

        public List<T> Data { get; set; }
        public int Total { get; set; }
        public int Count { get; }
    }
}
