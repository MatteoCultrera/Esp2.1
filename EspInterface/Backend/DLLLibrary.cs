using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspInterface.Backend
{
    class DLLLibrary
    {
        private IntPtr native_instance;

        public DLLLibrary()
        {
            native_instance = ExposedWrapper.new_ExposedFunctions();
        }

        public void delete()
        {
            ExposedWrapper.delete_ExposedFunctions(native_instance);
        }

    }
}
