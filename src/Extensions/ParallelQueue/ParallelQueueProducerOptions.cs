﻿using System;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class ParallelQueueProducerOptions<TEntity>
    {
   
        Func<IServiceProvider, string, IParallelQueueProducer<TEntity>> _creator;
        public void SetProducer(Func<IServiceProvider, string, IParallelQueueProducer<TEntity>> creator)
        {
            _creator = creator;
        }

        internal Func<IServiceProvider, string, IParallelQueueProducer<TEntity>> GetCreator()
        {
            return _creator;
        }
    }
}
