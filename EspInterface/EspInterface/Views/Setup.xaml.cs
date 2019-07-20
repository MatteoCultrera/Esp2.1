using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EspInterface.Models;
using EspInterface.ViewModels;

namespace EspInterface.Views
{
    /// <summary>
    /// Logica di interazione per Setup.xaml
    /// </summary>
    public partial class Setup : UserControl
    {
        public Setup()
        {
            InitializeComponent();
        }

        
        private void handleTextNumBoards(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            
            if (tb.GetLineLength(0) > 1)
            {
                e.Handled = true;
                return;
            }
            e.Handled = regex.IsMatch(e.Text);
        }

        private void deleteMac(object obj, RoutedEventArgs e)
        {
            Button b = obj as Button;
            Board board = b.DataContext as Board;
            board.MAC = "";
            SetupModel sm = (SetupModel)(this.DataContext);

            sm.checkMacs();

        }

        private void handleTextMAC(object sender, TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            Regex regex = new Regex("^[a-fA-F0-9:]*$");

            string text = box.GetLineText(0);
   
            if (box.GetLineLength(0) > 16)
            {
                e.Handled = true;
                return;
            }

            e.Handled = !regex.IsMatch(e.Text);
        }
         private void macEntered (object sender, KeyEventArgs e){

            TextBox box = sender as TextBox;
            Regex rxMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");

            SetupModel sm = (SetupModel)(this.DataContext);
            

            if (e.Key == Key.Return)
            {
                if ((!rxMacAddress.IsMatch(box.Text) || sm.macRepeated(box.Text.ToString())) && box.Text.Length != 0)
                {
                    Storyboard redFlash = box.TryFindResource("redFlash") as Storyboard;
                    box.Text = "";
                    redFlash.Begin(box);
                }
                else {
                    BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                    Keyboard.ClearFocus();
                    binding.UpdateSource();
                    sm.checkMacs();
                }
            }
           
        }

        private void BoxT_LostFocus(object sender, RoutedEventArgs e)
        {

            TextBox box = sender as TextBox;
            Regex rxMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");

            if (!rxMacAddress.IsMatch(box.Text) && box.Text.Length != 0)
            {
                Storyboard redFlash = box.TryFindResource("redFlash") as Storyboard;
                box.Text = "";
                redFlash.Begin(box);
            }
            else
            {
                BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                SetupModel sm = (SetupModel)(this.DataContext);
                Keyboard.ClearFocus();
                binding.UpdateSource();
                sm.checkMacs();
            }
        }

    }
}


