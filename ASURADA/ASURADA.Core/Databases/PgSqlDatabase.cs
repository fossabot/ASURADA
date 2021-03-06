﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASURADA.Core.Databases
{
    public class PgSqlDatabase : IDatabase
    {
        DataSourceInfo dataSourceInfo;
        public PgSqlDatabase(DataSourceInfo dataSourceInfo)
        {
            this.dataSourceInfo = dataSourceInfo;
        }
        public async Task<List<TableInfo>> GetTables(string filter)
        {
            filter = FilterUtility.ConvertSearchPatternToRegex(filter);
            List<TableInfo> tables = new List<TableInfo>();
            using (var conn = new NpgsqlConnection(dataSourceInfo.ConnectionString))
            {
                await conn.OpenAsync();
                var tableSchema = conn.GetSchema("Tables");
                foreach (DataRow r in tableSchema.Rows)
                {
                    //var schema = r["table_schema"];
                    var table = r["table_name"].ToString();
                    var type = r["table_type"].ToString();
                    if (Regex.IsMatch(table, filter, RegexOptions.IgnoreCase))
                    {
                        tables.Add(new TableInfo() { TableName = table, TableType = type });
                    }
                }
            }
            return tables;
        }

        public Task<long> GetTotalRowNumber(string sql)
        {
            throw new NotImplementedException();
        }

        public async Task<QueryResult> RunSQL(string sql)
        {
            QueryResult result = new QueryResult();
            result.Data = new List<object[]>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                using (var conn = new NpgsqlConnection(dataSourceInfo.ConnectionString))
                {
                    await conn.OpenAsync();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var newrow = new object[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                newrow[i] = reader[i];
                            }
                            result.Data.Add(newrow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            stopwatch.Stop();
            result.QueryElapsedTime = stopwatch.ElapsedMilliseconds;
            return result;
        }
    }
}
