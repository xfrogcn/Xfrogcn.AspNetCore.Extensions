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


        public void Normalize(int maxSize = 1000, int defaultSize = 20)
        {
            if(PageSize<0 || PageSize>maxSize)
            {
                PageSize = defaultSize;
            }
            if (PageIndex <= 0)
            {
                PageIndex = 1;
            }
        }


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
