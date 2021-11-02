using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cx_TrapConsole;


// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategory("code")]
[System.Xml.Serialization.XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRoot(Namespace = "", ElementName = "configuration", IsNullable = false)]
public partial class NuGetconfiguration
{

    private packageSourcesAdd[]? packageSourcesField;

    private configurationAdd[]? packageRestoreField;

    private configurationAdd[]? bindingRedirectsField;

    private configurationAdd[]? packageManagementField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem("add", IsNullable = false)]
    public packageSourcesAdd[] packageSources
    {
        get
        {
            return this.packageSourcesField ?? new packageSourcesAdd[0];
        }
        set
        {
            this.packageSourcesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem("add", IsNullable = false)]
    public configurationAdd[] packageRestore
    {
        get
        {
            return this.packageRestoreField ?? new configurationAdd[0];
        }
        set
        {
            this.packageRestoreField = value;
        }
    }

    /// <remarks/>
    public configurationAdd[] bindingRedirects
    {
        get
        {
            return this.bindingRedirectsField ?? new configurationAdd[0];
        }
        set
        {
            this.bindingRedirectsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItem("add", IsNullable = false)]
    public configurationAdd[] packageManagement
    {
        get
        {
            return this.packageManagementField ?? new configurationAdd[0];
        }
        set
        {
            this.packageManagementField = value;
        }
    }

    //*********************************************************************************

    /// <summary>
    /// Adds/Updates a packageSource 
    /// </summary>
    /// <param name="add_object">The package Source Object to add</param>
    public void Add_PackageSources(params packageSourcesAdd[] add_object)
    {
        var temp = packageSourcesField.isNull() ? new Dictionary<string, packageSourcesAdd>() :
            packageSourcesField.ToDictionary(d => d.key, d => d);

        foreach (packageSourcesAdd i in add_object.Where(w => !w.isNull()))
            temp[i.key] = i;
    }

    /// <summary>
    /// Adds/Updates a PackageRestore object
    /// </summary>
    /// <param name="add_object">The Package Restore Object to add </param>
    public void Add_PackageRestore(params configurationAdd[] add_object)
    {
        var temp = packageRestoreField.isNull() ? new Dictionary<string, configurationAdd>() :
            packageRestoreField.ToDictionary(d => d.key, d => d);

        foreach (configurationAdd i in add_object.Where(w => !w.isNull()))
            temp[i.key] = i;        
    }

    /// <summary>
    /// Adds/Updates a Package Management object
    /// </summary>
    /// <param name="add_object">The Package Management Object to add</param>
    public void Add_PackageManagement(params configurationAdd[] add_object)
    {
        var temp = packageManagementField.isNull() ? new Dictionary<string, configurationAdd>() :
            packageManagementField.ToDictionary(d => d.key, d => d);

        foreach (configurationAdd i in add_object.Where(w => !w.isNull()))
            temp[i.key] = i;
    }

    //*********************************************************************************

    const string nuget_org_key = "nuget.org";
    const string nuget_org_value = "https://api.nuget.org/v3/index.json";
    const string nuget_org_ProVersion = "3";
    internal static NuGetconfiguration Build_NuGet_Config(string solution_Directory, string Package_Directory)
    {
        NuGetconfiguration? nuget = nuget = new NuGetconfiguration()
        {
            packageSources = new packageSourcesAdd[] {
                new packageSourcesAdd{ key = nuget_org_key, value=nuget_org_value, protocolVersion = nuget_org_ProVersion },
                new packageSourcesAdd{ key = "cx.trap", value = Package_Directory }
            },
            packageRestore = new configurationAdd[]
            {
                new configurationAdd{ key = "enabled", value = "True" },
                new configurationAdd{ key = "automatic", value = "True" },
            },
            bindingRedirects = new configurationAdd[]
            {
                new() { key = "skip", value = "False" }
            },
            packageManagementField = new configurationAdd[]
            {
                new configurationAdd{ key = "format", value = "0"},
                new configurationAdd{ key = "disabled", value = "False"}
            }
        }; 

        var path = Path.Combine(solution_Directory, "NuGet.Config");

        if (!File.Exists(path))
        {
            try
            {
                //Serilize to xml
                System.Xml.Serialization.XmlSerializer writer =
                    new System.Xml.Serialization.XmlSerializer(typeof(NuGetconfiguration));

                FileStream file = File.Create(path);

                writer.Serialize(file, nuget);
                file.Close();
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Something went wrong message: {Ex.Message}");                
            }
        }

        return nuget;
    }

}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategory("code")]
[System.Xml.Serialization.XmlType(AnonymousType = true)]
public record packageSourcesAdd
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private string keyField;

    private string valueField;

    private string protocolVersionField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    //private bool protocolVersionFieldSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string key
    {
        get
        {
            return this.keyField;
        }
        set
        {
            this.keyField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string protocolVersion
    {
        get
        {
            return this.protocolVersionField;
        }
        set
        {
            this.protocolVersionField = value;
        }
    }

    /// <remarks/>
    //[System.Xml.Serialization.XmlIgnore()]
    //public bool protocolVersionSpecified
    //{
    //    get
    //    {
    //        return this.protocolVersionFieldSpecified;
    //    }
    //    set
    //    {
    //        this.protocolVersionFieldSpecified = value;
    //    }
    //}
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategory("code")]
[System.Xml.Serialization.XmlType(AnonymousType = true)]
public record configurationAdd
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private string keyField;

    private string valueField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string key
    {
        get
        {
            return this.keyField;
        }
        set
        {
            this.keyField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute()]
    public string value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

