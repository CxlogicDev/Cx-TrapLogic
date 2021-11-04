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
    public static async Task<T> AddUpdateEntity<T>(this DbSet<T> entityObj, T Input_Obj, Expression<Func<T, bool>> SearchPredicate, params string[] UpdateFields) 
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
    public static async Task<T?> UpdateEntity<T>(this DbSet<T> entityObj, T Itm, Expression<Func<T, bool>> SearchPredicate, params string[] UpdateFields) 
        where T : class, new() => await entityObj.UpdateEntityAsync(o =>
        {
            o.Model = Itm;
            o.UpdateFields = UpdateFields.ToList();
            o.SearchPredicate = SearchPredicate;
        });

    /// <summary>
    /// Loads a entity into the Change Tracker using a lock. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityObj"></param>
    /// <param name="itm"></param>
    /// <param name="SearchPredicate"></param>
    /// <param name="lockObj"></param>
    /// <param name="UpdateFields"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T? UpdateEntityLock<T>(this DbSet<T> entityObj, T itm, Expression<Func<T, bool>> SearchPredicate, params string[] UpdateFields)
        where T : class, new()
    {
        //Return if an update cannot be performed
        if (SearchPredicate == null) throw new ArgumentException("Invalid Argument", nameof(SearchPredicate));
        else if (itm == null) throw new ArgumentException("Invalid Argument");
        else if (UpdateFields.Length <= 0) throw new ArgumentException("Invalid Argument");
        else if (!typeof(T).GetProperties().Any(a => UpdateFields.Contains(a.Name))) throw new ArgumentException("Invalid Argument");
        
        return entityObj.UpdateEntityLock(o =>
        {
            o.Model = itm;
            o.UpdateFields = UpdateFields;
            o.SearchPredicate = SearchPredicate;
        });
    }

    /// <summary>
    /// Will save changes in the tracked entries.
    /// </summary>
    /// <param name="db">The Context used</param>
    /// <param name="CleanChangeTracker">Tell The save process to clean The change Tracker</param>
    public static async Task<int> SaveChangesCleanTrackerAsync(this DbContext db, bool CleanChangeTracker = false)
    {
        int ct = 0;
        try
        {
            if (db == null)
                throw new ArgumentNullException("The DbContext is null and should not be.");

            if (db.ChangeTracker.HasChanges())
                ct = await db.SaveChangesAsync();

            if (CleanChangeTracker)
                db.ChangeTracker.Clear();

        }
        catch (Exception Ex)
        {
            db?.ChangeTracker.Clear();
            Console.WriteLine(Ex.ToString());
            throw;
        }

        return ct;
    }

    //*
    static async Task<T> AddEntityAsync<T>(this DbSet<T> entityObj, Action<ContextOptions<T>> SetOptions)
        where T : class, new()
    {

        ContextOptions<T> Options = new ContextOptions<T>(EntityActions.Add_Update_Entity, SetOptions);

        T? record = default;

        try
        {
            if (!Options.SearchPredicate.isNull())
                record = await entityObj.UpdateEntityAsync(SetOptions);

                    //(await entityObj.FirstOrDefaultAsync(Options.SearchPredicate));

            if (record is null)
            {
                //New Entities
                record = Options.Model;

                //Add in the entity
                entityObj.Add(record);
            }


            //Not possible at this point
            //if (Options.SaveEnityAfterUpdate && !Options.DatabaseContext.isNull() && Options.DatabaseContext.ChangeTracker.HasChanges())
            //{
            //    await Options.DatabaseContext.SaveChangesAsync();
            //}
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());
            throw;
        }

        return record;

    }

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
        ContextOptions<T> Options = new ContextOptions<T>(EntityActions.Update_Entity, SetOptions);

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
            Options.logger?.LogError(Ex, Ex.Message);

            throw;
        }

        return record;
    }

    /// <summary>
    /// Runs a update only on the context
    /// </summary>
    /// <typeparam name="T">The Entity property for the context</typeparam>
    /// <param name="entityObj">The entity object mapper</param>
    /// <param name="contextLock">The context lock if avalible</param>
    /// <param name="SetOptions">The options applied to the request being made</param>
    /// <returns>The entity that is being updated</returns>
    static T? UpdateEntityLock<T>(this DbSet<T> entityObj, Action<ContextOptions<T>> SetOptions)
         where T : class, new()
    {
        T? record = default;
        ContextOptions<T> Options = new ContextOptions<T>(EntityActions.Update_Entity ,SetOptions);

        lock (entityObj.ErrorIfNull())
        {
            try
            {
                record = entityObj.FirstOrDefault(Options.SearchPredicate.DBContextErrorIfNull(true));

                if (record == null)
                    return record;

                var SkipFields = Options.Model.GetType()
                        .GetProperties()
                        .Where(w => !Options.UpdateFields.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                        .Select(s => s.Name)
                        .ToArray();

                var ChangeProps = Options.Model
                    .ModelUpdateFields(record, SkipFields);

                if (ChangeProps?.Count > 0)
                {
                    //We have Props to change
                    var entry = entityObj.Attach(record);

                    foreach (var item in ChangeProps)
                    {
                        var myProp = typeof(T).GetProperty(item);

                        if (myProp.isNull())
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
                Console.WriteLine(Ex.ToString());
                throw;
            }
        }

        return record;
    }

    /*/Method Unfinished
   static async Task<T> ProcessEntityAsync<T>(this DbSet<T> entityObj, EntityActions entityActions, Action<ContextOptions<T>> SetOptions) where T : class, new()
   {
       T? record = default;

       ContextOptions<T> Options = new ContextOptions<T>(entityActions, SetOptions);

       //Process Only if the Options are valid
       if (Options.IsValid)
       {
           #region Dev Method Part

           try
           {
               record = await entityObj.FirstOrDefaultAsync(Options.SearchPredicate);

               if (record == null && (Options.EntityAction & EntityActions.Add_Entity) != EntityActions.Add_Entity)
               {
                   if (Options.EntityAction == EntityActions.Save_Enity && Options.hasSaveContent)
                       await Options.DatabaseContext.SaveChangesCleanTrackerAsync();

                   return record;
               }

               //bool isMeetingOnTuesday = (meetingDays & Days.Tuesday) == Days.Tuesday;

               var SkipFields = Options.Model.GetType()
                       .GetProperties()
                       .Where(w => !Options.UpdateFields.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)))
                       .Select(s => s.Name)
                       .ToArray();

               var ChangeProps = Options.Model
                   .ModelUpdateFields(record, SkipFields);

               if (ChangeProps?.Count > 0)
               {
                   //We have Props to change
                   var entry = entityObj.Attach(record);

                   foreach (var item in ChangeProps)
                   {
                       var myProp = typeof(T).GetProperty(item);

                       myProp.SetValue(record, myProp.GetValue(Options.Model));

                       entry.Property(item).IsModified = true;
                   }
               }


               if (Options.SaveEnityAfterUpdate && Options.DatabaseContext.ChangeTracker.HasChanges())
               {
                   await Options.DatabaseContext.SaveChangesAsync();
               }
           }
           catch (Exception Ex)
           {
               Options.logger?.LogError(Ex, Ex.Message);

               throw;
           }

           #endregion
       }

       return record ?? throw new DBContextException($"The {nameof(SetOptions)} is invalid");
   }
   //*/

}




