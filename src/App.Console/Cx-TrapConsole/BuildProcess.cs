using CxUtility.Cx_Console;
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


        WriteOutput_Service.write_Lines("Test the new Code output in the build process");

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

        var start_Dir =  //<< just 2 dir behind: Make sure you stick to the pattern when Creating new Projects
            Path.Combine(start_Dir_Parts.Take(start_Dir_Parts.Length - ct).ToArray());

        const string CxTreeName = "Cx-BuildTree.json";
        string CxTreeRootPath = Path.Combine(start_Dir, CxTreeName);

        Build_Tree? _tree = null;

        if (File.Exists(CxTreeRootPath))
        {
            //using var file_Stream = File.Open(CxTreeRootPath, FileMode.Open);
            //System.Text.Json.JsonDocument jDoc = System.Text.Json.JsonDocument.Parse(file_Stream);
            string jsonString = await File.ReadAllTextAsync(CxTreeRootPath);
            _tree = jsonString.FromJson<Build_Tree>();

            throw new NotImplementedException("Need to Finsih this Action"); 
        }



        WriteOutput_Service.write_Lines($"Starting Path: {start_Dir}");
        //WriteOutput_Service.write_Lines("\n");

        Dir_Search(start_Dir);

        _tree = _tree ?? new Build_Tree(tree_view.Where(w => !w.EndsWith("Cx-TrapConsole.csproj", StringComparison.OrdinalIgnoreCase)));

        _tree.Check_Versions();


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

    //Searches the tree for .csproj Files
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

    public Build_Tree() { }

    public Build_Tree(IEnumerable<string> Branch_Paths)
    {
        Branches.AddRange(Branch_Paths.Select(s => new Tree_Branch(s)));
    }

    public Build_Tree(string root_Path)
    {
        if (Directory.Exists(root_Path))
            Branches.AddRange(Dir_Search(root_Path).Select(s => new Tree_Branch(s)));
    }

    public string? root { get; set; }
    
    public List<Tree_Branch> Branches { get; set; } = new List<Tree_Branch>();

    public void Check_Publish_Order()
    {
        throw new NotFiniteNumberException();
    }

    /// <summary>
    /// Will run through all projects and make sure they have a starting version number
    /// </summary>    
    public void Check_Versions()
    {
        foreach (var item in Branches)
            if (item.Proj_Version.hasNoCharacters())
                item.IncreseVersion();
    }

    
    /// <summary>
    /// Seraches for Braches from the root.
    /// </summary>
    /// <param name="dir_to_Search">The root Path To Search</param>
    /// <returns></returns>
    string[] Dir_Search(string root_dir_to_Search)
    {
        IList<string> tree_view = Recursion(root_dir_to_Search, new List<string>());

        IList<string> Recursion(string dir_to_Search, List<string> tree_view)
        {
            foreach (var item in Directory.GetFiles(dir_to_Search, "*.csproj"))
                tree_view.Add(item);

            foreach (var item in Directory.GetDirectories(dir_to_Search))
                tree_view.AddRange(Recursion(item, tree_view));

            return tree_view;
        }

        return tree_view.Distinct().ToArray();
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
        Proj_Name = Proj_Path.Split(Path.DirectorySeparatorChar).Last().Split('.')[0];
        Proj_Namespace = PropertyGroup["RootNamespace"]?.InnerText ?? string.Empty;
    }

    public List<Tree_Branch_Referenece> References { get; set; } = new();
  
    void Load_References(System.Xml.XmlDocument doc)
    {
        foreach(var node in doc.GetElementsByTagName("ItemGroup"))
        {
            
            //var t = node.


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
        var splitVersion = (Proj_Version ?? baseVersion).Split('.');
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
    //public enum ReferenceType { Dll, Package, Project  }
    //public ReferenceType referenceType { get; set; }
    public string? referenceType { get; set; }
    public string? name { get; set; }
    public string? version { get; set; }
}

