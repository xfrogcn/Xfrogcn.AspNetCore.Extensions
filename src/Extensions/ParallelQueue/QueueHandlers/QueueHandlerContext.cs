using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class QueueHandlerContext<TEntity, TState>
    {
        public TEntity Message { get; internal set; }

        public TState State { get; internal set; }

        public string QueueName { get; internal set; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public bool Stoped { get; internal set; } = false;

        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, object> Parameters => _parameters;

        public void SetParameter(string name, object val)
        {
            if (_parameters.ContainsKey(name))
            {
                _parameters[name] = val;
            }
            else
            {
                _parameters.Add(name, val);
            }
        }

        public void RemoveParameter(string name)
        {
            if (_parameters.ContainsKey(name))
            {
                _parameters.Remove(name);
            }
        }

        public object GetParameter(string name)
        {
            if (_parameters.ContainsKey(name))
            {
                return _parameters[name];
            }
            return default;
        }

        public void ClearParamater()
        {
            _parameters.Clear();
        }

        public TValue GetParameter<TValue>(string name)
        {
            return (TValue)GetParameter(name);
        }

        public void Stop()
        {
            Stoped = true;
        }
    }
}
