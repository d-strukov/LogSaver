using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LogTest
{
    internal class WAF :WebApplicationFactory<Program>
    {
        private readonly Action<IServiceCollection> _services;

        public WAF(Action<IServiceCollection> services)
        {
            _services = services;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(_services);
            return base.CreateHost(builder);
        }


    }
}
