using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLSchemaSync.Meta
{
    /// <summary>
    /// 字段信息
    /// 
    /// select COLUMN_NAME,COLUMN_TYPE,IS_NULLABLE,COLUMN_DEFAULT,COLUMN_COMMENT,EXTRA from information_schema.columns
    /// where TABLE_SCHEMA=$db and TABLE_NAME=$table order by ORDINAL_POSITION asc
    /// </summary>
    public class Column
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string IsNull { get; set; }

        public string DefaultValue { get; set; }

        public string Comment { get; set; }

        public string Extra { get; set; }

        public override bool Equals(object o)
        {
            if (this == o)
                return true;

            if (o == null)
                return false;

            Column column = (Column)o;

            if (Name != column.Name)
                return false;

            if (Type != column.Type)
                return false;

            if (IsNull != column.IsNull)
                return false;

            if (DefaultValue != column.DefaultValue)
                return false;

            if (Extra != column.Extra)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = Name != null ? Name.GetHashCode() : 0;
            result = 31 * result + (Type != null ? Type.GetHashCode() : 0);
            result = 31 * result + (IsNull != null ? IsNull.GetHashCode() : 0);
            result = 31 * result + (DefaultValue != null ? DefaultValue.GetHashCode() : 0);
            result = 31 * result + (Comment != null ? Comment.GetHashCode() : 0);
            result = 31 * result + (Extra != null ? Extra.GetHashCode() : 0);

            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Column: name={Name}, type={Type}, isNull={IsNull}, defaultValue={DefaultValue}, comment={Comment}, extra={Extra}";
        }
    }
}
