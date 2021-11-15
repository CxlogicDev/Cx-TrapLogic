using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CxUtility.EFCoreData.SQLite;

public static class SQLiteDbSetUtility
{
    /// <summary>
    /// Loads a new or updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="optionsActions">The options that can be that apply to the model</param>
    public static Task<Entity?> AddUpdateSqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, Action<SqliteEntityOptions<Entity>> optionsActions)
        where Entity : class, new() => entityObj.AddUpdateSqliteEntity_Async(new SqliteEntityOptions<Entity>(optionsActions));

    /// <summary>
    /// Loads a new or updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="options">The options that can be that apply to the model</param>
    public static async Task<Entity?> AddUpdateSqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, SqliteEntityOptions<Entity> options)
        where Entity : class, new()
    {

        //Not a new 
        var (result, _) = await entityObj.UpdateSqliteEntityAsync(options);//_Model, SearchPredicate, UpdateFields

        //Need to do a Add 
        if (result is null)
        {
            result = options.Model;
            entityObj.Add(result);
        }

        return result;
    }

    /// <summary>
    /// updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="optionsActions">The options that can be that apply to the model</param>
    public static Task<(Entity? dbEntity, bool can_Update)> UpdateSqliteEntityAsync<Entity>(this DbSet<Entity> entityObj, Action<SqliteEntityOptions<Entity>> optionsActions)
       where Entity : class, new() => entityObj.UpdateSqliteEntityAsync(new SqliteEntityOptions<Entity>(optionsActions));

    /// <summary>
    /// updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="options">The options that can be that apply to the model</param>
    public static async Task<(Entity? dbEntity, bool can_Update)> UpdateSqliteEntityAsync<Entity>(this DbSet<Entity> entityObj, SqliteEntityOptions<Entity> options)
        where Entity : class, new()
    {
        Entity? _record = options.Search_Predicate.isNull()? default : (await entityObj.FirstOrDefaultAsync(options.Search_Predicate));

        //Return if an update cannot be performed
        if (_record.isNull() || !options.Can_Update || !typeof(Entity).GetProperties().Any(a => options.Update_Fields.ErrorIfNull().Contains(a.Name)))
           return (_record, false);
        

        try
        {
            var ChangeProps = options.Model
                .ModelUpdateFields(_record, options.skip_fields());

            if (ChangeProps?.Count > 0)
            {
                //We have Props to change
                var entry = entityObj.Attach(_record);

                entry.UpdateProps(options.Model, _record, options.Update_Fields);
            }
        }
        catch (Exception Ex)
        {
            if (options._ILogger == null)
                Console.WriteLine(Ex);
            else
                options._ILogger.LogError(Ex, Ex.Message);

            throw;
        }

        return (_record, true);

    }

    /// <summary>
    /// Runs Updates on IProxy Properties
    /// </summary>
    /// <typeparam name="T">The IProxy object to Process</typeparam>
    /// <param name="entry">The Entity Object</param>
    /// <param name="model">The Entity Changes</param>
    /// <param name="_record">The Current Entity Record</param>
    /// <param name="UpdateProps">Properties allowed to be updated</param>
    /// <returns>The record object that is being updated</returns>
    static T UpdateProps<T>(this EntityEntry<T> entry, T model, T _record, params string[] UpdateProps) where T : class, new()
    {

        bool hasUpdates = false;

        var EditDateAttributes = typeof(T)
            .GetProperties()
            .Where(w => w.GetCustomAttributes(typeof(UpdateDateOnChangeAttribute), true).Length > 0)
            .ToArray();

        List<string> UdProps = new List<string>(UpdateProps);

        foreach (var item in UdProps)
            hasUpdates = hasUpdates || _setProp(item);
        
        if (hasUpdates)
        {
            foreach (var item in EditDateAttributes)
            {
                //Set the model edit date to update in the system on any update to other properties
                item.SetValue(model, DateTime.UtcNow);

                //Run the property update
                _setProp(item.Name);
            }
        }

        //Sets the property to update with ud value
        bool _setProp(string item)
        {
            bool IsModified = false;

            try
            {
                var myProp = _record.GetType().GetProperty(item);

                if (myProp == default)
                    return false;

                if (myProp.GetValue(model) is null || myProp.GetValue(_record) is null)
                    IsModified = !(myProp.GetValue(model) is null && myProp.GetValue(_record) is null);
                else
                    IsModified = !myProp.GetValue(model).Equals(myProp.GetValue(_record));

                myProp.SetValue(_record, myProp.GetValue(model));

                entry.Property(item).IsModified = IsModified;
            }
            catch (Exception Ex)
            {

            }

            return IsModified;
        }

        return _record;
    }
}



public record SqliteEntityOptions<Entity> where Entity : class, new()
{
    public SqliteEntityOptions()
    {
        Model.ErrorIfNull();
    }

    public SqliteEntityOptions(Action<SqliteEntityOptions<Entity>> action)
    {
        action.ErrorIfNull().Invoke(this);

        Model.ErrorIfNull();
    }

    /// <summary>
    /// Attch a logger for output
    /// </summary>
    public ILogger? _ILogger { get; set; }

    /// <summary>
    /// The Object being Added/Updated
    /// </summary>
    public Entity Model { get; set; }

    /// <summary>
    /// The search perdicate to find an exsiting entity
    /// </summary>
    public Expression<Func<Entity, bool>>? Search_Predicate { get; set; }

    /// <summary>
    /// The Fields that are allowed to be updated
    /// </summary>
    public string[]? Update_Fields { get; set; }

    /// <summary>
    /// Generates the Properites that need to be skipped 
    /// </summary>
    internal string[] skip_fields() => !Can_Update ? new string[0] : Model.GetType()
                    .GetProperties()
                    .Where(w => !Update_Fields.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                    .Select(s => s.Name)
                    .ToArray();

    /// <summary>
    /// Tell the processing context that an update can be proformed
    /// </summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Update_Fields), nameof(Search_Predicate))]
    internal bool Can_Update => Model.isNotNull() && Update_Fields?.Length > 0 && Search_Predicate.isNotNull();
}


