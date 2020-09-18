using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 应答消息
    /// </summary>
    public class ResponseMessage
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ResponseMessage()
        {
            Code = "0";
        }

        public bool IsSuccess => Code == "0";
    }

    public class ResponseMessage<TData> : ResponseMessage
    {
        [JsonPropertyName("extension")]
        public TData Extension { get; set; }
    }

    public class PagingResponseMessage<TDataItem>: ResponseMessage<List<TDataItem>>
    {
        [JsonPropertyName("pageSize")]
        public long PageSize { get; set; }

        [JsonPropertyName("pageIndex")]
        public long PageIndex { get; set; }

        [JsonPropertyName("total")]
        public long Total { get; set; }
    }
}
