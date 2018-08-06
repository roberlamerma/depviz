using CommandLine;
using DependenciesVisualizer.Connectors.Services;
using log4net;
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
                Console(args);
            }
            else
            {
                // GUI mode
                ShowWindow(GetConsoleWindow(), 0 /*SW_HIDE*/);
                App.Main();
            }
        }

        private static void Console(string[] args)
        {
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed<ConsoleOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<ConsoleOptions>((errs) => HandleParseError(errs));

        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new NotImplementedException();
        }

        private static void RunOptionsAndReturnExitCode(ConsoleOptions opts)
        {
            //var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //var tfsService = new TfsService(logger);
            
        }
    }
}
