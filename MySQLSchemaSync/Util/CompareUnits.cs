using MySQLSchemaSync.Meta;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MySQLSchemaSync.Util
{
    public class CompareUnits
    {
        public MetaData Source { get; private set; }
        public MetaData Target { get; private set; }
        public List<String> ChangeSql { get; private set; }

        public CompareUnits(MetaData source, MetaData target)
        {
            this.Source = source;
            this.Target = target;
            this.ChangeSql = new List<string>();
        }

        /// <summary>
        /// 比较所有表
        /// </summary>
        public void Compare()
        {
            this.ChangeSql = new List<string>();

            CompareSchema();
            CompareTables();
            CompareKeys();
        }

        /// <summary>
        /// Apply DDL
        /// </summary>
        public void ApplyChangeSql()
        {
            if (ChangeSql == null)
                return;

            using (var con = Target.GetConnection())
            {
                foreach (var sql in ChangeSql)
                {
                    con.Execute(sql);
                }
            }
        }

        /// <summary>
        /// 比较具体的两张表
        /// </summary>
        /// <param name="sourceTableName"></param>
        /// <param name="targetTableName"></param>
        /// <returns></returns>
        public void Compare(string sourceTableName, string targetTableName)
        {
            this.ChangeSql = new List<string>();

            CompareSchema();

            if (!Source.Tables.ContainsKey(sourceTableName))
            {
                throw new ApplicationException("source table not exist.");
            }

            if (!Target.Tables.ContainsKey(targetTableName))
            {
                throw new ApplicationException("target table not exist.");
            }

            CompareSingleTable(Source.Tables[sourceTableName], Target.Tables[targetTableName]);
        }


        #region Private Methods

        /// <summary>
        /// vaify schema
        /// </summary>
        private void CompareSchema()
        {
            // if not exist
            // changeSql.add("create database if not exists " + SqlUtil.getDbString(source.getSchema())+";");

            Source.Init();
            Target.Init();
        }

        /// <summary>
        /// compare all table 
        /// </summary>
        private void CompareTables()
        {
            foreach (var t in Source.Tables.Values)
            {
                if (!Target.Tables.ContainsKey(t.TableName))
                {
                    // if target not exist this table,create it!
                    ChangeSql.Add(t.CreateTable + ";");
                    continue;
                }

                // compare column and index
                CompareSingleTable(t, Target.Tables[t.TableName]);
            }
        }

        /// <summary>
        /// compare column and index
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="targetTable"></param>
        private void CompareSingleTable(Table sourceTable, Table targetTable)
        {
            CompareColumns(sourceTable, targetTable);
        }

        private void CompareColumns(Table sourceTable, Table targetTable)
        {
            // 记录最后一个比较的column
            string after = null;
            foreach (var column in sourceTable.Columns.Values)
            {
                if (!targetTable.Columns.ContainsKey(column.Name))
                {
                    // 如果对应的target没有这个字段,直接alter
                    var sql = $"alter table {Target.Schema}.{targetTable.TableName} add COLUMN {column.Name} {column.Type} ";
                    sql += column.IsNull == "NO" ? "NOT NULL " : "NULL ";

                    if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                    {
                        if (column.Type.Contains("varchar"))
                        {
                            sql += $"DEFAULT '{column.DefaultValue}' ";
                        }
                        else
                        {
                            sql += $"DEFAULT {column.DefaultValue} ";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(column.Comment))
                    {
                        sql += $"COMMENT '{column.Comment}' ";
                    }

                    if (after != null)
                    {
                        sql += $"after {after}";
                    }
                    ChangeSql.Add(sql + ";");
                }
                else
                {
                    var sql = $"alter table {Target.Schema}.{targetTable.TableName} change {column.Name} ";
                    // 比较两者字段,如果返回null,表明一致
                    string sqlExtend = CompareSingleColumn(column, targetTable.Columns[column.Name]);
                    if (sqlExtend != null)
                    {
                        ChangeSql.Add(sql + sqlExtend + ";");
                    }
                }

                after = column.Name;
            }

            // remove the target redundancy columns
            foreach (var column in targetTable.Columns.Values)
            {
                if (!sourceTable.Columns.ContainsKey(column.Name))
                {
                    // redundancy , so drop it
                    var sql = $"alter table {Target.Schema}.{targetTable.TableName} drop COLUMN {column.Name} ";
                    ChangeSql.Add(sql + ";");
                }
            }
        }

        private string CompareSingleColumn(Column sourceColumn, Column targetColumn)
        {
            List<string> modify = new List<string>();
            if (sourceColumn.Equals(targetColumn))
            {
                return null;
            }

            if (sourceColumn.Name != targetColumn.Name)
            {
                // never reach here
                throw new ApplicationException("the bug in this tool");
            }

            var changeSql = $"{sourceColumn.Name} {sourceColumn.Type} ";

            changeSql += sourceColumn.IsNull == "NO" ? "NOT NULL " : "NULL ";

            if (sourceColumn.Extra.ToUpper().Contains("AUTO_INCREMENT"))
            {
                changeSql += "AUTO_INCREMENT ";
            }

            if (!string.IsNullOrWhiteSpace(sourceColumn.DefaultValue))
            {
                if (sourceColumn.Type.Contains("varchar"))
                {
                    changeSql += $"DEFAULT '{sourceColumn.DefaultValue}' ";
                }
                else
                {
                    changeSql += $"DEFAULT {sourceColumn.DefaultValue} ";
                }
            }

            if (!string.IsNullOrWhiteSpace(sourceColumn.Comment))
            {
                changeSql += $"COMMENT '{sourceColumn.Comment}' ";
            }

            return changeSql;
        }

        /// <summary>
        /// compare the index for all table
        /// </summary>
        private void CompareKeys()
        {
            foreach (var t in Source.Tables.Values)
            {
                // 这样就需要比较两者的索引)
                if (Target.Tables.ContainsKey(t.TableName))
                {
                    CompareSingleKeys(t, Target.Tables[t.TableName]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="targetTable"></param>
        private void CompareSingleKeys(Table sourceTable, Table targetTable)
        {
            foreach (var index in sourceTable.Indexes.Values)
            {
                var sql = $"alter table {Target.Schema}.{targetTable.TableName} ";
                if (targetTable.Indexes.ContainsKey(index.IndexName) == false)
                {
                    if (index.IndexName == "PRIMARY")
                    {
                        sql += "add primary key ";
                    }
                    else
                    {
                        if (index.NotUnique == "0")
                        {
                            sql += $"add unique {index.IndexName} ";
                        }
                        else
                        {
                            sql += $"add index {index.IndexName} ";
                        }
                    }
                    sql += $"(`";
                    foreach (var key in index.Columns)
                    {
                        sql += $"{key.Trim()}`,`";
                    }
                    sql = sql.Substring(0, sql.Length - 2) + ")";

                    ChangeSql.Add(sql + ";");
                }
            }

            foreach (var index in targetTable.Indexes.Values)
            {
                if (sourceTable.Indexes.ContainsKey(index.IndexName) == false)
                {
                    // drop index
                    var sql = $"alter table {Target.Schema}.{targetTable.TableName} ";
                    if (index.IndexName == "PRIMARY")
                    {
                        sql += "drop primary key ";
                    }
                    else
                    {
                        sql += $"drop index {index.IndexName} ";
                    }
                    ChangeSql.Add(sql + ";");
                }
            }
        }

        #endregion
    }
}
