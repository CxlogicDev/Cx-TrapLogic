using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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
            param.DbType = DbValueTypeSelection(paramValue);
        cmd.Parameters.Add(param);
        return cmd;
    }

    /// <summary>
    /// Chnage the object to the value type
    /// </summary>
    /// <param name="obj">The object to decide the database value type</param>    
    private static DbType DbValueTypeSelection(object obj) => obj switch
    {

        int => DbType.Int32,
        long => DbType.Int64,
        short => DbType.Int16,
        ushort => DbType.UInt16,
        uint => DbType.UInt32,
        ulong => DbType.UInt64,
        byte => DbType.Byte,
        byte[] => DbType.Binary,
        DateTimeOffset => DbType.DateTime2,
        DateTime => DbType.DateTime,
        bool => DbType.Boolean,
        _ => DbType.String,
    };

    /// <summary>
    /// Adds a OUTPUT Param to the Command Ex: @param = .WithSqlParam("param", paramValue)
    /// </summary>        
    /// <param name="paramName">The name of the parameter to add to the command</param>
    /// <param name="paramValueType">The value Type of the output object to add to the command</param>
    /// <returns>The Command that was build for the next pipe line process</returns>
    public static DbCommand WithSqlOutputParam(this DbCommand cmd, string paramName, System.Data.DbType paramValueType)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        param.DbType = paramValueType;

        param.Direction = System.Data.ParameterDirection.Output;
        cmd.Parameters.Add(param);

        return cmd;
    }

    /// <summary>
    /// Adds a OUTPUT Param to the Command Ex: @param = .WithSqlParam("param", paramValue)
    /// </summary>    
    /// <param name="paramName">The name of the parameter to add to the command</param>
    /// <param name="paramValue">The value object to add to the command</param>
    /// <exception cref="InvalidOperationException">The command is invalid</exception>
    public static DbCommand WithSqlOutputParam(this DbCommand cmd, string paramName, object paramValue)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException("Call BuildCommand, BuildScriptCommand, or BuildStoreProcedureCommand before using this method");

        var param = cmd.CreateParameter();
        param.ParameterName = $"@{paramName.TrimStart('@')}";
        param.DbType = DbValueTypeSelection(paramValue);
        param.Direction = System.Data.ParameterDirection.Output;
        cmd.Parameters.Add(param);

        return cmd;
    }

    /*
     * Marked for Deletion
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

    */

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
                    var mapping_v = colMapping[prop.Name.ToLower()];

                    if (mapping_v.isNull())
                        continue;

                    var val = dr.GetValue(mapping_v.ColumnOrdinal.ErrorIfNull().Value);

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

    /// <summary>
    /// Builds a simple object list nulls will be removed. Simple types are string, bool, int, decimal, ....
    /// </summary>
    /// <typeparam name="T">The type of object to process</typeparam>
    /// <param name="dr">The datareader used to read the rows</param>
    /// <exception cref="FormatException"></exception>
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
                    throw new FormatException("The Return is not a simple type. Simple types are string, bool, int, decimal, ....");                

                isFirst = false;

                map = colMapping.FirstOrDefault();
            }

            if (map.Value?.ColumnOrdinal == null)
                continue;

            object? val = dr.GetValue(map.Value.ColumnOrdinal.Value);

            if(val.isNotNull())               
                objList.Add((T)val);
        }

        dr.Close();

        return objList;
    }

    /// <summary>
    /// Build the Object List after running the StoreProcedure
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>        
    /// <returns>A list of mapped objects</returns>
    public static IList<T> Exec_StoredProc<T>(this DbCommand command) => command.Exec_Command<T>();

    /// <summary>
    /// Build the Object List after running the Command
    /// </summary>
    /// <typeparam name="T">The object for the list</typeparam>        
    /// <returns>A list of mapped objects</returns>
    public static IList<T> Exec_Command<T>(this DbCommand command)
    {
        using (command)
        {
            if (command.Connection.ErrorIfNull(new DataException("The connection is null")).State == System.Data.ConnectionState.Closed)
                command.Connection.Open();

            try
            {
                using var reader = command.ExecuteReader();

                return reader.HasRows ? reader.MapToList<T>() : new List<T>();
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
    public static IList<T> Exec_CommandSimple<T>(this DbCommand command)
    {
        using (command)
        {
            if (command.Connection.ErrorIfNull(new DataException("The connection is null")).State == System.Data.ConnectionState.Closed)
                command.Connection.Open();
            try
            {
                using var reader = command.ExecuteReader();
                return reader.HasRows ? reader.MapToSimpleList<T>() : new List<T>();                
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
    public static int Exec_StoreProc(this DbCommand command)
    {
        int ct = 0;
        using (command)
        {
            if (command.Connection.ErrorIfNull(new DataException("The connection is null")).State == ConnectionState.Closed)
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
    public static int Exec_Command(this DbCommand command)
    {
        int ct = 0;
        using (command)
        {
            if (command.Connection.ErrorIfNull(new DataException("The connection is null")).State == System.Data.ConnectionState.Closed)
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
    /// Will save If Change Tracker has changes for tracked entries. 
    /// </summary>
    /// <param name="ctx">The Context used</param>
    /// <param name="CleanChangeTracker">Tell The save process to clean The Change Tracker after saved. Errors will Automatically Clean Tracker</param>
    /// <returns> -1: No Changes in tracker; else the number of changed records. </returns>
    public static async Task<int> SaveChangesCleanTrackerAsync(this DbContext ctx, bool CleanChangeTracker = false)
    {
        int ct;

        try
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            if (!ctx.ChangeTracker.HasChanges())
                return -1;

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
            throw new ArgumentNullException(nameof(ctx));

        ctx.ChangeTracker.Clear();
    }
}