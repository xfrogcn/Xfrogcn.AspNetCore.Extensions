using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class TestLoggerProviderLogBuilderExtensions
    {
        public static ILoggingBuilder AddTestLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddTestLoggerProvider();
            return loggingBuilder;
        }
    }
}
