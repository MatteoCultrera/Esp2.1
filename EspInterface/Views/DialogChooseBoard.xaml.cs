using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EspInterface.Views
{
    /// <summary>
    /// Logica di interazione per DialogChooseBoard.xaml
    /// </summary>
    public partial class DialogChooseBoard : Window
    {
        List<LoadedBoard> loadedBoards;
        private LoadedBoard _selected;
        public LoadedBoard picked
        {
            get { return this._selected; }
        }
        public DialogChooseBoard(List<LoadedBoard> list)
        {
            InitializeComponent();
            this.loadedBoards = list;
            this._selected = null;
            lbBoards.ItemsSource = list;

        }

        private void listBox1_SelectedIndexChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
           if(lbBoards.SelectedItem != null)
            {
                LoadedBoard sel = lbBoards.SelectedItem as LoadedBoard;
                this._selected = sel;
                this.DialogResult = true;
            }   
        }

        private void cancelClicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


    }
}
