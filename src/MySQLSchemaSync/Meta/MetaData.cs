using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;

namespace MySQLSchemaSync.Meta
{
    /// <summary>
    /// 数据库元数据
    /// TODO:异常情况校验
    /// schema不存在的情况
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// 数据库连接串
        /// </summary>
        private string connectionString;

        /// <summary>
        /// TABLE SCHEMA
        /// </summary>
        public string Schema { get; private set; }

        private IDbConnection con;

        public Dictionary<string, Table> Tables { get; private set; }

        public MetaData(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Init()
        {
            try
            {
                this.con = new MySql.Data.MySqlClient.MySqlConnection(this.connectionString);
                this.Schema = this.Schema ?? con.Database;

                this.Tables = new Dictionary<string, Table>();

                GetTablesFromDb();
                GetCreateTables();
                GetAllTableColumns();
                GetAllTableKeys();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.con.Dispose();
            }
        }

        public IDbConnection GetConnection()
        {
            return new MySql.Data.MySqlClient.MySqlConnection(this.connectionString);
        }

        /// <summary>
        /// 获取所有表信息
        /// </summary>
        private void GetTablesFromDb()
        {
            List<string> tables = con.Query<string>($"show tables from {Schema}").AsList();

            foreach (var t in tables)
            {
                Tables.Add(t, new Table { TableName = t });
            }
        }

        /// <summary>
        /// 获取表的创建sql
        /// </summary>
        private void GetCreateTables()
        {
            foreach (var t in Tables.Values)
            {
                GetCreateTable(t);
            }
        }

        private void GetCreateTable(Table table)
        {
            var sql = $"show create table {Schema}.{table.TableName}";
            var reader = con.ExecuteReader(sql);

            if (reader.Read())
            {
                table.CreateTable = reader["Create Table"].ToString();
                reader.Close();
            }
        }

        /// <summary>
        /// 获取所有表的字段信息
        /// </summary>
        private void GetAllTableColumns()
        {
            foreach (var t in Tables.Values)
            {
                GetTableColumns(t);
            }
        }

        private void GetTableColumns(Table table)
        {
            var sql = @"select 
	                        COLUMN_NAME,COLUMN_TYPE,IS_NULLABLE,COLUMN_DEFAULT,COLUMN_COMMENT,EXTRA 
                        from 
	                        information_schema.columns
                        where 
 	                        TABLE_SCHEMA=@tableSchema and TABLE_NAME = @tableName 
                        order by ORDINAL_POSITION asc;";

            var reader = con.ExecuteReader(sql, new { tableSchema = Schema, tableName = table.TableName });

            while (reader.Read())
            {
                Column column = new Column
                {
                    Name = reader["COLUMN_NAME"].ToString(),
                    Type = reader["COLUMN_TYPE"].ToString(),
                    IsNull = reader["IS_NULLABLE"].ToString(),
                    DefaultValue = reader["COLUMN_DEFAULT"].ToString(),
                    Comment = reader["COLUMN_COMMENT"].ToString(),
                    Extra = reader["EXTRA"].ToString()
                };
                table.Columns.Add(column.Name, column);
            }

            reader.Close();
        }

        /// <summary>
        /// 获取所有表的索引信息
        /// </summary>
        private void GetAllTableKeys()
        {
            foreach (var t in Tables.Values)
            {
                GetTableKeys(t);
            }
        }

        private void GetTableKeys(Table table)
        {
            var sql = $"show keys from {Schema}.{table.TableName}";
            var reader = con.ExecuteReader(sql);

            Index last = null;
            while (reader.Read())
            {
                string keyName = reader["Key_name"].ToString();

                if (last == null || keyName != last.IndexName)
                {
                    last = new Index();
                    last.IndexName = keyName;
                    last.Columns.Add(reader["Column_name"].ToString());
                    last.NotUnique = reader["Non_unique"].ToString();

                    table.Indexes.Add(last.IndexName, last);
                }
                else
                {
                    // 表明这两个key在同一索引中
                    last.Columns.Add(reader["Column_name"].ToString());
                }
            }

            reader.Close();
        }
    }
}
