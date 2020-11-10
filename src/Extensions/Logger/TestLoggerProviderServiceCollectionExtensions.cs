using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TestLoggerProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddTestLoggerProvider(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddLogging();
            serviceDescriptors.TryAddSingleton<TestLogContent>();
            serviceDescriptors.AddSingleton<ILoggerProvider, TestLoggerProvider>();
            return serviceDescriptors;
        }
        public static TestLogContent GetTestLogContent(this IServiceProvider provider)
        {
            return provider.GetRequiredService<TestLogContent>();
        }
    }
}
