using System;
using System.Collections.Generic;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Linq;
using Serilog;

namespace Xfrogcn.AspNetCore.Extensions.Logger
{
    public delegate bool KeySelector<TKey>(LogEvent logEvent, out TKey key);

    
    sealed class MappedSink<TKey> : ILogEventSink, IDisposable
    {
        readonly KeySelector<TKey> _keySelector;
        readonly Action<TKey, LoggerSinkConfiguration> _configure;
        readonly Func<TKey, bool> _disposeCondition;
        readonly TimeSpan _checkInterval;
        readonly int? _sinkMapCountLimit;
        readonly ConcurrentDictionary<TKey, ILogEventSink> _sinkMap;
        bool _disposed;
        DateTimeOffset _lastCheckTime = DateTimeOffset.MinValue;
        readonly object _locker = new object();
        bool _isChecking = false;

        public MappedSink(KeySelector<TKey> keySelector,
                          Action<TKey, LoggerSinkConfiguration> configure,
                          Func<TKey,bool> disposeCondition,
                          TimeSpan checkInterval,
                          int? sinkMapCountLimit)
        {
            _keySelector = keySelector;
            _checkInterval = checkInterval;
            _disposeCondition = disposeCondition;
            _configure = configure;
            _sinkMapCountLimit = sinkMapCountLimit;

            Type keyType = typeof(TKey);

            if (typeof(IEqualityComparer<TKey>).IsAssignableFrom(typeof(TKey)) &&
                keyType.GetConstructor(new Type[] { }) != null)
            {

                _sinkMap = new ConcurrentDictionary<TKey, ILogEventSink>(
                    Activator.CreateInstance<TKey>() as IEqualityComparer<TKey>);
            }
            else
            {
                _sinkMap = new ConcurrentDictionary<TKey, ILogEventSink>();
            }
        }

        

        public void Emit(LogEvent logEvent)
        {
            if (!_keySelector(logEvent, out var key))
                return;

            if (_disposed)
                throw new ObjectDisposedException(nameof(MappedSink<TKey>), "The mapped sink has been disposed.");

            ILogEventSink sink;
            if (_sinkMapCountLimit == 0)
            {
                sink = CreateSink(key);
                using (sink as IDisposable)
                {
                    sink.Emit(logEvent);
                }
            }
            else
            {
                sink = _sinkMap.GetOrAdd(key, (_key) =>
                {
                    return CreateSink(_key);
                });

                sink.Emit(logEvent);

            }

            bool mustCheck = false;
            var now = DateTimeOffset.UtcNow;
            if (!_isChecking && (_checkInterval.TotalMilliseconds == 0 ||
                (_checkInterval.TotalMilliseconds > 0 && (now - _lastCheckTime) >= _checkInterval)))
            {
                lock (_locker)
                {
                    if (!_isChecking)
                    {
                        _isChecking = true;
                        _lastCheckTime = now;
                        mustCheck = true;
                    }
                }
            }

            if (!mustCheck)
            {
                return;
            }

            // 先移除超出数量限制的sink
            if (_sinkMapCountLimit.HasValue && _sinkMapCountLimit.Value > 0)
            {
                while (_sinkMap.Count > _sinkMapCountLimit.Value)
                {
                    foreach (var k in _sinkMap.Keys)
                    {
                        if (key.Equals(k))
                            continue;

                        _sinkMap.Remove(k, out ILogEventSink removed);
                        (removed as IDisposable)?.Dispose();
                        break;
                    }
                }
            }

            if (_disposeCondition != null)
            {
                List<TKey> removeKeys = new List<TKey>();
                foreach (var kv in _sinkMap)
                {
                    if(_disposeCondition(kv.Key))
                    {
                        removeKeys.Add(kv.Key);
                    }
                }

                foreach(var k in removeKeys)
                {
                    if (_sinkMap.TryRemove(k, out ILogEventSink removed))
                    {
                        (removed as IDisposable)?.Dispose();
                    }
                }
            }
            

            lock (_locker)
            {
                _isChecking = false;
            }

            
            
        }


        ILogEventSink CreateSink(TKey key)
        {
            // Allocates a few delegates, but avoids a lot more allocation in the `LoggerConfiguration`/`Logger` machinery.
            ILogEventSink sink = null;
            LoggerSinkConfiguration.Wrap(
                new LoggerConfiguration().WriteTo,
                s => sink = s,
                config => _configure(key, config),
                LevelAlias.Minimum,
                null);

            return sink;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            var values = _sinkMap.Values.ToArray();
            _sinkMap.Clear();
            foreach (var sink in values)
            {
                (sink as IDisposable)?.Dispose();
            }
        }
    }
}
