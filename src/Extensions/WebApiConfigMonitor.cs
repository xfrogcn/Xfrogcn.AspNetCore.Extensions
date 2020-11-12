using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class WebApiConfigMonitor
    {
        readonly IOptionsMonitor<WebApiConfig> _monitor;
        readonly IServiceProvider _serviceProvider;
        public WebApiConfigMonitor(IOptionsMonitor<WebApiConfig> monitor, IServiceProvider serviceProvider)
        {
            _monitor = monitor;
            _serviceProvider = serviceProvider;
            monitor.OnChange(onConfigChanged);
        }

        public void Init()
        {

        }

        private void onConfigChanged(WebApiConfig config, string name)
        {
            WebApiConfig old = _serviceProvider.GetRequiredService<WebApiConfig>();
            if(old!= config)
            {
                IMapperProvider mapper = _serviceProvider.GetRequiredService<IMapperProvider>();
                mapper.CopyTo<WebApiConfig, WebApiConfig>(config, old);
            }
        }
    }
}
