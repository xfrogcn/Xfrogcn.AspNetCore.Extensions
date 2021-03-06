﻿using System;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryOptionsExtensions
    {
        public static ParallelQueueProducerOptions<TEntity> UseMemory<TEntity>(
           this ParallelQueueProducerOptions<TEntity> options)
        {
            options.SetProducer((sp, name) =>
            {
                return new MemoryQueueProducer<TEntity>();
            });
            return options;
        }
    }
}
