using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CxUtility;

public enum PropertyCompaireOptionTypes { DateTimeDoTimeCompaire }

public static class ObjectUtility
{
    /// <summary>
    /// Transfers the Model 1 values that have athe same name into a new model.
    /// </summary>
    /// <typeparam name="M2">The new model type where values are being transfered to</typeparam>
    /// <param name="Md1">The Orignal model</param>
    /// <param name="ignorCase">Tell the function to match ignoring case</param>
    /// <param name="SetProperties">Will Allow setting or overridng of a property {PropName: Key; PropValue: Value}</param>
    /// <returns></returns>
    public static M2 ModelTransfer<M2>(this object Md1, bool ignorCase = false, bool copyCanWrite_Only = true, params string[] excludeProps) where M2 : class, new() =>
        Md1.ModelTransfer<M2>(null, ignorCase, copyCanWrite_Only, excludeProps);

    /// <summary>
    /// Transfers the Model 1 values that have athe same name into a new model.
    /// </summary>
    /// <typeparam name="M2">The new model type where values are being transfered to</typeparam>
    /// <param name="Md1">The Orignal model</param>
    /// <param name="ignorCase">Tell the function to match ignoring case</param>
    /// <param name="SetProperties">Will Allow setting or overridng of a property {PropName: Key; PropValue: Value}</param>
    /// <returns></returns>
    public static M2 ModelTransfer<M2>(this object Md1, IDictionary<string, object>? SetProperties, bool ignorCase = false, bool copyCanWrite_Only = true, params string[] excludeProps) where M2 : class, new()
    {
        //Creates the new model
        M2 exmdl = new M2();
        var lstProp = copyCanWrite_Only ? typeof(M2).GetTypeInfo().GetRuntimeProperties().Where(w => w.CanWrite) : typeof(M2).GetTypeInfo().GetRuntimeProperties();
        //Loop through the convert to models properties to set them
        if (ignorCase && excludeProps?.Length > 0)
            lstProp = lstProp.Where(w => !excludeProps.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)));
        else if (excludeProps?.Length > 0)
            lstProp = lstProp.Where(w => !excludeProps.Contains(w.Name));


        foreach (var M2prop in lstProp)
        {
            try
            {
                //Pulls the matching property from model1 
                var M1Prop = Md1.GetType().GetTypeInfo().GetRuntimeProperties().FirstOrDefault(f => f.Name.Equals(M2prop.Name, (ignorCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)));

                //Test to make sure model1 is tranferable to Model2
                if (M1Prop != null && M1Prop.PropertyType == M2prop.PropertyType)
                {
                    //The property matches so get the value
                    var M1Value = M1Prop.GetValue(Md1);

                    //Set the value in model2
                    if (M1Value == null)
                    {
                        M2prop.SetValue(exmdl, null);
                    }
                    else
                    {
                        M2prop.SetValue(exmdl, M1Value);
                    }
                }

                //Uses anything in the SetProperties dictionary
                if (SetProperties?.ContainsKey(M2prop.Name) ?? false)
                {
                    //Cool
                    if (SetProperties[M2prop.Name] is null)
                        M2prop.SetValue(exmdl, null);
                    else if (M2prop.PropertyType == SetProperties[M2prop.Name].GetType())
                        M2prop.SetValue(exmdl, SetProperties[M2prop.Name]);
                }
            }
            catch (Exception)
            {
                //Errors Continued because miss match
            }
        }

        return exmdl;
    }

    /// <summary>
    /// Copies values into a new model of itself.
    /// </summary>
    /// <typeparam name="M2">The model that is being copyied</typeparam>
    /// <param name="Md1">The Orignal model</param>        
    /// <returns></returns>
    public static M2 ModelCopy<M2>(this M2 Md1, bool copyCanWrite_Only = true, params string[] excludeProps) where M2 : class, new()
    {
        //Creates the new model
        M2 exmdl = new M2();
        var lstProp = copyCanWrite_Only ? exmdl.GetType().GetTypeInfo().GetRuntimeProperties().Where(w => w.CanWrite) : exmdl.GetType().GetTypeInfo().GetRuntimeProperties();
        //Loop through the convert to models properties to set them

        if (excludeProps?.Length > 0)
            lstProp = lstProp.Where(w => !excludeProps.Contains(w.Name));

        foreach (var M2prop in lstProp)
        {
            try
            {
                //Pulls the matching property from model1 
                var M1Prop = Md1.GetType().GetTypeInfo().GetRuntimeProperties().FirstOrDefault(f => f.Name.Equals(M2prop.Name, StringComparison.CurrentCulture));

                //Test to make sure model1 is tranferable to Model2
                if (M1Prop != null && M1Prop.PropertyType == M2prop.PropertyType)
                {
                    //The property matches so get the value
                    var M1Value = M1Prop.GetValue(Md1);

                    //Set the value in model2
                    if (M1Value == null)
                    {
                        M2prop.SetValue(exmdl, null);
                    }
                    else
                    {
                        M2prop.SetValue(exmdl, M1Value);
                    }
                }
            }
            catch (Exception)
            {
                //Errors Continued because miss match
            }
        }

        return exmdl;
    }

    /// <summary>
    /// Tranfer Related fields into from one object to a nother
    /// </summary>
    /// <typeparam name="MT">The transfer Into object</typeparam>
    /// <param name="Md1">Teh object that is being copied</param>
    /// <param name="TranferInto">The object being copied into</param>
    /// <param name="ignorCase">Tell the method to ignore property name case</param>

    public static void ModelTranferFields<MF, MT>(this MF Md1, MT TranferInto, bool ignorCase = false, bool copyCanWrite_Only = true, params string[] excludeProps)
         where MF : class, new()
        where MT : class, new()
        => Md1.ModelTranferFields(TranferInto, null, ignorCase, copyCanWrite_Only, excludeProps);

    /// <summary>
    /// Tranfer Related fields into from one object to a nother
    /// </summary>
    /// <typeparam name="MT">The compaired object type</typeparam>
    /// <param name="Md1">The object that is being copied</param>
    /// <param name="TranferInto">The object being copied into</param>
    /// <param name="ignorCase">Tell the method to ignore property name case</param>
    public static void ModelTranferFields<MF, MT>(this MF Md1, MT TranferInto, IDictionary<string, object>? SetProperties, bool ignorCase = false, bool copyCanWrite_Only = true, params string[] excludeProps)
        where MF : class, new()
        where MT : class, new()
    {
        //Creates the new model
        if (TranferInto == null) TranferInto = new MT();
        var lstProp = copyCanWrite_Only ? TranferInto.GetType().GetTypeInfo().GetRuntimeProperties().Where(w => w.CanWrite) : TranferInto.GetType().GetTypeInfo().GetRuntimeProperties();

        if (ignorCase && excludeProps?.Length > 0)
            lstProp = lstProp.Where(w => !excludeProps.Any(a => a.Equals(w.Name, StringComparison.CurrentCultureIgnoreCase)));
        else if (excludeProps?.Length > 0)
            lstProp = lstProp.Where(w => !excludeProps.Contains(w.Name));

        //Loop through the convert to models properties to set them
        foreach (var M2prop in lstProp)
        {
            try
            {
                //Pulls the matching property from model1 
                var M1Prop = Md1.GetType().GetTypeInfo().GetRuntimeProperties().FirstOrDefault(f => f.Name.Equals(M2prop.Name, (ignorCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)));

                //Test to make sure model1 is tranferable to Model2
                if (M1Prop != null && M1Prop.PropertyType == M2prop.PropertyType)
                {
                    //The property matches so get the value
                    var M1Value = M1Prop.GetValue(Md1);

                    //Set the value in model2
                    if (M1Value == null)
                    {
                        M2prop.SetValue(TranferInto, null);
                    }
                    else
                    {
                        M2prop.SetValue(TranferInto, M1Value);
                    }
                }

                //Uses anything in the SetProperties dictionary
                if (SetProperties?.ContainsKey(M2prop.Name) ?? false)
                {
                    //Cool
                    if (SetProperties[M2prop.Name] is null)
                        M2prop.SetValue(TranferInto, null);
                    else if (M2prop.PropertyType == SetProperties[M2prop.Name].GetType())
                        M2prop.SetValue(TranferInto, SetProperties[M2prop.Name]);
                }
            }
            catch (Exception)
            {
                //Errors Continued because miss match
            }
        }
    }

    /// <summary>
    /// Test To see if there are fields that need to be updated.
    /// </summary>
    /// <typeparam name="M1">The compaired object type</typeparam>
    /// <param name="Current">The Curent On that is changing</param>
    /// <param name="Source">The Original Object</param>
    /// <param name="skip">Properies to skip</param>
    public static bool ModelhasUpdates<M1>(this M1 Current, M1 Source, params string[] skip) where M1 : class, new()
    {
        if (Source != null)
        {
            foreach (var Mprop in Current.GetType().GetTypeInfo().GetRuntimeProperties())//.Where(w => w.Name.ToLower().EndsWith("id"))
            {
                if (!skip.Contains(Mprop.Name) && isDifferent(Mprop.GetValue(Current), Mprop.GetValue(Source)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The Fields that need to be updated.
    /// </summary>
    /// <typeparam name="M1">The compaired object type</typeparam>
    /// <param name="Current">The Curent On that is changing</param>
    /// <param name="Source">The Original Object</param>
    /// <param name="skip">Properies to skip</param>
    /// 
    public static List<string> ModelUpdateFields<M1>(this M1 Current, M1 Source, params string[] skip) where M1 : class, new()
    {
        if (Source != null)
        {
            var UdFields = new List<string>();

            foreach (var Mprop in Current.GetType().GetTypeInfo().GetRuntimeProperties())//.Where(w => w.Name.ToLower().EndsWith("id"))
            {
                //var Sprop = Source.GetType().GetProperty(Mprop.Name);
                if (!skip.Contains(Mprop.Name) && Mprop.isPropDifferent(Current, Source, PropertyCompaireOptionTypes.DateTimeDoTimeCompaire))
                {
                    UdFields.Add(Mprop.Name);
                }
            }

            return UdFields;
        }

        return new List<string>();
    }

    static bool isPropDifferent<P>(this P PropType, object Source, object Current, params PropertyCompaireOptionTypes[] options) where P : PropertyInfo
    {
        if (PropType is null)
            return false;
        else if (Source is null && Current is null)
            return false;        
        else if ((Source is null && Current is not null) || (Source is not null && Current is null))
            return true;        

        //Source and Current are not null here 
        object _Source = Source ?? throw new NullReferenceException();
        object _Current = Current ?? throw new NullReferenceException();

        if (_Source.GetType().GetTypeInfo().GetDeclaredProperty(PropType.Name) == null ||
            _Current.GetType().GetTypeInfo().GetDeclaredProperty(PropType.Name) == null)
            return false;
        else //if (PropType.GetValue(Source) == Current.PropertyType)
        {
            if (PropType.PropertyType == typeof(DateTime))
            {

                if (Source is not DateTime || Current is not DateTime)
                    return false;

                var c1 = (DateTime)(PropType.GetValue(Source) ?? throw new NullReferenceException());
                var c2 = (DateTime)(PropType.GetValue(Current) ?? throw new NullReferenceException());

                if (!options.Contains(PropertyCompaireOptionTypes.DateTimeDoTimeCompaire))
                {
                    //Only need to compaire the date element
                    c1 = c1.Date;
                    c2 = c2.Date;
                }

                var value = DateTime.Compare(c1, c2);

                return value != 0;
            }

            var thisEquals = false;

            var source_val = PropType.GetValue(_Source);
            var current_val = PropType.GetValue(_Current);

            if (source_val is null || current_val is null)
                thisEquals = source_val == current_val;            
            else
                thisEquals = source_val is not null && source_val.Equals(PropType.GetValue(_Current)) ;
            
            return !thisEquals; // (thisEquals && valueEquals);
        }

        throw new ArgumentException("The types do not match");
    }

    static bool isDifferent(object? Source, object? Current)
    {

        if (Source is null && Current is null)
            return false;
        if (Source is null && Current is not null)
            return true;
        else if (Source is not null && Current is null)
            return true;


        //Should have no nulls at this point
        object obj_Source = Source ?? throw new NullReferenceException();
        object obj_Current = Current ?? throw new NullReferenceException();

        if (Source.GetType() == Current.GetType())
        {
            if (Source is DateTime)
            {
                var c1 = (DateTime)Source;
                var c2 = (DateTime)Current;

                var value = DateTime.Compare(c1, c2);

                return value != 0;
            }

            return !Source.Equals(Current);

        }

        throw new ArgumentException("The types do not match");
    }

}
