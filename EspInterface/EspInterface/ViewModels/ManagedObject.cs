using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspInterface.ViewModels
{
    class ManagedObject
    {
        private IntPtr native_instance_;

        public ManagedObject(int value) {
            native_instance_ = ManagedWrapper.new_NativeObject(value);
        }

        public void delete() {
            ManagedWrapper.delete_NativeObject(native_instance_);
        }

        public int get_value() {
            return ManagedWrapper.get_value(native_instance_);
        }

        public void set_value(int value) {
            ManagedWrapper.set_value(native_instance_, value);
        }

    }
}
