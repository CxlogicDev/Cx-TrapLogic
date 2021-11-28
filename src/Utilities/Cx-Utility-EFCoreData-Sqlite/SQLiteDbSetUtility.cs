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
    public static Task<Entity?> AddUpdate_SqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, Action<SqliteEntityOptions<Entity>> optionsActions)
        where Entity : class, new() => entityObj.AddUpdate_SqliteEntity_Async(new SqliteEntityOptions<Entity>(optionsActions));

    /// <summary>
    /// Loads a new or updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="options">The options that can be that apply to the model</param>
    public static async Task<Entity?> AddUpdate_SqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, SqliteEntityOptions<Entity> options)
        where Entity : class, new()
    {

        //Not a new 
        var (result, _) = await entityObj.Update_SqliteEntity_Async(options);//_Model, SearchPredicate, UpdateFields

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
    public static Task<(Entity? dbEntity, bool can_Update)> Update_SqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, Action<SqliteEntityOptions<Entity>> optionsActions)
       where Entity : class, new() => entityObj.Update_SqliteEntity_Async(new SqliteEntityOptions<Entity>(optionsActions));

    /// <summary>
    /// updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="Entity">The BBSet entity to process</typeparam>
    /// <param name="entityObj">The DBSet object to process</param>
    /// <param name="options">The options that can be that apply to the model</param>
    public static async Task<(Entity? dbEntity, bool can_Update)> Update_SqliteEntity_Async<Entity>(this DbSet<Entity> entityObj, SqliteEntityOptions<Entity> options)
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

        List<string> UdProps = new (UpdateProps);

        foreach (var prop_Name in UdProps)
        {
            if (_setProp(prop_Name, model, _record))
            {
                System.Reflection.PropertyInfo? myProp = typeof(T).GetType().GetProperty(prop_Name);

                if (myProp.isNotNull())
                {
                    hasUpdates = true;

                    myProp.SetValue(_record, myProp.GetValue(model));

                    entry.Property(prop_Name).IsModified = true;
                }
            }
        }
        
        if (hasUpdates)
        {
            foreach (var myProp in EditDateAttributes)
            {
                //Set the model edit date to update in the system on any update to other properties
                myProp.SetValue(_record, DateTime.UtcNow);

                entry.Property(myProp.Name).IsModified = true;
            }
        }

        //Sets the property to update with ud value
        bool _setProp(string prop_Name, T i_model, T i_record)
        {
            bool IsModified = false;

            try
            {
                System.Reflection.PropertyInfo? myProp = typeof(T).GetType().GetProperty(prop_Name);

                if (myProp is null)
                    return false;

                var t_model = myProp.GetValue(i_model);
                var t_record = myProp.GetValue(i_record);

                if (t_model is null || t_record is null)
                    IsModified = !(myProp.GetValue(i_model) is null && myProp.GetValue(i_record) is null);
                else
                    IsModified = !t_model.Equals(t_record);
            }
            catch (Exception Ex)
            {
                /*
                  if (options._ILogger == null)
                    Console.WriteLine(Ex);
                  else
                    options._ILogger.LogError(Ex, Ex.Message);                
                 */

                Console.WriteLine(Ex.Message);
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
    internal string[] skip_fields() => !Can_Update ? Array.Empty<string>() : Model.GetType()
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


