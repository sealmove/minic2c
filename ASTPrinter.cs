using System.IO;

namespace MiniC {
  class ASTPrinter : MiniCBaseVisitor<int> {
    private StreamWriter STSpecFile = new StreamWriter("ast.dot");
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
        STSpecFile.WriteLine("}");
      }
    }

    public override int VisitCompileUnit(CCompileUnit node) {
      STSpecFile.WriteLine("digraph G{");
      ExtractSubgraphs(node);
      base.VisitChildren(node);
      STSpecFile.WriteLine("}");

      STSpecFile.Close();
      System.Diagnostics.Process.Start("dot.exe", "-Tgif -oast.gif ast.dot");
      return 0;
    }

    /*
    public override int VisitChildren(ASTVisitableElement node) {
      currParent = node;
      ExtractSubgraphs(node);
      base.VisitChildren(node);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }
    */
    
    public override int VisitFuncDef(CFuncDef node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CFuncDef.FName);
      VisitContext(node, CFuncDef.Params);
      VisitContext(node, CFuncDef.Body);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitBlock(CBlock node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CBlock.Body);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitIf(CIf node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CIf.Cond);
      VisitContext(node, CIf.BodyTrue);
      currParent = node;
      VisitContext(node, CIf.BodyFalse);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitRet(CRet node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CRet.Expr);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitWhile(CWhile node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CWhile.Cond);
      VisitContext(node, CWhile.Body);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitNot(CNot node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CNot.Operant);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitPlus(CPlus node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CPlus.Operant);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitMinus(CMinus node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CMinus.Operant);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitMult(CMult node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CMult.Left);
      currParent = node;
      VisitContext(node, CMult.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitDiv(CDiv node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CDiv.Left);
      currParent = node;
      VisitContext(node, CDiv.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitAdd(CAdd node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CAdd.Left);
      currParent = node;
      VisitContext(node, CAdd.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitSub(CSub node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CSub.Left);
      currParent = node;
      VisitContext(node, CSub.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitAnd(CAnd node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CAnd.Left);
      currParent = node;
      VisitContext(node, CAnd.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitOr(COr node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, COr.Left);
      currParent = node;
      VisitContext(node, COr.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitGt(CGt node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CGt.Left);
      currParent = node;
      VisitContext(node, CGt.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitGte(CGte node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CGte.Left);
      currParent = node;
      VisitContext(node, CGte.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitLt(CLt node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CLt.Left);
      currParent = node;
      VisitContext(node, CLt.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitLte(CLte node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CLte.Left);
      currParent = node;
      VisitContext(node, CLte.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitEq(CEq node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CEq.Left);
      currParent = node;
      VisitContext(node, CEq.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitNeq(CNeq node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CNeq.Left);
      currParent = node;
      VisitContext(node, CNeq.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitAsgn(CAsgn node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CAsgn.Left);
      currParent = node;
      VisitContext(node, CAsgn.Right);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitFuncCall(CFuncCall node) {
      currParent = node;
      ExtractSubgraphs(node);
      VisitContext(node, CFuncCall.FName);
      VisitContext(node, CFuncCall.Args);
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", node.Parents[0].Name, node.Name);
      return 0;
    }

    public override int VisitTerminal(MiniCASTElement node) {
      STSpecFile.WriteLine("\"{0}\"->\"{1}\";", currParent.Name, node.Name);
      return 0;
    }
  }
}