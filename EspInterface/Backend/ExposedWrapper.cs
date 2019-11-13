using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EspInterface.Backend
{
    class ExposedWrapper
    {
        [DllImport("Library.dll")]
        public static extern IntPtr new_ExposedFunctions();

        [DllImport("Library.dll")]
        public static extern void delete_ExposedFunctions(IntPtr instance);



    }
}
