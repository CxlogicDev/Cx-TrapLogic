﻿using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;

using System.Linq;

namespace Cx_TrapConsole;

[CxConsoleInfo("build", typeof(BuildProcess), CxRegisterTypes.Preview, "Helps Build the Soultion. Also used as a Training process to understand Console Pipelining ")]
internal class BuildProcess : ConsoleBaseProcess
{
    string ContentRootPath { get; }

    public BuildProcess(CxCommandService CxProcess, IConfiguration config, Microsoft.Extensions.Hosting.IHostEnvironment env) : 
        base(CxProcess, config)     
    {
        ContentRootPath = env.ContentRootPath;
        
        ////Override the default 
        //this.config_ProcessActionHelpInfoOptions = this._config_ProcessActionHelpInfoOptions;

        ////Override the default
        //this.config_TitleLineOptions = ops =>
        //{
        //    ops.Title = "Soultion Build Console ";

        //};//this._config_TitleLineOptions

    }

    [CxConsoleAction("tree", "Will discover the build tree and output it to the base directory or the solution", true, CxRegisterTypes.Preview)]
    [CxConsoleActionArg("v", CxConsoleActionArgAttribute.arg_Types.Switch, "Shows the previous tree or build the tree and shows it to the screen.", true, false)]
    public async Task DiscoverSolutionTree()
    {
        //Need to start at a definde spot and do a search through directory;
        
        var start_Dir_Parts = ContentRootPath.Split(Path.DirectorySeparatorChar);

        int ct = 2;
        if (start_Dir_Parts.Contains("bin"))
        {
            var (x, i) = start_Dir_Parts.Select((x, i) => (x, i)).FirstOrDefault(f => f.x.ToLower().Equals("bin"));
            //start_Dir_Parts.Contains("bin") ? 3 :
            if (x != null)
                ct += start_Dir_Parts.Length - i;

        }
                
        start_Dir_Parts = start_Dir_Parts[0..(start_Dir_Parts.Length - ct)];

        var start_Dir =  //<< just 2 dir behind: Make sure you stick to the pattern when Creating new Projects
            Path.Combine(start_Dir_Parts);

        var Solution_Dir = Path.Combine(start_Dir_Parts[0..^1]);

        const string CxTreeName = "Cx-BuildTree.json";

        string CxTreeRootPath = Path.Combine(Solution_Dir, CxTreeName);

        WriteOutput_Service.write_Lines(3, $"Solution Path: {Solution_Dir}");
        WriteOutput_Service.write_Lines(3, $"Starting Path: {start_Dir}");
        WriteOutput_Service.write_Lines(3, $"CxTreeRoot Path: {CxTreeRootPath}");

        Build_Tree? _tree = null;

        if (File.Exists(CxTreeRootPath))
        {
            throw new NotImplementedException("I now have A json tree file I Need to Finsih this Action"); 
            //using var file_Stream = File.Open(CxTreeRootPath, FileMode.Open);
            //System.Text.Json.JsonDocument jDoc = System.Text.Json.JsonDocument.Parse(file_Stream);
            //string jsonString = await File.ReadAllTextAsync(CxTreeRootPath);
            //_tree = jsonString.FromJson<Build_Tree>();

        }

        _tree = new Build_Tree(start_Dir);

        _tree.Check_Versions();

        WriteOutput_Service.write_Lines();

        _tree.update_Publish_Order();

        var maxValues = _tree.maxProj_FieldLengths();

        foreach (var item in _tree.ordered_Branchs)
        {
            WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"Project", item.Proj_Name, maxValues.max_name)}; {displayVal("Version", item.Proj_Version, maxValues.max_Version)}");
            if (item.References.Count > 0)
                foreach (var _ref in item.References)
                {
                    throw new NotImplementedException("I now have references to display check and compleate this for display ref");
                    //WriteOutput_Service.write_Lines("Project".Length + 5, $">> Reference: {_ref.name}; Version: {_ref.version}");
                }

