using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.EFCoreData;

public static partial class DBContextUtility
{
    /// <summary>
    /// Loads a new or updated entity into the context. This will not save the context Only add to Change tracker.
    /// </summary>
    /// <typeparam name="T">A class object of ITable</typeparam>
    /// <param name="entityObj">The DbSet entity the Add/Update Belongs to</param>
    /// <param name="Input_Obj">The Object being Added/Updated</param>
    /// <param name="SearchPredicate">The search perdicate to find an exsiting entity </param>
    /// <param name="UpdateFields">The Fields that are allowed to be updated. Leave empty if you want to Add Only</param>
    public static async Task<T> AddUpdateEntityAsync<T>(this DbSet<T> entityObj, T Input_Obj, Expression<Func<T, bool>> SearchPredicate, params string[] UpdateFields) 
        where T : class, new()
    {
        T? record = default;

        try
        {
            record = await entityObj.UpdateEntityAsync(o =>
            {
                o.UpdateFields = UpdateFields.ToList();
                o.SearchPredicate = SearchPredicate;
                o.Model = Input_Obj;
            });

            //Fix: No Update fields so a update cannot be ran and returns a null.
            //This will call into the Db and get the record if is exists
            if (record == null && SearchPredicate.isNotNull())
                record = await entityObj.FirstOrDefaultAsync(SearchPredicate.DBContextErrorIfNull(true));
        }
        catch (DBContextContinueException) { 
            //Exception may be an update only error problem
        }        
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());
            throw;
        }

        try
        {
            if (record == null)//No record found Add New record
            {
                record = Input_Obj;
                //Add in the entity
                entityObj.Add(record);
            }            
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());

            throw;
        }

        return record;
    }

    /// <summary>
    /// Loads a Update Entity into the context. This will not savethe context Only add to the Change Tarcker.
    /// </summary>
    /// <typeparam name="T">A class object of ITable</typeparam>
    /// <param name="entityObj">The DbSet entity the Add/Update Belongs to</param>
    /// <param name="Itm">The Object properties being updated</param>
    /// <param name="SearchPredicate">Required: The search perdicate to find the exsiting entity. </param>
    /// <param name="UpdateFields">The Fields that are allowed to be updated.</param>
    /// <returns>The Record that being updated.</returns>
    public static async Task<T?> UpdateEntityAsync<T>(this DbSet<T> entityObj, T Itm, Expression<Func<T, bool>> SearchPredicate, params string[] UpdateFields) 
        where T : class, new() => 
            await entityObj.UpdateEntityAsync(o =>
            {
                o.Model = Itm;
                o.UpdateFields = UpdateFields.ToList();
                o.SearchPredicate = SearchPredicate;
            });

    /// <summary>
    /// Runs a Async Call that updates only on the context
    /// </summary>
    /// <typeparam name="T">The Entity property for the context</typeparam>
    /// <param name="entityObj">The entity object mapper</param>
    /// <param name="SetOptions">The options applied to the request being made</param>
    /// <returns>The entity that is being updated or null if no entity can be updated</returns>
    static async Task<T?> UpdateEntityAsync<T>(this DbSet<T> entityObj, Action<ContextOptions<T>> SetOptions)
        where T : class, new()
    {        
        T? record = default;
        ContextOptions<T> Options = new (EntityActions.Update_Entity, SetOptions);

        //Return if an update cannot be performed
        if (Options.UpdateFields.Count  <= 0 || !typeof(T).GetProperties().Any(a => Options.UpdateFields.Contains(a.Name)))
            return record;

        try
        {
            record = await entityObj.FirstOrDefaultAsync(Options.SearchPredicate.DBContextErrorIfNull(true));

            if (record == null)
                return null;

            List<string> SkipFields = new(Options.SkipProperties);

            if (Options.UpdateFields?.Count > 0)
                SkipFields.AddRange(Options.Model.GetType()
                    .GetProperties()
                    .Where(w => !Options.UpdateFields.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                    .Select(s => s.Name));

            var ChangeProps = Options.Model
                .ModelUpdateFields(record, SkipFields.ToArray());

            if (ChangeProps?.Count > 0)
            {
                //We have Props to change
                var entry = entityObj.Attach(record);

                foreach (var item in ChangeProps)
                {
                    var myProp = typeof(T).GetProperty(item);

                    if(myProp.isNull())//No property to update continue the loop
                        continue;

                    myProp.SetValue(record, myProp.GetValue(Options.Model));

                    entry.Property(item).IsModified = true;
                }
            }

            //Not possible at this point
            //if (Options.SaveEnityAfterUpdate && !Options.DatabaseContext.isNull() && Options.DatabaseContext.ChangeTracker.HasChanges())
            //{
            //    await Options.DatabaseContext.SaveChangesAsync();
            //}
        }
        catch (Exception Ex)
        {
            Options.logger?.LogError(Ex, message: Ex.Message);

            throw;
        }

        return record;
    }


    /********************************* Adding in a new process to Return the Entities ************************************************************************************
     ********************************************************************************************************************************************************************* /

    //EntityActions

    */


    /// <summary>
    /// Runs a Async Call that updates only on the context
    /// Note: This should lonly be used on a single entity. 
    /// </summary>
    /// <typeparam name="T">The Entity property for the context</typeparam>
    /// <param name="entityObj">The entity object mapper</param>
    /// <param name="OptionAction">The setup option action applied to the request being made</param>
    /// <returns>The entity that is being updated or null if no entity can be updated</returns>
    public static Task<EntityResult<T>> UpdateEntityAsync<T>(this DbSet<T> entityObj, Action<EntityRequestOptions<T>> OptionAction)
        where T : class, new() => entityObj.UpdateEntityAsync(new EntityRequestOptions<T>(OptionAction));

    /// <summary>
    /// Runs a Async Call that updates only on the context
    /// Note: This should lonly be used on a single entity. 
    /// </summary>
    /// <typeparam name="T">The Entity property for the context</typeparam>
    /// <param name="entityObj">The entity object mapper</param>
    /// <param name="request">The Options to apply to the request being made</param>
    /// <returns>The entity that is being updated or null if no entity can be updated</returns>
    public static async Task<EntityResult<T>> UpdateEntityAsync<T>(this DbSet<T> entityObj, EntityRequestOptions<T> request)
        where T : class, new()
    {

        var result = new EntityResult<T>(request.Model, EntityResult<T>.EntityActionStatuses.Not_Processed, Array.Empty<string>());

        if (request.Model.isNotNull())
        {

            try
            {
                T[] records = request.SearchPredicate.isNull()? Array.Empty<T>():
                    await entityObj.AsNoTracking().Where(request.SearchPredicate)
                    .ToArrayAsync();

                if(records.Length > 1)
                {
                    //Error if multiple Entityies are found
                    result._errorMessages.Add("Multiple Entities Found but this method can only Process a single entity update. Please change your search perdicate to get a single entity");
                    result.Status = EntityResult<T>.EntityActionStatuses.Error;
                    return result;
                }

                records.Error_IfNotValid(v => v.isNull() || v.Length < 2, new DBContextMultipleEntityException());

                T? record = records.FirstOrDefault();

                if (record == null)
                {
                    //New Entity
                    record = request.Model;

                    //Add in the entity
                    entityObj.Add(record);

                    result.EntityModel = record.ModelCopy();
                    result.Status = EntityResult<T>.EntityActionStatuses.Add;
                    result.ChangeFields = record.GetType().GetProperties().Select(s => s.Name).ToArray();


                    return result;
                }
                else if (!(request.UpdateProperties.Count > 0) && !(request.SkipProperties.Count > 0))
                {
                    result.EntityModel = record.ModelCopy();
                    result.Status = EntityResult<T>.EntityActionStatuses.Update_Not_Processed;
                    return result;
                }

                List<string> SkipFields = new(request.SkipProperties);

                if (request.UpdateProperties?.Count > 0)
                    SkipFields.AddRange(request.Model.GetType()
                        .GetProperties()
                        .Where(w => !request.UpdateProperties.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                        .Select(s => s.Name));

                result.UpdateFields = request.Model
                    .ModelUpdateFields(record, SkipFields.ToArray())
                    .ToArray();

                if (result.UpdateFields.Length > 0)
                {

                    request.UpdateActions?.Invoke(request.Model, result.UpdateFields);

                    //We have Props to change
                    Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> entry = entityObj.Attach(record);

                    foreach (var item in result.UpdateFields)
                    {
                        var myProp = typeof(T).GetProperty(item);

                        if (myProp.isNull())//No property to update continue the loop
                            continue;

                        myProp.SetValue(record, myProp.GetValue(request.Model));

                        entry.Property(item).IsModified = true;
                    }
                }

            }
            catch (Exception Ex)
            {

                throw;
            }

        }

        if (request.Model == default)
            return new UpdateEntityResult<T>(request.Model, UpdateEntityStatuses.None, Array.Empty<string>());
    }

    /*

    /// <summary>
    /// Runs a Async Call that updates only on the context
    /// </summary>
    /// <typeparam name="T">The Entity property for the context</typeparam>
    /// <param name="entityObj">The entity object mapper</param>
    /// <param name="SetOptions">The options applied to the request being made</param>
    /// <returns>The entity that is being updated or null if no entity can be updated</returns>
    static async Task<EntityResponse<T>?> UpdateEntityAsync<T>(this DbSet<T> entityObj, Action<ContextOptions<T>> SetOptions)
        where T : class, new()
    {
        T? record = default;
        ContextOptions<T> Options = new(EntityActions.Update_Entity, SetOptions);

        //Return if an update cannot be performed
        if (Options.UpdateFields.Count <= 0 || !typeof(T).GetProperties().Any(a => Options.UpdateFields.Contains(a.Name)))
            return new EntityResponse<T>() { EntityItem = record, ChangeFields = Array.Empty<string>() };

        try
        {
            record = await entityObj.FirstOrDefaultAsync(Options.SearchPredicate.DBContextErrorIfNull(true));

            if (record == null)
                return null;

            List<string> SkipFields = new(Options.SkipProperties);

            if (Options.UpdateFields?.Count > 0)
                SkipFields.AddRange(Options.Model.GetType()
                    .GetProperties()
                    .Where(w => !Options.UpdateFields.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                    .Select(s => s.Name));

            var ChangeProps = Options.Model
                .ModelUpdateFields(record, SkipFields.ToArray());

            if (ChangeProps?.Count > 0)
            {
                //We have Props to change
                var entry = entityObj.Attach(record);

                foreach (var item in ChangeProps)
                {
                    var myProp = typeof(T).GetProperty(item);

                    if (myProp.isNull())//No property to update continue the loop
                        continue;

                    myProp.SetValue(record, myProp.GetValue(Options.Model));

                    entry.Property(item).IsModified = true;
                }
            }

            //Not possible at this point
            //if (Options.SaveEnityAfterUpdate && !Options.DatabaseContext.isNull() && Options.DatabaseContext.ChangeTracker.HasChanges())
            //{
            //    await Options.DatabaseContext.SaveChangesAsync();
            //}
        }
        catch (Exception Ex)
        {
            Options.logger?.LogError(Ex, message: Ex.Message);

            throw;
        }

        return record;
    }

    /*********************************************************************************************************************************************************************/
}

/// <summary>
/// Request Options
/// </summary>
/// <typeparam name="T"></typeparam>
public class EntityRequestOptions<T> where T : class, new()
{
    /// <summary>
    /// Build the Options into the entity 
    /// </summary>
    /// <param name="action"></param>
    public EntityRequestOptions(Action<EntityRequestOptions<T>> action)
    {
        action?.ErrorIfNull(new InvalidOperationException("No Request to process")).Invoke(this);

        Model.ErrorIfNull(new NullReferenceException($"The Model of Type: {typeof(T).Name} must me included"));

        //validate();
    }

    /// <summary>
    /// The Model That is being Added or Updated
    /// </summary>
    public T Model { get; set; }

    /// <summary>
    /// The Search Predicate used search the context for entities. Not suppling this will make it a Add Entity process
    /// </summary>
    public Expression<Func<T, bool>>? SearchPredicate { get; set; }
    
    /// <summary>
    /// The Properties that can be updated on a update process.
    /// </summary>
    public List<string> UpdateProperties { get; } = new ();
    
    /// <summary>
    /// Properties that need to be skipped For upload. Adding skip Properties will force all properties except the skipped to be updated unless specifying Update properties. 
    /// </summary>
    public List<string> SkipProperties { get; } = new ();

    /// <summary>
    /// Action on Update after Update Filelds have been determined; 
    /// Arg T: The Request Model
    /// Arg string[]: The Fields that ar going to be updated
    /// Return string[]: The new list of fileds that will get updated 
    /// </summary>
    public Func<T, string[], string[]>? UpdateActions { get; set; }

    /// <summary>
    /// The Context that you are pulling the data from. Not Required unless you want to auto save that update being made. 
    /// </summary>
    public DbContext? DatabaseContext { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool AutoSaveAction { get; set; }

    
    //void validate()
    //{
        
    //}
}


//[Flags]
//public enum EntityActionStatuses : byte
//{
//    /// <summary>
//    /// No Action
//    /// </summary>
//    None = 0x00,
//    /// <summary>
//    /// Add New Entity into Change Tracker. Not Saved to Db
//    /// </summary>
//    Add_Entity = 0x01,//A >> 0001
//    /// <summary>
//    /// Update Entity into Change Tracker. . Not Saved to Db
//    /// </summary>
//    Update_Entity = 0x02,//U >> 0010
//}

public class EntityResult<T> where T : class, new()
{

    public EntityResult(T model, EntityActionStatuses status, params string[] updateFields)
    {
        EntityModel = model;
        Status = status;
        ChangeFields = updateFields;
    }

    /// <summary>
    /// Process Response Status
    /// </summary>
    public enum EntityActionStatuses { Not_Processed, Error, Add, Update_Not_Processed, Update }

    public EntityActionStatuses Status { get; internal set; } = EntityActionStatuses.Not_Processed;

    public T? EntityModel { get; internal set; }

    public string[] ChangeFields { get; internal set; }

    public string[] ErrorMessages => _errorMessages.ToArray();
    internal List<string> _errorMessages { get; } = new();
}




