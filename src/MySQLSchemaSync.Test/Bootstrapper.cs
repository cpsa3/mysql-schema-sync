using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xiaobao.Framework.DependencyInjection;

namespace MySQLSchemaSync.Test
{
    public class Bootstrapper
    {
        private static IServiceProvider provider = null;
        private static IConfiguration configuration;

        static object locker = new object();
        public static void Initialise()
        {
            if (provider == null)
            {
                lock (locker)
                {
                    if (provider == null)
                    {
                        var configBuilder = new ConfigurationBuilder();
                        configBuilder.AddJsonFile("appsettings.json");
                        configuration = configBuilder.Build();

                        provider = CreateServiceProvider(services =>
                        {
                            services.AddScoped<IDbConnection>(p => new MySql.Data.MySqlClient.MySqlConnection(configuration["connectionStrings:db1"]));
                        });
                    }
                }
            }
        }

        private static IServiceProvider CreateServiceProvider(Action<IServiceCollection> action, Action<ContainerBuilder> configContainerBuilder = null)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddXiaobaoServiceProvider();

            action?.Invoke(services);

            var r = services.BuildServiceProvider();
            var factory = r.GetService<IServiceProviderFactory<ContainerBuilder>>();
            var containerBuilder = factory.CreateBuilder(services);

            configContainerBuilder?.Invoke(containerBuilder);
            var provider = factory.CreateServiceProvider(containerBuilder);
            return provider;
        }

        public static T Resolve<T>()
        {
            return provider.GetService<T>();
        }

        public static IServiceScope GetScope()
        {
            return provider.GetService<IServiceScopeFactory>().CreateScope();
        }

        public static IConfiguration GetConfiguration()
        {
            return configuration;
        }
    }
}
