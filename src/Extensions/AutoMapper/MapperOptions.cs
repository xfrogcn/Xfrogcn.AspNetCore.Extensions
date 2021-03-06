﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class MapperOptions
    {
        public class MapperItem
        {
            public Type SourceType { get; set; }

            public Type TargetType { get; set; }

        }

        public class MapperItem<TSource, TTarget> : MapperItem
        {
            public Action<IMapperProvider, TSource,TTarget> Convert { get; set; }
        }

        private readonly List<MapperItem> _mapperList = new List<MapperItem>();

        public IReadOnlyList<MapperItem> MapperList => _mapperList;

        public void AddConvert<TSource,TTarget>(Action<IMapperProvider, TSource,TTarget> converter)
        {
            if (converter != null)
            {
                _mapperList.Add(new MapperItem<TSource, TTarget>()
                {
                    SourceType = typeof(TSource),
                    TargetType = typeof(TTarget),
                    Convert = converter
                });
            }
        }

        public IReadOnlyList<MapperItem> GetConverter<TSource, TTarget>()
        {
            var sType = typeof(TSource);
            var tType = typeof(TTarget);
            // 能赋给TSource的，并且
            var list = _mapperList.Where(m => m.SourceType.IsAssignableFrom(sType)
                            && m.TargetType.IsAssignableFrom(tType)).ToList();
            return list;
        }
    }
}
