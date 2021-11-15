using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.EFCoreData.SQLite;

/// <summary>
/// Tells the context that a datetime property should be update if ant field is has an update out side of the this field
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class UpdateDateOnChangeAttribute : Attribute
{
    public UpdateDateOnChangeAttribute() { }

}