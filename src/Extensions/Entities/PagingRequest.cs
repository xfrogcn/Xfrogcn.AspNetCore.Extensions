using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class PagingRequest
    {
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }


        public int Skip
        {
            get
            {
                return (int)((PageIndex - 1) * PageSize);
            }
        }

        public void SetToResponse<T>(PagingResponseMessage<T> r)
        {
            r.PageIndex = PageIndex;
            r.PageSize = PageSize;
        }
    }
}
