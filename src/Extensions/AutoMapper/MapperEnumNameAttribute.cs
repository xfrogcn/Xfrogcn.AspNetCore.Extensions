using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MapperEnumNameAttribute : Attribute
    { 
        public MapperEnumNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
