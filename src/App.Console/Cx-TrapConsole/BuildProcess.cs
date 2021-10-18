using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cx_TrapConsole;

[CxConsoleInfo("project", typeof(BuildProcess), CxRegisterTypes.Preview, "Helps Build the Soultion. Also used as a Training process to understand Console Pipelining ")]
internal class BuildProcess : ConsoleBaseProcess
{
    Content_Path_Structure _contentRoot { get; }
    

    public BuildProcess(CxCommandService CxProcess, IConfiguration config, Microsoft.Extensions.Hosting.IHostEnvironment env) : 
        base(CxProcess, config)     
    {   
        _contentRoot = new Content_Path_Structure(env.ContentRootPath);
        ////Override the default 
        //this.config_ProcessActionHelpInfoOptions = this._config_ProcessActionHelpInfoOptions;

        ////Override the default
        //this.config_TitleLineOptions = ops =>
        //{
        //    ops.Title = "Soultion Build Console ";

        //};//this._config_TitleLineOptions

    }

    //protected override bool isCanceled { get { return base.isCanceled; } set { base.isCanceled = value; } }

    [CxConsoleAction("tree", "Will discover the build tree and output it to the base directory or the solution", true, CxRegisterTypes.Register)]
    [CxConsoleActionArg("v", CxConsoleActionArgAttribute.arg_Types.Switch, "Shows the previous tree or build the tree and shows it to the screen.", true, false)]
    [CxConsoleActionArg("r", CxConsoleActionArgAttribute.arg_Types.Switch, "Will delete the cached .json tree file. nagates", true, false)]
    public Task DiscoverSolutionTree(CancellationToken cancellationToken)
    {
        /*
          ToDo: handle the cancellationToken through out the method        
        */

        //WriteOutput_Service.write_Lines(3, $"Solution Path: {_contentRoot.Solution_Dir}");

        //WriteOutput_Service.write_Lines(3, $"Source Path: {_contentRoot.start_Dir}");

        Build_Tree? _tree = null;

        if(_CxCommandService.getCommandArg("-r", out var _) && File.Exists(_contentRoot.CxTreeRootPath))
            File.Delete(_contentRoot.CxTreeRootPath);//Delete the caches tree
        else if (File.Exists(_contentRoot.CxTreeRootPath) && _CxCommandService.getCommandArg("-v", out string? i_val))
        {
            /***/
            StringBuilder CxTree_data = new StringBuilder();
            using StreamReader sr = File.OpenText(_contentRoot.CxTreeRootPath);
            while (!sr.EndOfStream)
                CxTree_data.Append(sr.ReadLine());

            var fullString = CxTree_data.ToString();

            _tree = Build_Tree.LoadFrom_Json(CxTree_data.ToString());
            /*  */
            write_Output(_tree, _contentRoot.CxTreeRootPath);

            return Task.CompletedTask;
        }

        _tree = new Build_Tree(_contentRoot.start_Dir);

        _tree.Check_Versions();

        _tree.update_Publish_Order();

        string CxTree_Output = _tree.ToString();
        File.WriteAllText(_contentRoot.CxTreeRootPath, CxTree_Output);

        write_Output(_tree, _contentRoot.CxTreeRootPath);

        void write_Output(Build_Tree? _tree, string CxTreeRootPath)
        {
            WriteOutput_Service.write_Lines();

            if (_tree == null)
                return;

            var maxValues = _tree.maxProj_FieldLengths();

            WriteOutput_Service.write_Lines(3, "Build Tree ProjectS");

            foreach (var item in _tree.ordered_Branchs)
            {
                WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"{item.Publish_Order}", item.Proj_Name, maxValues.max_name)}; {displayVal("Version", item.Proj_Version, maxValues.max_Version)}; {displayVal("Build", item.Publish.ToString(), 6)}");
                if (item.References.Count > 0)
                    foreach (var _ref in item.References)
                    {
                        throw new NotImplementedException("I now have references to display check and compleate this for display ref");
                        //WriteOutput_Service.write_Lines("Project".Length + 5, $">> Reference: {_ref.name}; Version: {_ref.version}");
                    }

                WriteOutput_Service.write_Lines();
            }

