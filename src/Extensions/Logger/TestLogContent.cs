using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class TestLogContent
    {
        private readonly List<TestLogItem> _logList = new List<TestLogItem>();


        public IReadOnlyList<TestLogItem> LogContents => _logList;

        internal void AddLogItem(TestLogItem logItem)
        {
            _logList.Add(logItem);
        }
    }

    public class TestLogItem
    {
        public string CategoryName { get; set; }
        public string Message { get; set; }

        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }

        public List<object> ScopeValues { get; set; }
    }
}
