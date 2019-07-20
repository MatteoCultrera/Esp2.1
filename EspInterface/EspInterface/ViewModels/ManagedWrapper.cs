using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EspInterface.ViewModels
{
    class ManagedWrapper
    {
        [DllImport("Library.dll")]
        public static extern IntPtr new_NativeObject(int value);

        [DllImport("Library.dll")]
        public static extern void delete_NativeObject(IntPtr instance);

        [DllImport("Library.dll")]
        public static extern int get_value(IntPtr instance);

        [DllImport("Library.dll")]
        public static extern void set_value(IntPtr instance, int value);

    }
}
