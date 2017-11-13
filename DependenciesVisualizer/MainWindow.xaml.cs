﻿using System.Windows;
using DependenciesVisualizer.ViewModels;
using Ninject;

namespace DependenciesVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IKernel ioc)
        {
            this.DataContext = new MainWindowViewModel(ioc);
            this.InitializeComponent();
            

            //var theViewModel = new MainViewModel(new DialogManager(this, this.Dispatcher));

            //if (theViewModel.IsAppValidAndReady)
            //{
            //    this.DataContext = theViewModel;

            //    theViewModel.BuildTreeView(ref this.Queries);
            //}
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