            WriteOutput_Service.write_Lines(3, $"CxTreeRoot Path: {CxTreeRootPath}; File: {(File.Exists(CxTreeRootPath) ? "" : "Does Not ")}Exists");

        }

        return Task.CompletedTask;
    }

    [CxConsoleAction("build", CommandDisplayInfo.build_action_description, false, CxRegisterTypes.Preview)]
    [CxConsoleActionArg("p", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.build_arg_p__Publish_Description, true, false )]
    [CxConsoleActionArg("pa", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.build_arg_pa__Publish_Description, false, false)]
    //[CxConsoleActionArg("report", CxConsoleActionArgAttribute.arg_Types.Value, CommandDisplayInfo.build_arg_report__Publish_Description, false, false)]
    public async Task BuildSolutionTree(CancellationToken cancellationToken)
    {
        /*
         
         */
        
        if (!_contentRoot.hasCxTreeFile)
        {
            //Cannot run write the output and exit
            WriteOutput_Service.write_Lines(3, $"Build Tree file could not be found. Please Build the Tree by calling the [{_CxCommandInfo?.name ?? "--null--"} tree ] command");
            return;
        }

        bool publish = _CxCommandService.getCommandArg("-p", out var _);

        bool publish_All = _CxCommandService.getCommandArg("-pa", out var _);

        Build_Tree _tree = _contentRoot.get_CurrentTree();

        var maxValues = _tree.maxProj_FieldLengths();

        if (publish_All || publish)
        {
            Dictionary<string, bool> Publis_Reference_To_Me = new();

            var build_List = _tree.update_Publish_Order().Where(w => publish_All || w.Publish).ToArray();

            if(build_List.Length == 0)
            {
                WriteOutput_Service.write_Lines(3, 
                    "No Projects to build. ",
                    "Set the project/s you want built to true in the tree",
                    "or use Arg -pa to build all projects");
                return;
            }

            foreach (var item in build_List)
            {
                if (item.Proj_Name == null)
                    continue;
                else if (publish && !item.Publish)//|| Publis_Reference_To_Me.any(a => item.ref.any(ia => a))) //Note Check to see if a higher ref project was updated 
                    continue;

                Publis_Reference_To_Me[item.Proj_Name] = true;

                string nupkg_Path_Name = $"{item.Proj_Name}.{item.Proj_Version}.nupkg";

                string nupkg_Path = Path.Combine(item.Proj_Directory ?? throw new NullReferenceException(), "bin", "release", nupkg_Path_Name);

                string Cx_nupkg_Path = Path.Combine(_contentRoot.CxTreeNupkgPath, nupkg_Path_Name);

                if(!Directory.Exists(_contentRoot.CxTreeNupkgPath))
                    Directory.CreateDirectory(_contentRoot.CxTreeNupkgPath);

                if (File.Exists(nupkg_Path))
                {
                    WriteOutput_Service.write_Lines(3, $"Deleting {nupkg_Path}");
                }

                //Quick Write
                WriteOutput_Service.write_Lines(3, $"Packing {displayVal("Project", item.Proj_Name, maxValues.max_name)} >> {nupkg_Path}");

                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new();
                //startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = item.Proj_Directory;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "dotnet.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = "pack --configuration release ";

                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using Process? exeProcess = Process.Start(startInfo);

                    if (exeProcess != null)
                        await exeProcess.WaitForExitAsync(cancellationToken);

                    if (!File.Exists(nupkg_Path))
                        throw new FileNotFoundException($"{nupkg_Path} could not be found. ");
                                        
                    WriteOutput_Service.write_Lines(3, $"Moving {nupkg_Path} >> {(File.Exists(Cx_nupkg_Path) ? "[Overwriting] " : "")}{Cx_nupkg_Path}");
                    File.Move(nupkg_Path, Cx_nupkg_Path, true);

                    //

                }
                catch (Exception ex)
                {
                    WriteOutput_Service.write_Lines(2, ex.ToString());
                    // Log error.
                    return;
                }
            }


            /*
                Need to run through the list of processed and update the version numbers.

                delete the exsting tree.

                
            */
        }


        return;

    }

    [CxConsoleAction("view", "Will show the full project tree and tell what can and cannot publish", true, CxRegisterTypes.Preview)]
    [CxConsoleActionArg("s", CxConsoleActionArgAttribute.arg_Types.Value, "Search and return only project that have the search term in the name", false, false)]
    public Task ListProjects(CancellationToken cancellationToken)
    {
        if (File.Exists(_contentRoot.CxTreeRootPath))
        {
            var _tree = new Build_Tree(_contentRoot.start_Dir);

            _tree.Check_Versions();
            
            WriteOutput_Service.write_Lines();

            if (_tree == null)
                return Task.CompletedTask;

            var maxValues = _tree.maxProj_FieldLengths();

            WriteOutput_Service.write_Lines(3, "Build Tree ProjectS");

            foreach (var item in _tree._Branches.Values)
            {
                if (item.References.Count > 0)
                    WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"Cannot Publish", item.Proj_Name, maxValues.max_name)} >> {displayVal("Version", item.Proj_Version, maxValues.max_Version)}; {displayVal("Build", item.Publish.ToString(), 6)}");
                else
                    WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"Can Publish", item.Proj_Name, maxValues.max_name + 3)} >> {displayVal("Version", item.Proj_Version, maxValues.max_Version)}; {displayVal("Build", item.Publish.ToString(), 6)}");

            }

            return Task.CompletedTask;
        }


        WriteOutput_Service.write_Lines($"The File {_contentRoot.CxTreeRootPath} fould not be found. ",
            "Please Call [project tree] first to continue. ");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a display point 
    /// </summary>
    /// <param name="Label">The label for the display point</param>
    /// <param name="value">The Value For the display point</param>
    /// <param name="maxLength">The Max Lenght the display point should be</param>
    string displayVal(string Label, string? value, int maxLength) => 
        $"{Label}: {value ?? ""}{new string(' ', maxLength - (value ?? "").Length)}";

    /* !!!!!! ToDo: Add in a call that will Set a project to build in full publlish. !!!!!!
        -- Set Project refs to package refs
        -- Set Build order should be last or before project that use it but should never happen. 
    */

    /// <summary>
    /// Used to build a Content Path structure for the project build command
    /// </summary>
    record class Content_Path_Structure
    {
        /// <summary>
        /// The Project Content Root supplied
        /// </summary>
        public string contentRootPath { get; }

        /// <summary>
        /// The start Directory Parts to build the path structure
        /// </summary>
        public string[] start_Dir_Parts {  get; }

        /// <summary>
        /// The back count from the content root path supplied
        /// </summary>
        int start_Dir_Back_ct { get; }

        /// <summary>
        /// The Source Directory
        /// </summary>
        public string start_Dir { get; }

        /// <summary>
        /// The solution Directory 
        /// </summary>
        public string Solution_Dir { get; }

        /// <summary>
        /// The Cx Tree Root Path
        /// </summary>
        public string CxTreeRootPath { get; }

        public string CxTreeNupkgPath { get; }

        public Content_Path_Structure(string ContentRoot)
        {

            if (!Directory.Exists(ContentRoot))
                throw new DirectoryNotFoundException($"The content cannot be process because the dirctory: [{ContentRoot}] does not exist!!!!");

            contentRootPath = ContentRoot;

            start_Dir_Parts = ContentRoot.Split(Path.DirectorySeparatorChar);
            
            start_Dir_Back_ct = 2;

            if (start_Dir_Parts.Contains("bin"))
            {
                //Gets the valid start path if debugging.
                var (x, i) = start_Dir_Parts.Select((x, i) => (x, i)).FirstOrDefault(f => f.x.ToLower().Equals("bin"));
                //start_Dir_Parts.Contains("bin") ? 3 :
                if (x != null)
                    start_Dir_Back_ct += start_Dir_Parts.Length - i;
            }

            start_Dir_Parts = start_Dir_Parts[0..(start_Dir_Parts.Length - start_Dir_Back_ct)];

             start_Dir =  //<< just 2 dir behind: Make sure you stick to the pattern when Creating new Projects
                Path.Combine(start_Dir_Parts);

            Solution_Dir = Path.Combine(start_Dir_Parts[0..^1]);

            if (!Directory.Exists(Path.Combine(Solution_Dir, ".Cx-Trap-Build")))
                Directory.CreateDirectory(Path.Combine(Solution_Dir, ".Cx-Trap-Build"));

            CxTreeRootPath = Path.Combine(Solution_Dir, ".Cx-Trap-Build", ".Cx-BuildTree.json");

            CxTreeNupkgPath = Path.Combine(Solution_Dir, ".Cx-Trap-packages");
        }

        /// <summary>
        /// Test to see if the tree file exists
        /// </summary>
        public bool hasCxTreeFile => File.Exists(CxTreeRootPath);

        /// <summary>
        /// gets the Content Tree object
        /// </summary>
        public Build_Tree get_CurrentTree()
        {
            StringBuilder CxTree_data = new StringBuilder();
            using StreamReader sr = File.OpenText(CxTreeRootPath);
            while (!sr.EndOfStream)
                CxTree_data.Append(sr.ReadLine());

            var fullString = CxTree_data.ToString();

            Build_Tree? _tree = Build_Tree.LoadFrom_Json(CxTree_data.ToString());

            return _tree;
        }
    }

    /// <summary>
    /// A display only type action
    /// </summary>
    static class CommandDisplayInfo
    {
        public const string build_action_description = "Will build all project in the tree.";        
        public const string build_arg_p__Publish_Description = "Publishes the build tree where Tree_Branch.Publish is true and any project that reference the one that is being published";
        public const string build_arg_pa__Publish_Description = "Publishes all of the projects in the build tree.";
        public const string build_arg_report__Publish_Description = "Writes a report of the build and publish process. supply a exsiting directory location to overide default location.";
    }


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
    internal Dictionary<string, Tree_Branch> _Branches { get; set; } = new();
    
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
