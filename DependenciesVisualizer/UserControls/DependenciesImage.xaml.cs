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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DependenciesVisualizer.ViewModels;

namespace DependenciesVisualizer.UserControls
{
    /// <summary>
    /// Interaction logic for DependenciesImage.xaml
    /// </summary>
    public partial class DependenciesImage : UserControl
    {
        public DependenciesImage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.LoadingPanel.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //this.LoadingPanel.Visibility = Visibility.Collapsed;
        }
    }
}
