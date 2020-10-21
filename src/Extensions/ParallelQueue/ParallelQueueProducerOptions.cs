using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
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
