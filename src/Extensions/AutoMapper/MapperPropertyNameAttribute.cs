using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class MapperPropertyNameAttribute : Attribute
    {
        public string Name { get; set; }

        public Type TargetType { get; set; }

        public Type SourceType { get; set; }
    }
}
