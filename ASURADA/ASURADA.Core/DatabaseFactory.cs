﻿using ASURADA.Core.Databases;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASURADA.Core
{
    public static class DatabaseFactory
    {
        public static IDatabase Create(DataSourceInfo dataSourceInfo)
        {
            var datasourceType = dataSourceInfo.DataSourceType.ToLower();
            if (datasourceType == "pgsql" || datasourceType == "postgresql")
            {
                var datasource = new PgSqlDatabase(dataSourceInfo);
                return datasource;
            }
            if (datasourceType == "mysql")
            {
                var datasource = new MySqlDatabase(dataSourceInfo);
                return datasource;
            }
            throw new NotSupportedException($"The database type '{dataSourceInfo.DataSourceType}' is not supported");
        }
    }
}
