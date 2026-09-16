using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;

namespace BudgetWise.Configurations;

public static class SerilogConfiguration
{
    public static void Configure(string dbConnectionString)
    {
        var columnOptions = new ColumnOptions();

        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);

        columnOptions.AdditionalColumns = new Collection<SqlColumn>
        {
            new SqlColumn
            {
                ColumnName = "UserId",
                DataType = SqlDbType.Int
            },
            new SqlColumn
            {
                ColumnName = "Username",
                DataType = SqlDbType.NVarChar,
                DataLength = 100
            },
            new SqlColumn
            {
                ColumnName = "Action",
                DataType = SqlDbType.NVarChar,
                DataLength = 100
            },
            new SqlColumn
            {
                ColumnName = "EntityName",
                DataType = SqlDbType.NVarChar,
                DataLength = 100
            },
            new SqlColumn
            {
                ColumnName = "Activity",
                DataType = SqlDbType.NVarChar,
                DataLength = 500
            }
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent =>
                    logEvent.Properties.ContainsKey("Action"))
                .WriteTo.MSSqlServer(
                    dbConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "TransactionLogs",
                        AutoCreateSqlTable = false
                    },
                    columnOptions: columnOptions,
                    restrictedToMinimumLevel: LogEventLevel.Information))

            .CreateLogger();
    }
}