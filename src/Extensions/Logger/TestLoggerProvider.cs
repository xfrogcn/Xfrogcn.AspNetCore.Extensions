using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 测试日志提供器
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        class TestLogger : ILogger
        {
            internal IExternalScopeProvider ScopeProvider { get; set; }
            protected readonly TestLogContent _logContent;
            protected readonly string _categoryName;

            public TestLogger(string categoryName, TestLogContent logContent)
            {
                _categoryName = categoryName;
                _logContent = logContent;
            }
            public IDisposable BeginScope<TState>(TState state)
            {
                return ScopeProvider?.Push(state);
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel != LogLevel.None;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                TestLogItem item = new TestLogItem()
                {
                    CategoryName = _categoryName,
                    EventId = eventId,
                    LogLevel = logLevel,
                    Message = formatter(state, exception),
                    ScopeValues = new List<object>()
                };

                ScopeProvider?.ForEachScope<TestLogItem>((obj, state) =>
                {
                    item.ScopeValues.Add(obj);
                }, item);
                _logContent.AddLogItem(item);
            }
        }

        readonly ConcurrentDictionary<string, TestLogger> _loggers;
        readonly TestLogContent _logContent;
        IExternalScopeProvider _scopeProvider = null;

        public TestLoggerProvider(TestLogContent logContent)
        {
            _loggers = new ConcurrentDictionary<string, TestLogger>();
            _logContent = logContent;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, key => new TestLogger(key, _logContent)
            {
                ScopeProvider = _scopeProvider
            });
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }

        public void Dispose()
        {
           
        }
    }
}
