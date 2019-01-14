using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLSchemaSync.Test
{
    /// <summary>
    /// 单元测试基类
    /// </summary>
    public abstract class TestSuit : IDisposable
    {
        protected IServiceScope scope = null;

        public TestSuit()
        {
            Bootstrapper.Initialise();
            scope = Bootstrapper.GetScope();
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
