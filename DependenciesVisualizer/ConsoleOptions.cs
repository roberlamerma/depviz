using System;
using System.Text;

using CommandLine;
using CommandLine.Text;


namespace DependenciesVisualizer
{
    class ConsoleOptions
    {
        [Option('t', "tfsUrl", Required = true, HelpText = @"TFS Server URL (e.g. http://tfsprod:8080/tfs/defaultcollection).")]
        public string TfsUrl { get; set; }

        [Option('p', "project", Required = true, HelpText = @"Project name.")]
        public string Project { get; set; }

        [Option('q', "query", Required = true, HelpText = @"Full query path.")]
        public string Query { get; set; }

        [Option('o', "output", Required = true, HelpText = @"Output filename (i.g. MyDependencyImage.png)")]
        public string Output { get; set; }

        [Option('q', "queryLocation", Required = false, Default = "Shared Queries", HelpText = @"Where is the query stored ('My Queries', 'Shared Queries'")]
        public string QueryLocation { get; set; }
    }
}
