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

namespace DashboardFrontend.DetachedWindows
{
    public partial class ConnectDBDialog : Window
    {
        public ConnectDBDialog()
        {
            InitializeComponent();
        }

        private void OnButtonConnectDBClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnButtonBackClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}