using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cx_TrapConsole;

public record Build_Tree
{
    internal Dictionary<string, Tree_Branch> _Branches { get; set; } = new();


    public Tree_Branch? this[string name] => _Branches.TryGetValue(name, out Tree_Branch? branch) ? branch : null;

    public string? rootPath { get; set; }

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
            rootPath = root_Path;

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
    public Tree_Branch[] ordered_Branchs { get; private set; } = new Tree_Branch[0];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ob_Keys"></param>
    /// <returns></returns>
    public Tree_Branch[] Filter_Ordered_Branches(IEnumerable<string> ob_Keys)
            => ordered_Branchs.Where(w => ob_Keys.Contains(w.Proj_Name)).ToArray();

    /// <summary>
    /// Set the Branch with project_Name to the supplied flag status
    /// </summary>
    /// <param name="Branch_key">Project Name</param>
    /// <param name="publish_Status_Flag">The state publish should be set too</param>
    public void set_Branch_Published(string Branch_key, bool publish_Status_Flag)
    {
        if (!_Branches[Branch_key].isNull())
            _Branches[Branch_key].Publish = publish_Status_Flag;
    }

    /// <summary>
    /// Will Save the Branch_Tree to a File or Override it with the new one;
    /// Calls >> update_Publish_Order()
    /// </summary>
    /// <param name="CxTreeRootPath"></param>
    /// <param name="call__update_Publish_Order">Set to true to re_order the publish branches</param>
    /// <exception cref="FileNotFoundException"></exception>
    public void publish_Branch_File(string CxTreeRootPath, bool call__update_Publish_Order = false)
    {
        if (CxTreeRootPath.hasNoCharacters())//|| File.Exists(CxTreeRootPath)
            return;

        //Update the new order 
        if (call__update_Publish_Order)
            update_Publish_Order();

        /**/
        string CxTree_Output = this.ToString();
        File.WriteAllText(CxTreeRootPath, CxTree_Output);

        //No file throw error do not allow to move forward
        if (!File.Exists(CxTreeRootPath))
            throw new FileNotFoundException($"The file was not found", "_contentRoot.CxTreeRootPath");
    }

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
            ct++;
            _Branches[obj.Key].Publish_Order = ct;
            Branch_Order[ct] = obj.Value;
        }

        max_No_Refs = Branch_Order.Select(s => s.Key).Max();

        //Pull All that have only PackageReferences
        var Package_Ref = _Branches
            .Where(w => w.Value.References.Count > 0 && w.Value.References.All(a => a.referenceType == Tree_Branch_Referenece.PackageReferenceName));

        List<Tree_Branch> order = new List<Tree_Branch>();

        foreach (var obj in Package_Ref)
        {


            foreach (var item in obj.Value.References)
            {
                if (!item.isNull() && item.name.hasCharacters())
                {
                    item.isLocal = !this[item.name].isNull();
                    continue;
                }

                item.isLocal = false;
            }


            if (order.Count == 0)
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

            if (idx <= 0)
                order.Add(obj.Value);
            else if (idx > 0)
                order.Insert(idx + 1, obj.Value);
            else
                throw new NotImplementedException("I now have Project that have reference. I can now finish this method.");
        }

        if (order.Count > 0)
        {
            foreach (var ord in order)
            {
                ct++;// = max_No_Refs;
                ord.Publish_Order = ct;
                Branch_Order[++ct] = ord;
            }

        }


        //Ignore any that have Project References

        ordered_Branchs = Branch_Order.OrderBy(o => o.Key).Select(s => s.Value).ToArray();

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

    /// <summary>
    /// Returns the json string that can be saved to file 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (!(ordered_Branchs.Length > 0))
            update_Publish_Order();

        return ordered_Branchs.ToJson();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json_string"></param>
    public static Build_Tree LoadFrom_Json(string json_string) =>
        LoadFrom_Branches(json_string.FromJson<Tree_Branch[]>() ?? new Tree_Branch[0]);

    /// <summary>
    /// Load a Tree by branches
    /// </summary>
    /// <param name="branches">The branches to load</param>
    /// <exception cref="ArgumentNullException">if a branch is null or the name is null</exception>
    public static Build_Tree LoadFrom_Branches(IEnumerable<Tree_Branch> branches)
    {
        Build_Tree result = new Build_Tree();

        if (branches?.Count() > 0)
            foreach (var tb in branches.Distinct())
                result._Branches.Add(tb.Proj_Name ?? throw new ArgumentNullException(nameof(tb.Proj_Name)), tb);

        return result;
    }

    /// <summary>
    /// Loads from root path and factors in published_Branch_File
    /// </summary>
    /// <param name="json_Publish_File_Path"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static Build_Tree LoadFrom_File(string base_Root_Path, string? json_Publish_File_Path = null)
    {
        if (base_Root_Path.hasNoCharacters() || !Directory.Exists(base_Root_Path))
            throw new DirectoryNotFoundException("The Tree Directory Could not be found");

        Build_Tree? b_Tree_Full = new Build_Tree(base_Root_Path);

        if (json_Publish_File_Path.hasCharacters() && File.Exists(json_Publish_File_Path) && !b_Tree_Full.isNull())
        {
            StringBuilder CxTree_data = new StringBuilder();
            using StreamReader sr = File.OpenText(json_Publish_File_Path);
            while (!sr.EndOfStream)
                CxTree_data.Append(sr.ReadLine());

            Build_Tree temp = LoadFrom_Json(CxTree_data.ToString());

            if (!temp.isNull())
                foreach (var proj_key in temp._Branches.Keys)
                    if (temp._Branches.TryGetValue(proj_key, out Tree_Branch? val) && !val.isNull())
                        b_Tree_Full._Branches[proj_key] = val;
        }

        return b_Tree_Full;
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
        string[] Proj_Path_Parts = Proj_Path.Split(Path.DirectorySeparatorChar);
        Proj_Directory = Path.Combine(Proj_Path_Parts[0..^1]);

        System.Xml.XmlDocument doc = new();
        doc.Load(Proj_Path);
        Load_Properties(doc);
        Load_References(doc);
    }

    public string? Proj_Path { get; set; }

    public string? Proj_Directory { get; set; }
    public string? Proj_Name { get; set; }
    public string? Proj_Namespace { get; set; }
    public string? Proj_Version { get; set; }
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
        foreach (System.Xml.XmlElement node in doc.GetElementsByTagName("ItemGroup"))
        {
            var PackageRefs = node.SelectNodes(Tree_Branch_Referenece.PackageReferenceName);
            if (PackageRefs?.Count > 0)
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

        if (PropertyGroup_Version != null && Proj_Version.hasNoCharacters())
        {
            Proj_Version = PropertyGroup_Version.InnerText;
        }

        //Increase Posibly 
        var splitVersion = (Proj_Version != null && Proj_Version.hasCharacters() ? Proj_Version : baseVersion).Split('.');
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

    internal void UpdateReferences(IEnumerable<(Tree_Branch branch, string Cx_nupkg_Path)> Ref_Branches)
    {

        if (!File.Exists(Proj_Path))
            throw new FileNotFoundException($"Could not find .csproj: {Proj_Path}");

        System.Xml.XmlDocument doc = new();
        
        doc.Load(Proj_Path);

        var ProjectNode = doc["Project"];
        
        if (ProjectNode == null)
            throw new FileLoadException($"Not a valid .csproj file: {Proj_Path}; Missing [Project] Section");

        var ItemGroups = ProjectNode.SelectNodes("ItemGroup");

        if (!ItemGroups.isNull())
            foreach (System.Xml.XmlElement ItemGroup in ItemGroups)
            {
                
                var PackageReferences = ItemGroup.SelectNodes("PackageReference");

                if (!PackageReferences.isNull())
                    foreach (System.Xml.XmlElement PackageReference in PackageReferences)
                    {
                        string Include = PackageReference.GetAttribute("Include");

                        var Ref_Branch = Ref_Branches.FirstOrDefault(f => f.branch.Proj_Name?.Equals(Include) ?? false);

                        if (Ref_Branch.isNull())
                            continue;

                        string Version = PackageReference.GetAttribute("Version");

                        if (!Version.isNull() && File.Exists(Ref_Branch.Cx_nupkg_Path))
                            PackageReference.SetAttribute("Version", Ref_Branch.branch.Proj_Version);
                    }
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
    internal bool isLocal { get; set; } = false;
}