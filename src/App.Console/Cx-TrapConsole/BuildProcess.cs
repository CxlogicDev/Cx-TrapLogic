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

    [CxConsoleAction("tree", CommandDisplayInfo.tree_action_description, true, CxRegisterTypes.Register)]
    [CxConsoleActionArg("p", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.tree_arg_p__Publish_Description, true, false)]
    [CxConsoleActionArg("r", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.tree_arg_r__Publish_Description, true, false)]
    [CxConsoleActionArg("f", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.tree_arg_f__Publish_Description, true, false)]
    public Task DiscoverSolutionTree(CancellationToken cancellationToken)
    {       
        if (_CxCommandService.getCommandArg("-r", out var _) && File.Exists(_contentRoot.CxTreeRootPath))
            File.Delete(_contentRoot.CxTreeRootPath);//Delete the caches tree

        if (!File.Exists(_contentRoot.CxTreeRootPath))
        {
            var tree_1 = Build_Tree.LoadFrom_File(_contentRoot.start_Dir);

            tree_1.Check_Versions();

            tree_1.publish_Branch_File(_contentRoot.CxTreeRootPath, true);
        }

        //If the File exists Use it. if not Then mov into create it
        Build_Tree? _tree = Build_Tree.LoadFrom_File(_contentRoot.start_Dir, _contentRoot.CxTreeRootPath);

        if (internal_Publish(_tree))
            _tree.publish_Branch_File(_contentRoot.CxTreeRootPath, true);

        //Pull the list of appoves Projects
        var Project_List = internal_Filter(_tree);

        write_Output(_tree, Project_List, _contentRoot.CxTreeRootPath);

        return Task.CompletedTask;

        void write_Output(Build_Tree? _tree, IEnumerable<string> ob_Keys, string CxTreeRootPath)
        {
            WriteOutput_Service.write_Lines();

            if (_tree == null)
                return;

            var maxValues = _tree.maxProj_FieldLengths();

            WriteOutput_Service.write_Lines(3, "Build Tree Projects");

            foreach (var item in _tree.Filter_Ordered_Branches(ob_Keys))
            {
                WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"{item.Publish_Order}", item.Proj_Name, maxValues.max_name)}; {displayVal("Dev-Version", item.Proj_Version, maxValues.max_Version)}; {displayVal("Build", item.Publish.ToString(), 6)}");
                if (item.References.Count > 0)
                    foreach (var _ref in item.References.Where(w => w.isLocal))
                    {
                        WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(10)}>> Reference: {_ref.name}; Version: {_ref.version}");
                    }

                WriteOutput_Service.write_Lines();
            }

            WriteOutput_Service.write_Lines(3, $"CxTreeRoot Path: {CxTreeRootPath}; File: {(File.Exists(CxTreeRootPath) ? "" : "Does Not ")}Exists");
        }
    }

    [CxConsoleAction("build", CommandDisplayInfo.build_action_description, false, CxRegisterTypes.Preview)]
    [CxConsoleActionArg("p", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.build_arg_p__Publish_Description, true, false)]
    [CxConsoleActionArg("pa", CxConsoleActionArgAttribute.arg_Types.Switch, CommandDisplayInfo.build_arg_pa__Publish_Description, false, false)]
    //[CxConsoleActionArg("report", CxConsoleActionArgAttribute.arg_Types.Value, CommandDisplayInfo.build_arg_report__Publish_Description, false, false)]
    public async Task BuildSolutionTree(CancellationToken cancellationToken)
    {

        if (!_contentRoot.hasCxTreeFile)
        {
            //Cannot run write the output and exit
            WriteOutput_Service.write_Lines(3, $"Build Tree file could not be found. Please Build the Tree by calling the [{_CxCommandInfo?.name ?? "--null--"} tree ] command");
            return;
        }

        bool publish = _CxCommandService.getCommandArg("-p", out var _);

        bool publish_All = _CxCommandService.getCommandArg("-pa", out var _);

        Build_Tree _tree = Build_Tree.LoadFrom_File(_contentRoot.start_Dir, _contentRoot.CxTreeRootPath);

        var maxValues = _tree.maxProj_FieldLengths();

        if (publish_All || publish)
        {
            NuGetconfiguration nuGet = NuGetconfiguration.Build_NuGet_Config(_contentRoot.Solution_Dir, _contentRoot.CxTreeNupkgPath);

            Dictionary<string, (Tree_Branch branch, string Cx_nupkg_Path)> Publis_Reference_To_Me = new();

            var build_List = _tree.update_Publish_Order().OrderBy(o => o.Publish_Order);//.Where(w => publish_All || w.Publish).ToArray();

            if (build_List.Where(w => publish_All || w.Publish).Count() == 0)
            {
                WriteOutput_Service.write_Lines(3,
                    "No Projects to build. ",
                    "Set the project/s you want built to true in the tree",
                    "or use Arg -pa to build all projects");
                return;
            }

            foreach (var item in build_List)
            {
                bool hasMatchedRef = item.References.Where(w => w.isLocal).Any(a => a.name.hasCharacters() && Publis_Reference_To_Me.ContainsKey(a.name));

                if (item.Proj_Name == null)
                    continue;
                else if (publish && !item.Publish && !hasMatchedRef) //Note Check to see if a higher ref project was updated 
                    continue;

                string nupkg_Path_Name = $"{item.Proj_Name}.{item.Proj_Version}.nupkg";

                string nupkg_Path = Path.Combine(item.Proj_Directory ?? throw new NullReferenceException(), "bin", "release", nupkg_Path_Name);

                string Cx_nupkg_Path = Path.Combine(_contentRoot.CxTreeNupkgPath, nupkg_Path_Name);

                Publis_Reference_To_Me[item.Proj_Name] = (item, Cx_nupkg_Path);

                if (hasMatchedRef)//Here is where the references get updated
                    _tree[item.Proj_Name]?.UpdateReferences(Publis_Reference_To_Me.Values);

                if (!Directory.Exists(_contentRoot.CxTreeNupkgPath))
                    Directory.CreateDirectory(_contentRoot.CxTreeNupkgPath);

                if (File.Exists(nupkg_Path))
                    WriteOutput_Service.write_Lines(3, $"Deleting {nupkg_Path}");

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
                }
                catch (Exception ex)
                {
                    WriteOutput_Service.write_Lines(2, ex.ToString());
                    // Log error.
                    return;
                }
            }

            foreach (var i in Publis_Reference_To_Me)
            {
                try
                {
                    //Need to run through the list of processed and update the version numbers.
                    _tree[i.Key]?.IncreseVersion();
                }
                catch (Exception Ex)
                {
                    WriteOutput_Service.write_Lines(3, Ex.Message);
                }
            }

            //Delete the caches tree
            File.Delete(_contentRoot.CxTreeRootPath);

        }

        return;
    }

    /// <summary>
    /// List all Projects under the solution that can be built and the status of them all
    /// </summary>
    /// <param name="cancellationToken">The token to cancle the action</param>
    [CxConsoleAction("list", CommandDisplayInfo.view_action_description, true, CxRegisterTypes.Preview)]
    [CxConsoleActionArg("s", CxConsoleActionArgAttribute.arg_Types.Value, CommandDisplayInfo.view_arg_s__Publish_Description, true, false)]
    public Task ListProjects(CancellationToken cancellationToken)
    {
        
        //ToDo: Reconsider how the tree is built if a View already exists
        var _tree = new Build_Tree(_contentRoot.start_Dir);

        _tree.Check_Versions();

        WriteOutput_Service.write_Lines();

        if (_tree == null)
            return Task.CompletedTask;

        var maxValues = _tree.maxProj_FieldLengths();

        WriteOutput_Service.write_Lines(3, "Build Tree Projects");

        var hasSearch = _CxCommandService.getCommandArg("-s", out string? Search_V);

        int[]? wildCardIndex = Search_V?.Select((v, i) => (v, i)).Where(w => w.v == '*').Select(s => s.i).Distinct().ToArray();

        bool hasStart_WildCard = false;
        bool hasEnd_WildCard = false;

        //Null Ref Check Logic Interfered so wrote code different way
        if (wildCardIndex?.Length > 0 && Search_V != null)
        {
            if (wildCardIndex.Any(a => a > 0 && a < Search_V.Length - 1) || wildCardIndex.Length > 1)
            {
                WriteOutput_Service.write_Lines(3, $"The Search Arg: [-s {Search_V}] Is Invalid",
                  wildCardIndex.Length > 1 ? $"Error: {wildCardIndex.Length} wildcards when only 1 allowed" : "wildcards can only begin or end with a wildcard: *");
                return Task.CompletedTask;
            }

            hasStart_WildCard = wildCardIndex.Contains(0);
            hasEnd_WildCard = wildCardIndex.Contains(Search_V.Length - 1);
        }

        List<string> st = new List<string>();

        if (hasStart_WildCard && Search_V != null)
        {
            var part = Search_V?.Substring(1) ?? "";
            st.AddRange(_tree._Branches.Keys.Where(w => w.EndsWith(part, StringComparison.OrdinalIgnoreCase)));
        }
        else if (hasEnd_WildCard && Search_V != null)
        {
            var part = new string(Search_V.AsSpan(0, Search_V.Length - 1));
            st.AddRange(_tree._Branches.Keys.Where(w => w.StartsWith(part, StringComparison.OrdinalIgnoreCase)));
        }
        else if (Search_V == null)
        {
            st.AddRange(_tree._Branches.Keys);
        }

        foreach (var key in st)
        {
            if (!_tree._Branches.TryGetValue(key, out Tree_Branch? item) || item == null)
                continue;

            if (item?.References.Count > 0 && item.References.Any(a => a.referenceType == "ProjectReference"))
            {
                WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"Cannot Publish", item.Proj_Name, maxValues.max_name)} >> {displayVal("Version", item.Proj_Version, maxValues.max_Version)}; ");//ToDo: >>> {displayVal("Publish", "No", 6)}
                continue;
            }

            WriteOutput_Service.write_Lines(5, $"{WriteOutput_Service.space(5)}{displayVal($"   Can Publish", item?.Proj_Name, maxValues.max_name)} >> {displayVal("Version", item?.Proj_Version, maxValues.max_Version)}; "); //Todo: >>> {displayVal("Publish", item.Publish ? "Yes" : "No", 6)}
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a list of all of the Searchable projects
    /// </summary>
    /// <param name="_tree">The tree to process for</param>
    IList<string> internal_Filter(Build_Tree _tree)
    {
        List<string> st = new List<string>();

        if (_tree == null)
            return st;

        var hasSearch = _CxCommandService.getCommandArg("-f", out string? filter_V) && filter_V.hasCharacters();

        if (!hasSearch && _tree.ordered_Branchs is not null && _tree.ordered_Branchs.Length > 0)
        {
            List<string> temp = new(_tree.ordered_Branchs.Where(w => w.Proj_Name.hasCharacters()).Select(s => s.Proj_Name ?? ""));
            st.AddRange(temp);
            
            return st;
        }

        int[]? wildCardIndex = filter_V?.Select((v, i) => (v, i)).Where(w => w.v == '*').Select(s => s.i).Distinct().ToArray();

        bool hasStart_WildCard = false;
        bool hasEnd_WildCard = false;

        //Null Ref Check Logic Interfered so wrote code different way
        if (wildCardIndex?.Length > 0 && filter_V != null)
        {
            if (wildCardIndex.Any(a => a > 0 && a < filter_V.Length - 1) || wildCardIndex.Length > 1)
            {
                WriteOutput_Service.write_Lines(3, $"The Search Arg: [-s {filter_V}] Is Invalid",
                  wildCardIndex.Length > 1 ? $"Error: {wildCardIndex.Length} wildcards when only 1 allowed" : "wildcards can only begin or end with a wildcard: *");
                return new List<string>();
            }

            hasStart_WildCard = wildCardIndex.Contains(0);
            hasEnd_WildCard = wildCardIndex.Contains(filter_V.Length - 1);
        }

        if (hasStart_WildCard && filter_V != null)
        {
            var part = filter_V?.Substring(1) ?? "";
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            st.AddRange(_tree._Branches.Keys.Where(w => w.EndsWith(part, StringComparison.OrdinalIgnoreCase)));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        else if (hasEnd_WildCard && filter_V != null)
        {
            var part = new string(filter_V.AsSpan(0, filter_V.Length - 1));
#pragma warning disable CS8602
            st.AddRange(_tree._Branches.Keys.Where(w => w.StartsWith(part, StringComparison.OrdinalIgnoreCase)));
#pragma warning restore CS8602
        }
        else if (filter_V == null)
        {
#pragma warning disable CS8602
            st.AddRange(_tree._Branches.Keys);
#pragma warning restore CS8602
        }

        return st;
    }

    /// <summary>
    /// Test to see if the process tree is able to set the publish
    /// </summary>
    /// <param name="_tree">The tree to process for</param>
    bool internal_Publish(Build_Tree _tree)
    {        
        IList<string> Proj_List = internal_Filter(_tree);

        bool canPublish = false;

        if (_CxCommandService.getCommandArg("-p", out string? Publish_V))
        {
            canPublish = true;

            var Publish_flag_V = Publish_V.hasCharacters() && Publish_V.ToLower() switch
            {
                "set" => true,
                "true" => true,
                "yes" => true,
                "clear" => false,
                "false" => false,
                "no" => false,
                "on" => true,
                "off" => false,
                _ => errorCondion()
            };

            bool errorCondion()
            {
                canPublish = false;
                WriteOutput_Service.write_Lines(3, "The Publish Arg Value: -p is not valid. Please Use {true, false, yes, no, set, or clear}. ");
                return false;
            };

            if (canPublish)
                foreach (var proj_key in _tree._Branches.Keys)
                {
                    if (proj_key.isNull() || proj_key.hasNoCharacters() || !Proj_List.Contains(proj_key))
                        continue;

                    //Set Branch as active
                    _tree.set_Branch_Published(proj_key, Publish_flag_V);
                }
        }

        return canPublish;
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

    }

    /// <summary>
    /// A display only type action
    /// </summary>
    static class CommandDisplayInfo
    {
        public const string tree_action_description = "Will discover the build tree and output it to the base directory or the solution";
        public const string tree_arg_p__Publish_Description = "Sets the Publish flag for all projects in the path.";
        public const string tree_arg_r__Publish_Description = "Will delete the cached .json tree file so that it can be rebuilt with any newly added projects to the build list.";
        public const string tree_arg_f__Publish_Description = "Will filter and show projects by the filtered value supplied. Use [{filter search}] [{filter search}:{ (true:1 | false:0) | (yes:1 | no:0) | (set:1 | clear:0) | (on:1 | Off:0)}] turn build on or off Ex: 1 [] [:]  ";

        public const string build_action_description = "Will build all project in the tree.";        
        public const string build_arg_p__Publish_Description = "Publishes the build tree where Tree_Branch.Publish is true and any project that reference the one that is being published";
        public const string build_arg_pa__Publish_Description = "Publishes all of the projects in the build tree.";
        public const string build_arg_report__Publish_Description = "Writes a report of the build and publish process. supply a exsiting directory location to overide default location.";

        //view
        public const string view_action_description = "Will show a list of projects and tell what can and cannot publish";
        public const string view_arg_f__Publish_Description = "Will show the full build";
        public const string view_arg_s__Publish_Description = "Search and return only project that have the search term in the name";
        //public const string view_arg_p__Publish_Description = "Tells the application to set the publish flag to true or false Valid values: {true, false, yes, no, set, or clear}. use with Search arg: -s. Only Publishable projects can be set";


    }
}
