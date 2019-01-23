using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLSchemaSync.Meta
{
    /// <summary>
    /// 索引信息
    /// </summary>
    public class Index
    {
        public List<string> Columns { get; set; } = new List<string>();

        public string IndexName { get; set; }

        /// <summary>
        /// 0表示unique，1表示普通索引
        /// </summary>
        public string NotUnique { get; set; }
    }
}
