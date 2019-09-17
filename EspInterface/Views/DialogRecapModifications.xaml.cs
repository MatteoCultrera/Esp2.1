﻿using System;
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
    /// Logica di interazione per DialogRecapModifications.xaml
    /// </summary>
    public partial class DialogRecapModifications : Window
    {

        List<NewModifiedBoards> myBoards;

        public DialogRecapModifications(List<NewModifiedBoards> news)
        {
            InitializeComponent();
            this.myBoards = news;
            lbBoards.ItemsSource = myBoards;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


    }
}
