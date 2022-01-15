﻿
namespace System;

using Text.Json;
using Diagnostics.CodeAnalysis;

/// <summary>
/// Use for common re-used methods 
/// </summary>
public static partial class CxExtensions
{

    /// <summary>
    /// Test to see if the String has Characters.
    /// </summary>
    /// <param name="str">The string to test</param>
    /// <param name="allowBlankSpace">Are blank Chars allowed</param>    
    public static bool hasCharacters([NotNullWhen(true)] this string? str, bool allowBlankSpace = false) => str?.Length > 0 && (allowBlankSpace ? true : str.Trim().Length > 0);

    /// <summary>
    /// Test to see if the string has No Chars. Blank Spaces are Ignored. 
    /// </summary>
    /// <param name="str">The string to test for not content.</param>
    public static bool hasNoCharacters([NotNullWhen(false)] this string? str) => !(str?.Trim().Length > 0);

    /// <summary>
    /// Quick null Object Check. 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool isNull([NotNullWhen(false)] this object? Vanity_obj) => Vanity_obj is null;

    /// <summary>
    /// Quick not null Object Check.
    /// </summary>
    /// <param name="Vanity_obj"></param>
    /// <returns></returns>
    public static bool isNotNull([NotNullWhen(true)] this object? Vanity_obj) => Vanity_obj is not null;

    /// <summary>
    /// Helps to with null checker to ensure a supplied object is not null. 
    /// </summary>
    /// <param name="obj">the object that is returned if not null</param>
    /// <param name="optionalexception">an optional Exception to return if the object is null. General ArgumentNullException returned if not supplied with a optionalexception</param>
    [return: NotNull]
    public static T ErrorIfNull<T>([NotNull]this T? obj, Exception? optionalexception = null) => obj ?? throw (optionalexception ?? new ArgumentNullException());

    /// <summary>
    /// Null Check & Validation Check . Helps with the Null Checker. then validates the object with a validation check supplied.
    /// </summary>
    /// <typeparam name="T">The checking object type</typeparam>
    /// <param name="obj">The object to check validation</param>
    /// <param name="validation">The validation delegat</param>
    /// <param name="validationException">optional supplied exception</param>
    /// <returns>The object if not null and passes validation</returns>
    /// <exception cref="InvalidOperationException">if the validation check on the object fails</exception>
    [return: NotNull]
    public static T ErrorIfNull_Or_NotValid<T>([NotNull]this T? obj, Func<T, bool> validation, Exception? validationException = default, Exception? nullException = default)
    {
        //Test Null 
        _ = obj ?? throw (nullException ?? new ArgumentNullException(nameof(obj)));
        _ = validation ?? throw (new ArgumentNullException(nameof(validation)));


        if (!validation(obj))
            throw validationException ?? new InvalidOperationException(message: "The object validation was invalid.");

        return obj;
    }

    /// <summary>
    /// Validation Check only. Validates the object with a validation check supplied
    /// </summary>
    /// <typeparam name="T">The checking object type</typeparam>
    /// <param name="obj">The object to check validation</param>
    /// <param name="validation">The validation delegat</param>
    /// <param name="optionalexception">optional supplied exception</param>
    /// <returns>The object if not null and passes validation</returns>
    /// <exception cref="InvalidOperationException">if the validation check on the object fails</exception>
    public static T? Error_IfNotValid<T>(this T? obj, Func<T?, bool> validation, Exception? optionalexception = null)
    {
        //Test Null        
        _ = validation ?? throw (optionalexception ?? new ArgumentNullException(nameof(validation)));

        if (!validation(obj))
            throw new InvalidOperationException(message: "The object validate was invalid.");

        return obj;
    }

    /// <summary>
    /// Will produce a list of indexs of where values are stored
    /// </summary>
    /// <typeparam name="T">The Object value to search from </typeparam>
    /// <param name="values">The Enumerable that is being searched</param>
    /// <param name="val">The object value to look for in the search</param>
    /// <returns></returns>
    public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val) =>
        values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();

    //------------------------------------- System.Test.Json 

    /// <summary>
    /// Will Take a class object and convert it to json string. Intended Use is for simple Objects
    /// </summary>
    /// <typeparam name="Obj">The ObjectType being converted to json</typeparam>
    /// <param name="Model">The object being converted</param>
    /// <param name="Options">Options Allows by Json Api</param>    
    public static string ToJson<Obj>(this Obj Model, JsonSerializerOptions? Options = default) where Obj : class =>
           JsonSerializer.Serialize<Obj>(Model, Options ?? new JsonSerializerOptions { });

    /// <summary>
    /// Converts a json string to a object. Intended Use is for simple Objects
    /// </summary>
    /// <typeparam name="obj">The ObjectType being Converted to json</typeparam>
    /// <param name="json">The json to Convert</param>
    /// <param name="Options">The options allowed by the Json Api</param>
    /// <returns></returns>
    public static obj? FromJson<obj>(this string json, JsonSerializerOptions? Options = default) where obj : class =>
        JsonSerializer.Deserialize<obj>(json, Options ?? new JsonSerializerOptions { });

    /// <summary>
    /// Take a Pascal or Camal Case String to a Space seperated display. Ex: (accountId | AccountId) = Account Id
    /// </summary>
    /// <param name="Name">The Display String to use</param>
    /// <param name="SetFirstCharUpper">Tells the dispaly the starting char should always be Uppercase</param>
    public static string CaseNameDisplay(this string Name, bool SetFirstCharUpper = true)
    {
        if (Name == null)
            return string.Empty;

        List<char> clst = new List<char>();

        Name.ToList().ForEach(c => { if (clst.Count == 0) clst.Add(SetFirstCharUpper ? char.ToUpper(c) : c); else if (char.IsUpper(c)) clst.AddRange($" {c}"); else clst.Add(c); });

        return new string(clst.ToArray());
    }
}

/*
 
 - ToDo: look into a yaml converter
 */
