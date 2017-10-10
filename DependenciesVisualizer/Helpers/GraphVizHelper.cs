using System.Collections.Generic;
using System.Collections.Immutable;
using Shields.GraphViz.Models;

namespace DependenciesVisualizer.Helpers
{
    public enum Colors
    {
        Green
    }

    public static class GraphVizHelper
    {
        private static ImmutableDictionary<Id, Id> emptyDictionary = new Dictionary<Id, Id>().ToImmutableDictionary();

        public static void AddGeneralStatements(ref List<Statement> statements)
        {
            // Graph orientation: left to right
            var generalStyleSettings = new Dictionary<Id, Id>();
            generalStyleSettings.Add(new Id("rankdir"), new Id("LR"));
            var generalStyleAttributes = new AttributeStatement(AttributeKinds.Graph, generalStyleSettings.ToImmutableDictionary());
            statements.Add(generalStyleAttributes);

            // We draw filled rectangles
            var generalNodeStyleSettings = new Dictionary<Id, Id>();
            generalNodeStyleSettings.Add(new Id("shape"), new Id("rectangle"));
            generalNodeStyleSettings.Add(new Id("style"), new Id("filled"));
            //generalNodeStyleSettings.Add(new Id("fontname"), new Id("Monospace"));
            var generalNodeStyleAttributes = new AttributeStatement(AttributeKinds.Node, generalNodeStyleSettings.ToImmutableDictionary());

            statements.Add(generalNodeStyleAttributes);
        }

        public static void AddEdgeStatement(ref List<Statement> statements, string origin, string destination)
        {
            var edge = new EdgeStatement(new NodeId(origin),
                                         new NodeId(destination),
                                         emptyDictionary);

            statements.Add(edge);
        }

        public static void ColorizeNode(ref List<Statement> statements, string nodeId, Colors color)
        {
            Dictionary<Id, Id> nodeStyleSettings = null;

            switch (color)
            {
                case Colors.Green:
                    nodeStyleSettings = new Dictionary<Id, Id>();
                    nodeStyleSettings.Add(new Id("color"), new Id("0.408 0.498 1.000"));
                    break;
            }

            var node = new NodeStatement(new Id(nodeId), nodeStyleSettings.ToImmutableDictionary());
            statements.Add(node);
        }

    }
}
