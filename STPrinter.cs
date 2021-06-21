using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;

namespace MiniC {
  public class STPrinter : MiniCParserBaseVisitor<int> {
    private StreamWriter STSpecFile = new StreamWriter("st.dot");
    private Stack<string> parentsLabel = new Stack<string>();
    private static int counter = 0;

    public override int VisitCompileUnit(MiniCParser.CompileUnitContext context) {
      ++counter;
      string s = "CompileUnit";
      parentsLabel.Push(s);
      STSpecFile.WriteLine("digraph G{");
      base.VisitChildren(context);
      STSpecFile.WriteLine("}");
      parentsLabel.Pop();
      STSpecFile.Close();
      System.Diagnostics.Process.Start("dot.exe", "-Tgif -ost.gif st.dot");
      return 0;
    }

    public override int VisitChildren(IRuleNode node) {
      string mangled = node.GetType().ToString();
      string s = Regex.Replace(mangled, @".*[+]", "");
      s = Regex.Replace(s, "Context", "");
      s += "_" + ++counter;
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), s);
      parentsLabel.Push(s);
      base.VisitChildren(node);
      parentsLabel.Pop();
      return 0;
    }

    public override int VisitExprINT(MiniCParser.ExprINTContext context) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), "INT_" + context.GetText() + "_" + ++counter);
      return 0;
    }

    public override int VisitExprFLOAT(MiniCParser.ExprFLOATContext context) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), "FLOAT_" + context.GetText() + "_" + ++counter);
      return 0;
    }

    public override int VisitExprID(MiniCParser.ExprIDContext context) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), "ID_" + context.GetText() + "_" + ++counter);
      return 0;
    }

    public override int VisitStmtBREAK(MiniCParser.StmtBREAKContext context) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), "BREAK_" + context.GetText() + "_" + ++counter);
      return 0;
    }

    public override int VisitTerminal(ITerminalNode node) {
      switch (node.Symbol.Type) {
      case MiniCLexer.ID:
        STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentsLabel.Peek(), "ID_" + node.GetText() + "_" + ++counter);
        break;
      }
      return 0;
    }
  }
}