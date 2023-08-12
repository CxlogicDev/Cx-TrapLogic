﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Linq.Expressions;

namespace CxUtility.EFCoreData;


[Flags]
internal enum EntityActions : byte
{
    /// <summary>
    /// No Action
    /// </summary>
    _ = 0x00,
    /// <summary>
    /// Add New Entity into Change Tracker. Not Saved to Db
    /// </summary>
    Add_Entity = 0x01,//A >> 0001
    /// <summary>
    /// Update Entity into Change Tracker. . Not Saved to Db
    /// </summary>
    Update_Entity = 0x02,//U >> 0010
    /// <summary>
    /// Add or Update 
    /// </summary>
    Add_Update_Entity = 0x03,//M:AU >> 0011 
    /** Save Entities Below  */
    //Save_Enity = 0x04,//S >> 0100
    //Add_Save_Entity = 0x05,//M:AS >> 0101
    //Update_Save_Entity = 0x06,//M:US >> 0110
    //Add_Update_Save_Entity = 0x07,//M:AUS >> 0111
    /** Save And Clean Tracker  */
    //CleanChangeTracker = 0x08,//C >> 1000 = S:0x04 >> 0100
    /***/
    //Add_CCT_Entity = 0x09,//M:1001 == 0x05:0101 AddSave
    //Update_CCT_Entity = 0x0A,//M:1010 == 0x06:0110 updateSave
    //Add_Update_CCT_Entity = 0x0B,//M:1011 == 0x07 AddUpdateSave
    /***/
    //Save_CCT_Entity = 0x0C,//M:1100 == 0x08 save Only
    /***/
    //Add_Save_CCT_Entity = 0x0D,//M:1101 == 0x05 AddSave
    //Update_Save_CCT_Entity = 0x0E,//M:1110 == 0x06 UpdateSave
    //Add_Update_Save_CCT_Entity = 0x0F,//M:1111 == 0x07 AddUpdateSave

}

/// <summary>
/// The Options used to update an entity. 
/// Note: The Model is required when building 
/// </summary>
/// <typeparam name="T">The Model that is being updated</typeparam>
internal class ContextOptions<T> where T : class, new()
{
    /// <summary>
    /// The Action that will be taken.
    /// </summary>
    public EntityActions EntityAction { get; }

    /// <summary>
    /// The DbContext object
    /// </summary>
    public DbContext? DatabaseContext { get; }

    /// <summary>
    /// Check to see if there is Content To save
    /// </summary>
    //public bool hasSaveContent => ((EntityAction & EntityActions.Save_Enity) == EntityActions.Save_Enity) && !DatabaseContext.isNull() && DatabaseContext.ChangeTracker.HasChanges();

    public ContextOptions(EntityActions EntityAction, DbContext DatabaseContext, Action<ContextOptions<T>> SetOptions) : this(EntityAction, SetOptions)
    {
        this.DatabaseContext = DatabaseContext;
    }

    public ContextOptions(EntityActions EntityAction, Action<ContextOptions<T>> SetOptions)
    {        
        //SkipProperties = SkipProperties ?? new List<string>();
        if (SetOptions == default)
            throw new DBContextStopException($"The {nameof(SetOptions)} options must be set and cannot be null ");

        SetOptions?.Invoke(this);

        if(EntityAction == EntityActions._)
                throw new DBContextStopException("You need to Choose an Entity Action");


        this.EntityAction = EntityAction;
        
        if (Model is null)
        {
            string log_Message = $"The Supplied Model of type: {typeof(T).Name} that is being {EntityAction} is null or missing.";
            logger?.LogError(message: log_Message);
            throw new DBContextStopException($"{nameof(ContextOptions<T>)}.{nameof(Model)} cannot be null!!!!");
        }

        if ((new[] { EntityActions.Update_Entity, EntityActions.Add_Update_Entity/*, EntityActions.Update_Save_Entity, EntityActions.Add_Update_Save_Entity*/ }).Contains(EntityAction))
        {
            if (SearchPredicate == default)
            {
                string log_Message = $"The Update Cannot be done because the {nameof(SearchPredicate)} is null or missing.";
                logger?.LogError(message: log_Message);
                log_Message = $"You need to supply an {nameof(SearchPredicate)} so that the entity that is being updated can be found. ";
                throw new DBContextContinueException(log_Message);
            }

            if (!(UpdateFields?.Count > 0))
            {
                string log_Message = $"You have Choosen an Update Action but did not suplly any UpdateFields. No Update can be performed";
                logger?.LogWarning(message: log_Message);
                IsValid = IsValid && (EntityAction == EntityActions.Update_Entity /*|| EntityAction == EntityActions.Update_Save_Entity*/);
            }
        }
    }

    /// <summary>
    /// A logger object
    /// </summary>
    public ILogger? logger { get; set; }

    /// <summary>
    /// Tell the function to save the update back to the database. Default is False.
    /// </summary>
    public bool SaveEnityAfterUpdate { get; set; } = false;

    /// <summary>
    /// The object to update
    /// </summary>
    public T Model { get; set; }

