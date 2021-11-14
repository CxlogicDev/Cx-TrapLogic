using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.EFCoreData;

public static partial class DBContextUtility
{
    /// <summary>
    /// Builds a DbCommand that will run a script or store Procedure
    /// </summary>
    /// <param name="query">The Query That needs to be ran</param>
    /// <param name="commandType">The cammand type to run</param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand BuildCommand(this DbContext ctx, string query, System.Data.CommandType commandType = System.Data.CommandType.Text)
    {
        if ((query ?? "").Length <= 0)
            throw new ArgumentException("The Store Procdure is needed");

        var cmd = ctx.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = query;
        cmd.CommandType = commandType;
        return cmd;
    }

    /// <summary>
    /// Builds a DbCommand that will run a script
    /// </summary>
    /// <param name="query">The Query That needs to be ran </param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand BuildScriptCommand(this DbContext ctx, string query)
    {
        if ((query ?? "").Length <= 0)
            throw new ArgumentException("The Store Procdure is needed");

        var cmd = ctx.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = query;
        return cmd;
    }

    /// <summary>
    /// Builds a DbCommand that will run a store Procedure
    /// </summary>        
    /// <param name="StoreProcName">The Query That needs to be ran</param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand BuildStoreProcedureCommand(this DbContext ctx, string StoreProcName)
    {
        if ((StoreProcName ?? "").Length <= 0)
            throw new ArgumentException("The Store Procdure is needed");

        var cmd = ctx.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = StoreProcName;
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        return cmd;
    }

