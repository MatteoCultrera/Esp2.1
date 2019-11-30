using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspInterface.Backend
{
    public class BackendInterface
    {
        private DLLLibrary library;

        public BackendInterface()
        {
            library = new DLLLibrary();

            library.startBackendThread();
        }

        
    }


}
