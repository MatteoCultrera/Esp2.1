using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspInterface.ViewModels
{
    public class ManagedObject
    {
        private IntPtr native_instance_;  //switched from pvt to public to make serverinterop works

        public ManagedObject(int value)
        {
            native_instance_ = ManagedWrapper.new_NativeObject(value);
        }

        public void delete()
        {
            ManagedWrapper.delete_NativeObject(native_instance_);
        }

        public int get_value()
        {
            return ManagedWrapper.get_value(native_instance_);
        }

        public void dump() {

        }

        public void set_value(int value)
        {
            ManagedWrapper.set_value(native_instance_, value);
        }

        public IntPtr checkMacAddr(char[] macAddr, int size)
        {
            return ManagedWrapper.checkMacAddr(native_instance_, macAddr, size);
        }
        public int set_board_user(char[] macAddr, int posx, int posy)
        {
            return ManagedWrapper.set_board_user(native_instance_, macAddr, posx, posy);
        }

        public void printBoardList()
        {
            ManagedWrapper.printBoardList(native_instance_);
        }
    }
}