            WriteOutput_Service.write_Lines();
        }

        string displayVal(string Label, string? value, int maxLength) =>
            //{label}: {value}{space(maxLength - value.Length)}
            $"{Label}: {value??""}{new string(' ', maxLength - (value??"").Length)}";

        //display
        /*
        foreach (var tree in tree_view.Where(w => !w.EndsWith("Cx-TrapConsole.csproj", StringComparison.OrdinalIgnoreCase) ))
        {
            WriteOutput_Service.write_Lines($"File: {tree}");
            System.Xml.XmlDocument doc = new();

            doc.Load(tree);

            var ProjectNode = doc["Project"];//["PropertyGroup"];
            if (ProjectNode == null)
                continue;

            var PropertyGroup = ProjectNode["PropertyGroup"];
            if (PropertyGroup == null)
                continue;

            var versionNode = PropertyGroup["Version"];

            bool hasVersion = versionNode != null;

            versionNode = versionNode ?? doc.CreateElement("Version");
           

            //if(!hasVersion)
            //{               
            //    versionNode.InnerText = baseVersion;
            //    PropertyGroup.AppendChild(versionNode);
            //    doc.Save(tree);
            //}

            WriteOutput_Service.write_Lines(doc.OuterXml.Split('\n'));
        }
        //*/

        //return Task.CompletedTask;
    }

    /* !!!!!! ToDo: Add in a call that will Set a project to build in full publlish. !!!!!!
        -- Set Project refs to package refs
        -- Set Build order should be last or before project that use it but should never happen. 
    */

    /*/Searches the tree for .csproj Files
    List<string> tree_view { get; set; }
    void Dir_Search(string dir_to_Search)
    {
        if (tree_view == null)
            tree_view = new();

        foreach (var item in Directory.GetFiles(dir_to_Search, "*.csproj"))
            tree_view.Add(item);

        foreach (var item in Directory.GetDirectories(dir_to_Search))
            Dir_Search(item);
    }
    //*/
    
    /*
    public override Task ProcessAction(CancellationToken cancellationToken)
    {
        Console.WriteLine($"WindowBuffer-Width: {Console.BufferWidth}; WindowBuffer-Height: {Console.BufferHeight}");

        return Task.CompletedTask;
    }
    //*/
}

public record Build_Tree
{
    Dictionary<string, Tree_Branch> _Branches { get; set; } = new();

    public Build_Tree() { }

    /// <summary>
    /// Builds the tree using the Root Path
    /// </summary>
    /// <param name="root_Path"></param>
    public Build_Tree(string root_Path)
    {
        const string exclude = "Cx-TrapConsole.csproj";

        if (Directory.Exists(root_Path))
        {
            var Dir_Search_Results = Dir_Search(root_Path).Where(w => !w.EndsWith(exclude, StringComparison.OrdinalIgnoreCase));
            foreach (var tb in Dir_Search_Results.Select(s => new Tree_Branch(s)))
            {
                if (tb?.Proj_Name != null)
                    _Branches.Add(tb.Proj_Name, tb);
            }
        }
    }

    //void temp(IEnumerable<string> Branch_Paths)
    //{
    //    foreach (var tb in Branch_Paths.Select(s => new Tree_Branch(s)))
    //    {
    //        if (tb?.Proj_Name != null)
    //            _Branches.Add(tb.Proj_Name, tb);
    //    }
    //}

    /// <summary>
    /// A cached values of update_Publish_Order(); Note: Must Call update_Publish_Order() first
    /// </summary>
    public Tree_Branch[] ordered_Branchs { get; private set;  } = new Tree_Branch[0];

