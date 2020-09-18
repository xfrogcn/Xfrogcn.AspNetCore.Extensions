using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class PagingRequest
    {
        [JsonPropertyName("pageSize")]
        public long PageSize { get; set; }

        [JsonPropertyName("pageIndex")]
        public long PageIndex { get; set; }
    }
}
