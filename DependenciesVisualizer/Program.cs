using CommandLine;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Helpers;
using log4net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DependenciesVisualizer
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                new ConsoleApp(args);
            }
            else
            {
                // GUI mode
                ShowWindow(GetConsoleWindow(), 0 /*SW_HIDE*/);
                App.Main();
            }
        }
    }
}