    /// <summary>
    /// Attches the build order for any active projects
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Tree_Branch[] update_Publish_Order()
    {        
        Dictionary<int, Tree_Branch> Branch_Order = new();

        //Pull All Trees that have no References
        var No_Refs = _Branches
            .Where(w => w.Value.References.Count == 0);

        int ct = 0, max_No_Refs = 0;
        foreach (var obj in No_Refs)
        {
            _Branches[obj.Key].Publish_Order = 1;
            Branch_Order[++ct] = obj.Value;
        }

        max_No_Refs = Branch_Order.Select(s => s.Key).Max();

        //Pull All that have only PackageReferences
        var Package_Ref = _Branches
            .Where(w => w.Value.References.Count > 0 && w.Value.References.All(a => a.referenceType == Tree_Branch_Referenece.PackageReferenceName));

        List<Tree_Branch> order = new List<Tree_Branch>();

        foreach (var obj in Package_Ref)
        {
            if(order.Count == 0)
            {
                order.Add(obj.Value);
                continue;
            }

            //all Ref for the current Obj
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            string[]? allRefName = obj.Value.References.Select(s => s.name).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

            //Get the max index to figure out where to upload
            var idx = order.Where(o => allRefName.Contains(o.Proj_Name)).Select((v, i) => i).Max();

            order.Insert(idx + 1, obj.Value);
            throw new NotImplementedException("I now have Project that have reference. I can now finish this method.");
        }

        //Ignore any that have Project References

        ordered_Branchs =  Branch_Order.OrderBy(o => o.Key).Select(s => s.Value).ToArray();

        return ordered_Branchs;
    }

    /// <summary>
    /// Will run through all projects and make sure they have a starting version number
    /// </summary>    
    public void Check_Versions()
    {
        foreach (var key in _Branches.Keys)
            if (_Branches[key].Proj_Version.hasNoCharacters())
                _Branches[key].IncreseVersion();
    }

    /// <summary>
    /// Seraches for Braches from the root.
    /// </summary>
    /// <param name="dir_to_Search">The root Path To Search</param>
    /// <returns></returns>
    string[] Dir_Search(string root_dir_to_Search)
    {
        IList<string> tree_view = new List<string>();
        Recursion(root_dir_to_Search);

        void Recursion(string dir_to_Search)
        {
            foreach (var item in Directory.GetFiles(dir_to_Search, "*.csproj"))
                tree_view.Add(item);

            foreach (var item in Directory.GetDirectories(dir_to_Search))
                Recursion(item);            
        }

        return tree_view.Distinct().ToArray();
    }

    /// <summary>
    /// Gets the Max Lengths values from the Tree_Branch values. maxProjValueLengths() called if order not set
    /// </summary>
    public (bool isValid, int max_name, int max_Namespace, int max_Version, int max_Path, int max_Framework) maxProj_FieldLengths()
    {
        (bool isValid, int max_name, int max_Namespace, int max_Version, int max_Path, int max_Framework) result = 
            (false, 0, 0, 0, 0, 0);

        if (ordered_Branchs.Length > 0 || (!(ordered_Branchs.Length > 0) && update_Publish_Order().Length > 0))
        {
            result.max_name = ordered_Branchs.Select(s => (s.Proj_Name?.Length ?? 0)).Max();
            result.max_Namespace = ordered_Branchs.Select(s => (s.Proj_Namespace?.Length ?? 0)).Max();
            result.max_Version = ordered_Branchs.Select(s => (s.Proj_Version?.Length ?? 0)).Max();
            result.max_Path = ordered_Branchs.Select(s => (s.Proj_Path?.Length ?? 0)).Max();
            result.max_Framework = ordered_Branchs.Select(s => (s.Proj_Framework?.Length ?? 0)).Max();
            result.isValid = true;
        }

        return result;
    }
}

public record Tree_Branch
{
    public Tree_Branch() { }

    public Tree_Branch(string Project_Path)
    {
        if (Project_Path.hasNoCharacters())
            throw new ArgumentException("The Project_Path is missing");

        if (!File.Exists(Project_Path))
            throw new FileNotFoundException($"The file: [{Project_Path ?? "null"}] was not found");

        Proj_Path = Project_Path;
        System.Xml.XmlDocument doc = new();
        doc.Load(Proj_Path);
        Load_Properties(doc);
        Load_References(doc);        
    }

    public string? Proj_Path { get; set; }
    public string? Proj_Name { get; set; }
    public string? Proj_Namespace { get; set; }
    public string? Proj_Version {  get; set; }
    public string? Proj_Framework { get; set; }

    public bool Publish { get; set; }
    public int Publish_Order { get; set; }

