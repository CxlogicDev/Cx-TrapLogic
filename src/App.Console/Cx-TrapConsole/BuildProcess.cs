using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;

using System.Linq;

namespace Cx_TrapConsole;

[CxConsoleInfo("build", typeof(BuildProcess), CxRegisterTypes.Preview, "Helps Build the Soultion. Also used as a Training process to understand Console Pipelining ")]
internal class BuildProcess : ConsoleBaseProcess
{
    string ContentRootPath { get; }

    const string baseVersion = "1.0.0.1";

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
    public Task DiscoverSolutionTree()
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

        WriteOutput_Service.write_Lines($"Starting Path: {start_Dir}");
        //WriteOutput_Service.write_Lines("\n");

        List<string> tree_view = new List<string>();

        Dir_Search(start_Dir);
        
        foreach (var tree in tree_view.Where(w => !w.Equals("Cx-TrapConsole.csproj") ))
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

            var versionNode = ProjectNode["Version"];

            bool hasVersion = versionNode != null;

            versionNode = versionNode ?? doc.CreateElement("Version");
           

            if(!hasVersion)
            {               
                versionNode.InnerText = baseVersion;
                PropertyGroup.AppendChild(versionNode);
                doc.Save(tree);
            }

            WriteOutput_Service.write_Lines(doc.OuterXml);

        }


        //Searches the tree for .csproj Files
        void Dir_Search(string dir_to_Search)
        {
            foreach (var item in Directory.GetFiles(dir_to_Search, "*.csproj"))
                tree_view.Add(item);            

            foreach (var item in Directory.GetDirectories(dir_to_Search))
                Dir_Search(item);           
        }

        string IncreseVersion(string currentVersion)
        {
            if (currentVersion.hasNoCharacters())
                return baseVersion;


            //Increase Posibly 
            var splatVersion = currentVersion.Split('.');
            var idx = splatVersion.Select((x, i) => i).Max();

            splatVersion[idx] = $"{((splatVersion[idx].toInt32() ?? 0) + 1)}";

            return string.Join(".",splatVersion);
        }
        


        return Task.CompletedTask;
    }


    /*
    public override Task ProcessAction(CancellationToken cancellationToken)
    {
        Console.WriteLine($"WindowBuffer-Width: {Console.BufferWidth}; WindowBuffer-Height: {Console.BufferHeight}");

        return Task.CompletedTask;
    }
    //*/
    




}

