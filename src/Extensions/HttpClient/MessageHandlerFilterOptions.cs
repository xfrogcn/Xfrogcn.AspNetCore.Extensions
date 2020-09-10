using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class MessageHandlerFilterOptions
    {
        public bool EnableLoggingDetial { get; set; } = true;
        public bool EnableTransHttpContextHeaders { get; set; } = false;
    }
}
