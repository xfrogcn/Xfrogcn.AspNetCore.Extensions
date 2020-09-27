using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class ParallelQueueProducerOptions<TEntity>
    {
        private readonly Dictionary<string, Func<string, IParallelQueueProducer<TEntity>>> creatorMapper
            = new Dictionary<string, Func<string, IParallelQueueProducer<TEntity>>>();

        public void AddProducer(string name, Func<string, IParallelQueueProducer<TEntity>> creator)
        {
            lock (creatorMapper)
            {
                if (creatorMapper.ContainsKey(name))
                {
                    creatorMapper[name] = creator;
                }
                else
                {
                    creatorMapper.Add(name, creator);
                }
            }
        }

        internal Func<string, IParallelQueueProducer<TEntity>> GetCreator(string name)
        {
            if(creatorMapper.ContainsKey(name))
            {
                return creatorMapper[name];
            }
            return null;
        }
    }
}
