using MySQLSchemaSync.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLSchemaSync.Util
{
    public interface ICompareUnits
    {
        MetaData Source { get; }

        MetaData Target { get; }

        List<String> ChangeSql { get; }

        CompareUnitsOption Option { get; }

        void Compare();

        void Compare(string sourceTableName, string targetTableName);

        void ApplyChanges();
    }
}
