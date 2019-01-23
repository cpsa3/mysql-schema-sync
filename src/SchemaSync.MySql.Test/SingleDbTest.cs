using SchemaSync.MySql.Meta;
using SchemaSync.MySql.Util;
using System;
using System.Linq;
using Xunit;

namespace SchemaSync.MySql.Test
{
    /// <summary>
    /// 相同数据库之间同步
    /// </summary>
    public class SingleDbTest : TestSuit
    {
        private string connectionString1;

        public SingleDbTest() : base()
        {
            connectionString1 = Bootstrapper.GetConfiguration()["connectionStrings:db1"];
        }

        [Fact]
        public void MetaDataTest()
        {
            var metaData = new MetaData(connectionString1);

            metaData.Init();

            Assert.Equal("demo", metaData.Schema);

            Assert.True(metaData.Tables.Count >= 2);

            Assert.True(metaData.Tables.ContainsKey("event_received_template"));
            Assert.True(metaData.Tables.ContainsKey("event_received_local"));

            Assert.True(metaData.Tables["event_received_template"].Columns.ContainsKey("NextRetryTime"));
            Assert.True(metaData.Tables["event_received_template"].Indexes.ContainsKey("idx_statusname_retry"));

            Assert.Equal(2, metaData.Tables["event_received_template"].Indexes["idx_statusname_retry"].Columns.Count);
            Assert.Equal("CURRENT_TIMESTAMP", metaData.Tables["event_received_template"].Columns["NextRetryTime"].DefaultValue);
            Assert.Equal("datetime", metaData.Tables["event_received_template"].Columns["NextRetryTime"].Type);
        }

        /// <summary>
        /// 新增column并且apply
        /// </summary>
        [Fact]
        public void CompareSingleTable_AddColumn_Apply_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData);

            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.True(compareUnits.ChangeSql.Count == 1);
            Assert.Contains("alter table demo.event_received_local add COLUMN Remark varchar(45) NOT NULL COMMENT '备注' after db_updated_at;", compareUnits.ChangeSql);

            compareUnits.ApplyChanges();
            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.Empty(compareUnits.ChangeSql);
        }

        /// <summary>
        /// 开启IsDropRedundancy
        /// 开启IsSyncIndex
        /// apply
        /// </summary>
        [Fact]
        public void CompareSingleTable_DropColumnAndIndex_Apply_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsSyncIndex = true, IsDropRedundancy = true });

            compareUnits.Compare("event_received_local", "event_received_template");
            Assert.True(compareUnits.ChangeSql.Count == 2);

            Assert.Contains("alter table demo.event_received_template drop index idx_statusname_retry ;", compareUnits.ChangeSql);

            compareUnits.ApplyChanges();
            compareUnits.Compare("event_received_local", "event_received_template");
            Assert.Empty(compareUnits.ChangeSql);
        }

        /// <summary>
        /// 对比相同的表
        /// </summary>
        [Fact]
        public void CompareSingleTable_Same_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData);

            compareUnits.Compare("event_received_local", "event_received_local");
            Assert.False(compareUnits.ChangeSql.Any());
        }

        /// <summary>
        /// 新增column（默认不同步索引）
        /// </summary>
        [Fact]
        public void CompareSingleTable_AddColumn_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData);

            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.True(compareUnits.ChangeSql.Count == 1);
            Assert.Contains("alter table demo.event_received_local add COLUMN Remark varchar(45) NOT NULL COMMENT '备注' after db_updated_at;", compareUnits.ChangeSql);
        }

        /// <summary>
        /// 新增column并同步索引
        /// </summary>
        [Fact]
        public void CompareSingleTable_AddColumnAndIndex_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsSyncIndex = true });

            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.True(compareUnits.ChangeSql.Count == 2);
            Assert.Contains("alter table demo.event_received_local add index idx_statusname_retry (`StatusName`,`Retry`);", compareUnits.ChangeSql);
        }

        /// <summary>
        /// 不开启IsDropRedundancy配置的情况
        /// </summary>
        [Fact]
        public void CompareSingleTable_NotDropRedundancy_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsDropRedundancy = false });

            compareUnits.Compare("event_received_local", "event_received_template");
            Assert.True(compareUnits.ChangeSql.Count == 0);
        }

        /// <summary>
        /// 开启IsDropRedundancy
        /// </summary>
        [Fact]
        public void CompareSingleTable_DropColumn_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsDropRedundancy = true });

            compareUnits.Compare("event_received_local", "event_received_template");
            Assert.True(compareUnits.ChangeSql.Count == 1);
            Assert.Contains("alter table demo.event_received_template drop COLUMN Remark ;", compareUnits.ChangeSql);
        }

        /// <summary>
        /// 开启IsDropRedundancy
        /// 开启IsSyncIndex
        /// </summary>
        [Fact]
        public void CompareSingleTable_DropColumnAndIndex_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsSyncIndex = true, IsDropRedundancy = true });

            compareUnits.Compare("event_received_local", "event_received_template");
            Assert.True(compareUnits.ChangeSql.Count == 2);

            Assert.Contains("alter table demo.event_received_template drop index idx_statusname_retry ;", compareUnits.ChangeSql);
        }
    }
}