    void Load_Properties(System.Xml.XmlDocument doc)
    {   
        var ProjectNode = doc["Project"];//["PropertyGroup"];
        if (ProjectNode == null)
            return;

        var PropertyGroup = ProjectNode["PropertyGroup"];
        if (PropertyGroup == null)
            return;

        Proj_Version = PropertyGroup["Version"]?.InnerText ?? string.Empty;
        Proj_Framework = PropertyGroup["TargetFramework"]?.InnerText ?? string.Empty;
        Proj_Name = Proj_Path?.Split(Path.DirectorySeparatorChar).Last().Split('.')[0] ?? throw new ArgumentNullException(nameof(Proj_Path));
        Proj_Namespace = PropertyGroup["RootNamespace"]?.InnerText ?? string.Empty;
    }

    public List<Tree_Branch_Referenece> References { get; set; } = new();
  
    void Load_References(System.Xml.XmlDocument doc)
    {
        foreach(System.Xml.XmlElement node in doc.GetElementsByTagName("ItemGroup"))
        {
            var PackageRefs = node.SelectNodes(Tree_Branch_Referenece.PackageReferenceName);
            if(PackageRefs?.Count > 0)
                foreach (System.Xml.XmlElement refNode in PackageRefs)
                    References.Add(Tree_Branch_Referenece.PackageReference(refNode.GetAttribute("Include"), refNode.GetAttribute("Version")));

            var ProjectRefs = node.SelectNodes(Tree_Branch_Referenece.ProjectReferenceName);
            if (ProjectRefs?.Count > 0)
                foreach (System.Xml.XmlElement projNode in ProjectRefs)
                    References.Add(Tree_Branch_Referenece.ProjectReference(projNode.GetAttribute("Include")));
        }
    }

    internal void IncreseVersion()
    {
        if (!File.Exists(Proj_Path))
            throw new FileNotFoundException($"Could not find .csproj: {Proj_Path}");

        const string baseVersion = "1.0.0.0";
        const string VersionPropName = "Version";

        System.Xml.XmlDocument doc = new();
        doc.Load(Proj_Path);

        var ProjectNode = doc["Project"];
        if (ProjectNode == null)
            throw new FileLoadException($"Not a valid .csproj file: {Proj_Path}; Missing [Project] Section");

        var PropertyGroup = ProjectNode["PropertyGroup"];
        if (PropertyGroup == null)
            throw new FileLoadException($"Not a valid .csproj file: {Proj_Path}; Missing [PropertyGroup] Section");

        var PropertyGroup_Version = PropertyGroup[VersionPropName];

        if(PropertyGroup_Version != null && Proj_Version.hasNoCharacters())
        {
            Proj_Version = PropertyGroup_Version.InnerText;
        }

        //Increase Posibly 
        var splitVersion =  (Proj_Version != null && Proj_Version.hasCharacters()? Proj_Version : baseVersion).Split('.');
        var idx = splitVersion.Select((x, i) => i).Max();

        splitVersion[idx] = $"{((splitVersion[idx].toInt32() ?? 0) + 1)}";

        Proj_Version = string.Join(".", splitVersion);

        if (PropertyGroup_Version != null)
        {
            PropertyGroup_Version.InnerText = Proj_Version;
        }
        else
        {
            PropertyGroup_Version = doc.CreateElement(VersionPropName);
            PropertyGroup_Version.InnerText = Proj_Version;
            PropertyGroup.AppendChild(PropertyGroup_Version);
        }
            
        doc.Save(Proj_Path);
    }
}

public record Tree_Branch_Referenece
{

    public const string PackageReferenceName = nameof(PackageReference);
    public const string ProjectReferenceName = nameof(ProjectReference);

    public static Tree_Branch_Referenece PackageReference(string Name, string Version) => new() { name = Name, version = Version, referenceType = PackageReferenceName };

    public static Tree_Branch_Referenece ProjectReference(string Name) => new() { name = Name, referenceType = ProjectReferenceName };

    //public enum ReferenceType { Dll, Package, Project  }
    //public ReferenceType referenceType { get; set; }
    public string? referenceType { get; set; }
    public string? name { get; set; }
    public string? version { get; set; }
}


