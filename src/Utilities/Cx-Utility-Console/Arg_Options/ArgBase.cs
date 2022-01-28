using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.Cx_Console.Arg_Options
{


    //Create a Args Base to pass to the attributes, and pull for quick in action processing an
    internal class ArgBase
    {
    }


    /*
     


     record websiteDbArgs(string saveDir, string? websiteId, string? systemId)
    {
        internal const string Name = "build-db";
        internal const string Description = "Build The Websites Processing Data";
        internal const CxRegisterTypes RegisterType = CxRegisterTypes.Preview;

        internal const string arg_saveDir = "-saveDir";
        internal const string arg_saveDirName = "saveDir";
        internal const string arg_saveDirDescription = "";
        internal const CxConsoleActionArgAttribute.arg_Types arg_savePathType = CxConsoleActionArgAttribute.arg_Types.Value;


        public static websiteDbArgs Get_Args(CxCommandService _CxService)
        {

            _ = _CxService.getCommandArg(arg_saveDir, out string? saveDir);

            // return new(saveDir.ErrorIfNull(new ArgumentNullException(arg_saveDirName)));  
            throw new NotImplementedException();
        }
    }
     
     
     */



}
