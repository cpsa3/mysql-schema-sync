using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySQLSchemaSync.Test
{
    /// <summary>
    /// 单元测试基类
    /// </summary>
    public abstract class TestSuit : IDisposable
    {
        protected IServiceScope scope = null;
        protected IDbConnection connection;

        public TestSuit()
        {
            Bootstrapper.Initialise();
            scope = Bootstrapper.GetScope();

            this.connection = scope.ServiceProvider.GetService<IDbConnection>();

            Init();
        }

        public void Dispose()
        {
            scope.Dispose();
        }

        void Init()
        {
            var fileInfo = new System.IO.FileInfo("InitTestSql/init.sql");
            var script = fileInfo.OpenText().ReadToEnd();
            connection.Execute(script);
        }
    }
}
