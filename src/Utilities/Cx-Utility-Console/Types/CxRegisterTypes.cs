using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.Cx_Console;

    public enum CxRegisterTypes
    {
        /// <summary>
        /// Do not show or process in the console 
        /// </summary>
        None,
        /// <summary>
        /// New or Updating Process that marked in the display Visible only in Code
        /// </summary>
        Preview,
        /// <summary>
        /// Fully Registed in the console
        /// </summary>
        Register
    }
