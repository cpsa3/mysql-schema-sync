using SchemaSync.MySql.Meta;
using SchemaSync.MySql.Util;
using System;
using System.Linq;
using Xunit;

namespace SchemaSync.MySql.Test
{
    /// <summary>
    /// ��ͬ���ݿ�֮��ͬ��
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
        /// ����column����apply
        /// </summary>
        [Fact]
        public void CompareSingleTable_AddColumn_Apply_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData);

            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.True(compareUnits.ChangeSql.Count == 1);
            Assert.Contains("alter table demo.event_received_local add COLUMN Remark varchar(45) NOT NULL COMMENT '��ע' after db_updated_at;", compareUnits.ChangeSql);

            compareUnits.ApplyChanges();
            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.Empty(compareUnits.ChangeSql);
        }

        /// <summary>
        /// ����IsDropRedundancy
        /// ����IsSyncIndex
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
        /// �Ա���ͬ�ı�
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
        /// ����column��Ĭ�ϲ�ͬ��������
        /// </summary>
        [Fact]
        public void CompareSingleTable_AddColumn_Test()
        {
            var metaData = new MetaData(connectionString1);
            var compareUnits = new CompareUnits(metaData, metaData);

            compareUnits.Compare("event_received_template", "event_received_local");
            Assert.True(compareUnits.ChangeSql.Count == 1);
            Assert.Contains("alter table demo.event_received_local add COLUMN Remark varchar(45) NOT NULL COMMENT '��ע' after db_updated_at;", compareUnits.ChangeSql);
        }

        /// <summary>
        /// ����column��ͬ������
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
        /// ������IsDropRedundancy���õ����
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
        /// ����IsDropRedundancy
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
        /// ����IsDropRedundancy
        /// ����IsSyncIndex
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
