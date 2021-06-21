using System;
using System.Collections.Generic;
using System.IO;

namespace MiniC {
  class CodePrinter : CodeBaseVisitor<int> {
    private StreamWriter STSpecFile = new StreamWriter("code.dot");
    private static int clusterCount = 0;
    private ASTElement currParent = null;

    private void ExtractSubgraphs(ASTElement node) {
      for (int context = 0; context < node.ContextNumber(); ++context) {
        if (node.ChildrenNumber(context) == 0) continue;
        STSpecFile.WriteLine("subgraph cluster" + clusterCount++ + "{");
        STSpecFile.WriteLine("\tnode [style=filled,color=white];");
        STSpecFile.WriteLine("\tstyle=filled;");
        STSpecFile.WriteLine("\tcolor=lightgrey;");
        STSpecFile.Write("\t");
        for (var i = 0; i < node.ChildrenNumber(context); ++i) {
          STSpecFile.Write(node.GetChild(context, i).Name + ";");
        }

        STSpecFile.WriteLine("\n\tlabel=" + node.ContextNames[context] + ";");
        //STSpecFile.WriteLine("\n\t\tlabel=" + context + ";");
        STSpecFile.WriteLine("}");
      }
    }

    public override int VisitFile(GFile node) {
      currParent = node;
      STSpecFile.WriteLine("digraph G{");
      ExtractSubgraphs(node);
      base.VisitChildren(node);
      STSpecFile.WriteLine("}");

      STSpecFile.Close();
      System.Diagnostics.Process.Start("dot.exe", "-Tgif -ocode.gif code.dot");
      return 0;
    }

    public override int VisitChildren(ASTVisitableElement node) {
      currParent = node;
      ExtractSubgraphs(node);
      base.VisitChildren(node);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitCodeRepo(GCodeRepo node) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", currParent.Name, node.Name);
      return 0;
    }
  }
}