    /// <summary>
    /// Adds a Param to the Command Ex: @param = .WithSqlParam("param", paramValue)
    /// </summary>        
    /// <param name="paramName">The name of the parameter to add to the command</param>
    /// <param name="paramValue">The value object to add to the command</param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand WithSqlParam(this DbCommand cmd, string paramName, object paramValue, System.Data.ParameterDirection direction = System.Data.ParameterDirection.Input)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        if (direction == System.Data.ParameterDirection.Input || direction == System.Data.ParameterDirection.InputOutput)
            param.Value = paramValue;
        else if (direction == System.Data.ParameterDirection.Output)
            param.DbType = Direction(paramValue);
        cmd.Parameters.Add(param);
        return cmd;
    }

    private static System.Data.DbType Direction(object obj)
    {
        switch (obj)
        {

            case int i: return System.Data.DbType.Int32;
            case long l: return System.Data.DbType.Int64;
            case short sh: return System.Data.DbType.Int16;
            case UInt16 u16: return System.Data.DbType.UInt16;
            case UInt32 u32: return System.Data.DbType.UInt32;
            case UInt64 u64: return System.Data.DbType.UInt64;
            case byte bt: return System.Data.DbType.Byte;
            case byte[] ba: return System.Data.DbType.Binary;
            case DateTimeOffset os: return System.Data.DbType.DateTime2;
            case DateTime d: return System.Data.DbType.DateTime;
            case bool b: return System.Data.DbType.Boolean;
            case string s:
            default: return System.Data.DbType.String;
        }

    }

    /// <summary>
    /// Adds a OUTPUT Param to the Command Ex: @param = .WithSqlParam("param", paramValue)
    /// </summary>        
    /// <param name="paramName">The name of the parameter to add to the command</param>
    /// <param name="paramValue">The value object to add to the command</param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand WithSqlOutputParam(this DbCommand cmd, string paramName, System.Data.DbType paramValue)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        param.DbType = paramValue;

        param.Direction = System.Data.ParameterDirection.Output;
        cmd.Parameters.Add(param);

        return cmd;
    }

    public static DbCommand WithSqlOutputParam(this DbCommand cmd, string paramName, object paramValue)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        param.DbType = Direction(paramValue);
        param.Direction = System.Data.ParameterDirection.Output;
        cmd.Parameters.Add(param);

        return cmd;
    }

    public static DbCommand WithSqlOutputParam(this DbCommand cmd, string paramName, object paramValue, out DbParameter param)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        param.DbType = Direction(paramValue);
        param.Direction = System.Data.ParameterDirection.Output;
        cmd.Parameters.Add(param);

        return cmd;
    }

    /// <summary>
    /// Gets a param from the command
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static DbParameter OutputParam(this DbCommand cmd, string paramName)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.Parameters[$"@{paramName.TrimStart('@')}"];

        return param;
    }

    /// <summary>
    /// Gets a Array of output or return params
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static DbParameter[] OutputParams(this DbCommand cmd, string paramName)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.Parameters.Cast<DbParameter>().Where(w => (new[] { System.Data.ParameterDirection.Output, System.Data.ParameterDirection.ReturnValue, System.Data.ParameterDirection.InputOutput })
        .Contains(w.Direction)).ToArray();

        //[$"@{paramName.TrimStart('@')}"];

        return param;
    }

    /// <summary>
    /// Private Mapping a row to a object
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>
    /// <returns>A list of mapped objects</returns>
    private static IList<T> MapToList<T>(this DbDataReader dr)
    {
        var objList = new List<T>();
        var props = typeof(T).GetRuntimeProperties();

        while (dr.Read())
        {
            T obj = Activator.CreateInstance<T>();
            int ct = 0;

            var colMapping = dr.GetColumnSchema()
            .Where(x => props.Any(y => y.Name.Equals(x.ColumnName, StringComparison.CurrentCultureIgnoreCase)))
            .ToDictionary(key => key.ColumnName.ToLower());

            foreach (var prop in props)
            {
                ct++;
                try
                {
                    var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                    prop.SetValue(obj, val == DBNull.Value ? null : val);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
            }
            objList.Add(obj);

        }

        dr.Close();

        return objList;
    }

    private static IList<T> MapToSimpleList<T>(this DbDataReader dr)
    {
        var objList = new List<T>();
        var props = typeof(T).GetRuntimeProperties();



        KeyValuePair<string, DbColumn> map = default;

        bool isFirst = true;

        while (dr.Read())
        {
            if (isFirst)
            {
                var colMapping = dr.GetColumnSchema().ToDictionary(key => key.ColumnName.ToLower());

                if (colMapping.Count > 1)
                {
                    throw new FormatException("The Return is not a simple type. Simple types are string, bool, int, decimal, ....");
                }

                isFirst = false;
                map = colMapping.FirstOrDefault();
            }

            var val = dr.GetValue(map.Value.ColumnOrdinal.Value);
            T obj = (T)(val == DBNull.Value ? null : val);
            objList.Add(obj);
        }

        dr.Close();

        return objList;
    }

    /// <summary>
    /// Build the Object List after running the StoreProcedure
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>        
    /// <returns>A list of mapped objects</returns>
    public static IList<T> ExecuteStoredProc<T>(this DbCommand command) => command.ExecuteCommand<T>();

    /// <summary>
    /// Build the Object List after running the Command
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>        
    /// <returns>A list of mapped objects</returns>
    public static IList<T> ExecuteCommand<T>(this DbCommand command)
    {
        using (command)
        {
            if (command.Connection.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    return reader.HasRows ? reader.MapToList<T>() : new List<T>();
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }

    /// <summary>
    /// Build the Simple Object List after running the Command
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>        
    /// <returns>A list of mapped Simple Objects</returns>
    public static IList<T> ExecuteCommandSimple<T>(this DbCommand command)
    {
        using (command)
        {
            if (command.Connection.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();
            try
            {
                using (var reader = command.ExecuteReader())
                {

                    return reader.HasRows ? reader.MapToSimpleList<T>() : new List<T>();
                }
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }

    /// <summary>
    /// Executes the The Command with no Return assumed
    /// </summary>
    /// <returns>The number of rows effeceted</returns>
    public static int ExecStoreProc(this DbCommand command)
    {
        int ct = 0;
        using (command)
        {
            if (command.Connection.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();

            try
            {
                ct = command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        return ct;
    }

    /// <summary>
    /// Executes the The Store procedure with no Return assumed
    /// </summary>
    /// <returns>The number of rows effeceted</returns>
    public static int ExecCommand(this DbCommand command)
    {
        int ct = 0;
        using (command)
        {
            if (command.Connection.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();

            try
            {
                ct = command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        return ct;
    }

    /// <summary>
    /// Will save changes in the tracked entries.
    /// </summary>
    /// <param name="ctx">The Context used</param>
    /// <param name="CleanChangeTracker">Tell The save process to clean The change Tracker</param>
    public static async Task<int> SaveChangesCleanTrackerAsync(this DbContext ctx, bool CleanChangeTracker = false)
    {
        int ct = 0;
        try
        {
            if (ctx == null)
                throw new ArgumentNullException("The DbContext is null and should not be.");

            if (ctx.ChangeTracker.HasChanges())
                ct = await ctx.SaveChangesAsync();

            if (CleanChangeTracker)
                ctx.CleanChangeTracker();
        }
        catch (Exception Ex)
        {
            ctx?.ChangeTracker.Clear();
            Console.WriteLine(Ex.ToString());
            throw;
        }

        return ct;
    }

    /// <summary>
    /// Will Clean the Change Tracker
    /// </summary>
    /// <param name="ctx"></param>
    public static void CleanChangeTracker(this DbContext ctx)
    {
        if (ctx == null)
            throw new ArgumentNullException("The DbContext is null and should not be.");

        ctx.ChangeTracker.Clear();
    }
}