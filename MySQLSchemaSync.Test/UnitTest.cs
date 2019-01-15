using MySQLSchemaSync.Meta;
using MySQLSchemaSync.Util;
using System;
using System.Linq;
using Xunit;

namespace MySQLSchemaSync.Test
{
    public class UnitTest : TestSuit
    {
        [Fact]
        public void MetaDataTest()
        {
            var connectionString = "server=localhost;port=3306;Initial Catalog=service_architecture_test;user id=root;password=123456;ConnectionReset=false";

            MetaData metaData = new MetaData(connectionString);

            metaData.Init();

            Assert.True(metaData.Tables.Count == 4);

            Assert.True(metaData.Tables.ContainsKey("api7_event_published"));
            Assert.True(metaData.Tables.ContainsKey("api7_event_received"));
            Assert.True(metaData.Tables.ContainsKey("domain_lego05event_received"));
            Assert.True(metaData.Tables.ContainsKey("tran_test"));

            Assert.True(metaData.Tables["api7_event_received"].Columns.ContainsKey("NextRetryTime"));
            Assert.True(metaData.Tables["api7_event_received"].Indexes.ContainsKey("idx_statusname_retry"));

            Assert.Equal(2, metaData.Tables["api7_event_received"].Indexes["idx_statusname_retry"].Columns.Count);
            Assert.Equal("CURRENT_TIMESTAMP", metaData.Tables["api7_event_received"].Columns["NextRetryTime"].DefaultValue);
            Assert.Equal("datetime", metaData.Tables["api7_event_received"].Columns["NextRetryTime"].Type);

            var x = metaData.Tables["api7_event_received"].ToString();
        }

        [Fact]
        public void CompareSingleTableTest()
        {
            var connectionString = "server=localhost;port=3306;Initial Catalog=service_architecture_test;user id=root;password=123456;ConnectionReset=false";
            MetaData metaData = new MetaData(connectionString);

            ICompareUnits compareUnits = new CompareUnits(metaData, metaData, new CompareUnitsOption { IsDropRedundancy = true });

            compareUnits.Compare("api7_event_received", "domain_lego05event_received");
            Assert.True(compareUnits.ChangeSql.Any());
            // check changesql before apply
            //compareUnits.ApplyChanges();

            compareUnits.Compare("api7_event_received", "domain_lego05event_received");
            Assert.False(compareUnits.ChangeSql.Any());

            //compareUnits.Compare("domain_lego05event_received", "api7_event_received");
            //Assert.True(compareUnits.ChangeSql.Any());
        }
    }
}