    [property: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("SearchPredicate")]
    /// <summary>
    /// The Search Predicate to pill 
    /// </summary>    
    public Expression<Func<T, bool>>? SearchPredicate { get; set; }

    /// <summary>
    /// Tells the context what properties to skip in the update
    /// </summary>
    public IList<string> SkipProperties { get; set; } = new List<string>();

    /// <summary>
    /// The fields that are allowed to be updated
    /// </summary>
    public IList<string> UpdateFields { get; set; } = new List<string>();

    /// <summary>
    /// Currently: False if Update only without Update Fields.
    /// </summary>
    public bool IsValid { get; private set; } = true;
       
}

/// <summary>
/// DB Context Extention Utilities
/// </summary>
public static partial class DBContextUtility
{

    /// <summary>
    /// Null check pass through. Error if the value is null otherwise allow the object to pass through. 
    /// </summary>    
    /// <param name="obj">The pass through object of type T</param>
    /// <param name="isContinueException">True will produce an DBContextContinueException. False will produce an DBContextStopException </param>
    /// <param name="exceptionMessage"></param>
    /// <exception cref="DBContextContinueException"></exception>
    /// <exception cref="DBContextStopException"></exception>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T DBContextErrorIfNull<T>(this T? obj, bool isContinueException, string? exceptionMessage = null) => obj ??
        (isContinueException ? throw new DBContextContinueException(exceptionMessage ?? "") : throw new DBContextStopException(exceptionMessage ?? ""));

}

public class ContextEntityOptions<T> where T : class, new()
{
    /// <summary>
    /// The Action that will be taken.
    /// </summary>
    internal EntityActions EntityAction { get; set; } = EntityActions._;

    /// <summary>
    /// The DbContext object
    /// </summary>
    public DbContext? DatabaseContext { get; }

    /// <summary>
    /// Check to see if there is Content To save
    /// </summary>
    //public bool hasSaveContent => ((EntityAction & EntityActions.Save_Enity) == EntityActions.Save_Enity) && !DatabaseContext.isNull() && DatabaseContext.ChangeTracker.HasChanges();

    public ContextEntityOptions(DbContext DatabaseContext, Action<ContextEntityOptions<T>> SetOptions) : this(SetOptions)
    {
        this.DatabaseContext = DatabaseContext;

        //EntityActions EntityAction,
    }

    public ContextEntityOptions(Action<ContextEntityOptions<T>> SetOptions)
    {
        //SkipProperties = SkipProperties ?? new List<string>();
        if (SetOptions == default)
            throw new DBContextStopException($"The {nameof(SetOptions)} options must be set and cannot be null ");

        SetOptions?.Invoke(this);

        //if (EntityAction == EntityActions._)
        //    throw new DBContextStopException("You need to Choose an Entity Action");

        if (Model is null)
        {
            string log_Message = $"The Supplied Model of type: {typeof(T).Name} that is being {EntityAction} is null or missing.";
            logger?.LogError(message: log_Message);
            throw new DBContextStopException($"{nameof(ContextOptions<T>)}.{nameof(Model)} cannot be null!!!!");
        }

        if ((new[] { EntityActions.Update_Entity, EntityActions.Add_Update_Entity/*, EntityActions.Update_Save_Entity, EntityActions.Add_Update_Save_Entity*/ }).Contains(EntityAction))
        {
            if (SearchPredicate == default)
            {
                string log_Message = $"The Update Cannot be done because the {nameof(SearchPredicate)} is null or missing.";
                logger?.LogError(message: log_Message);
                log_Message = $"You need to supply an {nameof(SearchPredicate)} so that the entity that is being updated can be found. ";
                throw new DBContextContinueException(log_Message);
            }

            if (!(UpdateFields?.Count > 0))
            {
                string log_Message = $"You have Choosen an Update Action but did not suplly any UpdateFields. No Update can be performed";
                logger?.LogWarning(message: log_Message);
                IsValid = IsValid && (EntityAction == EntityActions.Update_Entity /*|| EntityAction == EntityActions.Update_Save_Entity*/);
            }
        }
    }

    /// <summary>
    /// A logger object
    /// </summary>
    public ILogger? logger { get; set; }

    /// <summary>
    /// Tell the function to save the update back to the database. Default is False.
    /// </summary>
    public bool SaveEnityAfterUpdate { get; set; } = false;

    /// <summary>
    /// The object to update
    /// </summary>
    public T Model { get; set; }

    [property: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("SearchPredicate")]
    /// <summary>
    /// The Search Predicate to pill 
    /// </summary>    
    public Expression<Func<T, bool>>? SearchPredicate { get; set; }

    /// <summary>
    /// Tells the context what properties to skip in the update
    /// </summary>
    public IList<string> SkipProperties { get; set; } = new List<string>();

    /// <summary>
    /// The fields that are allowed to be updated
    /// </summary>
    public IList<string> UpdateFields { get; set; } = new List<string>();

    /// <summary>
    /// Currently: False if Update only without Update Fields.
    /// </summary>
    public bool IsValid { get; private set; } = true;   
}