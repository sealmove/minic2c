using System.Collections.Generic;
using Antlr4.Runtime.Tree;

namespace MiniC {
  class ASTGenerator : MiniCParserBaseVisitor<int> {
    private CCompileUnit root;
    public CCompileUnit Root => root;
    private Dictionary<string, MiniCASTElement> funcSymbolTable = new Dictionary<string, MiniCASTElement>();
    Stack<(MiniCASTElement, int)> parents = new Stack<(MiniCASTElement, int)>();

    public override int VisitCompileUnit(MiniCParser.CompileUnitContext context) {
      root = new CCompileUnit();
      parents.Push((root, CCompileUnit.Stmts));
      foreach (MiniCParser.StmtContext stmtCtx in context.stmt())
        Visit(stmtCtx);
      parents.Pop();
      parents.Push((root, CCompileUnit.FuncDefs));
      foreach (MiniCParser.FuncDefContext funcDefCtx in context.funcDef())
        Visit(funcDefCtx);
      parents.Pop();
      return 0;
    }

    public override int VisitFuncDef(MiniCParser.FuncDefContext context) {
      var nn = new CFuncDef();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CFuncDef.FName));
      Visit(context.ID());
      parents.Pop();

      parents.Push((nn, CFuncDef.Params));
      Visit(context.parameters());
      parents.Pop();

      parents.Push((nn, CFuncDef.Body));
      Visit(context.block());
      parents.Pop();

      return 0;
    }

    public override int VisitStmtBlock(MiniCParser.StmtBlockContext context) {
      var nn = new CBlock();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CBlock.Body));
      Visit(context.block());
      parents.Pop();

      return 0;
    }

    public override int VisitStmtRet(MiniCParser.StmtRetContext context) {
      var nn = new CRet();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CRet.Expr));
      Visit(context.expr());
      parents.Pop();
      
      return 0;
    }

    public override int VisitStmtWhile(MiniCParser.StmtWhileContext context) {
      var nn = new CWhile();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CWhile.Cond));
      Visit(context.expr());
      parents.Pop();

      parents.Push((nn, CWhile.Body));
      Visit(context.stmts());
      parents.Pop();

      return 0;
    }

    public override int VisitStmtIf(MiniCParser.StmtIfContext context) {
      var nn = new CIf();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CIf.Cond));
      Visit(context.expr());
      parents.Pop();

      parents.Push((nn, CIf.BodyTrue));
      Visit(context.stmts(0));
      parents.Pop();

      parents.Push((nn, CIf.BodyFalse));
      if (context.stmts(1) != null)
        Visit(context.stmts(1));
      parents.Pop();

      return 0;
    }

    public override int VisitTerminal(ITerminalNode node) {
      var label = node.Symbol.Text;
      switch (node.Symbol.Type) {
      case MiniCLexer.ID:
        var (parent1, ctx1) = parents.Peek();
        CID nn1;
        if (parent1.Nt == MiniCNodeType.FuncDef ||
            (parent1.Nt == MiniCNodeType.FuncCall) && ctx1 == 0) {
          if (funcSymbolTable.ContainsKey(label)) {
            nn1 = funcSymbolTable[label] as CID;
          }
          else {
            nn1 = new CID(label);
            funcSymbolTable.Add(label, nn1);
          }
        }
        else {
          MiniCASTElement p = parent1;
          while (!(p is MiniCScope)) {
            p = p.Parents[0] as MiniCASTElement;
          }
          nn1 = (p as MiniCScope).CacheVar(label) as CID;
        }
        parent1.AddChild(nn1, ctx1);
        break;
      case MiniCLexer.INT:
        var nn2 = new CINT(label);
        var (parent2, ctx2) = parents.Peek();
        parent2.AddChild(nn2, ctx2);
        break;
      case MiniCLexer.FLOAT:
        var nn3 = new CFLOAT(label);
        var (parent3, ctx3) = parents.Peek();
        parent3.AddChild(nn3, ctx3);
        break;
      case MiniCLexer.BREAK:
        var nn4 = new CBREAK();
        var (parent4, ctx4) = parents.Peek();
        parent4.AddChild(nn4, ctx4);
        break;
      }
      return 0;
    }

    public override int VisitExprMulDiv(MiniCParser.ExprMulDivContext context) {
      switch (context.op.Type) {
      case MiniCLexer.MULT:
        var nn1 = new CMult();
        var (parent1, ctx1) = parents.Peek();
        parent1.AddChild(nn1, ctx1);

        parents.Push((nn1, CMult.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn1, CMult.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn1.InferType();
        break;
      case MiniCLexer.DIV:
        var nn2 = new CDiv();
        var (parent2, ctx2) = parents.Peek();
        parent2.AddChild(nn2, ctx2);

        parents.Push((nn2, CDiv.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn2, CDiv.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn2.InferType();
        break;
      }
      return 0;
    }

    public override int VisitExprAddSub(MiniCParser.ExprAddSubContext context) {
      switch (context.op.Type) {
      case MiniCLexer.PLUS:
        var nn1 = new CAdd();
        var (parent1, ctx1) = parents.Peek();
        parent1.AddChild(nn1, ctx1);

        parents.Push((nn1, CAdd.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn1, CAdd.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn1.InferType();
        break;
      case MiniCLexer.MINUS:
        var nn2 = new CSub();
        var (parent2, ctx2) = parents.Peek();
        parent2.AddChild(nn2, ctx2);

        parents.Push((nn2, CSub.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn2, CSub.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn2.InferType();
        break;
      }

      return 0;
    }

    public override int VisitExprAnd(MiniCParser.ExprAndContext context) {
      var nn = new CAnd();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CAnd.Left));
      Visit(context.expr(0));
      parents.Pop();

      parents.Push((nn, CAnd.Right));
      Visit(context.expr(1));
      parents.Pop();

      nn.InferType();
      return 0;
    }

    public override int VisitExprOr(MiniCParser.ExprOrContext context) {
      var nn = new COr();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, COr.Left));
      Visit(context.expr(0));
      parents.Pop();

      parents.Push((nn, COr.Right));
      Visit(context.expr(1));
      parents.Pop();

      nn.InferType();
      return 0;
    }

    public override int VisitExprNot(MiniCParser.ExprNotContext context) {
      var nn = new CNot();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CNot.Operant));
      Visit(context.expr());
      parents.Pop();

      nn.InferType();
      return 0;
    }

    public override int VisitExprComp(MiniCParser.ExprCompContext context) {
      switch (context.op.Type) {
      case MiniCLexer.GT:
        var nn1 = new CGt();
        var (parent1, ctx1) = parents.Peek();
        parent1.AddChild(nn1, ctx1);

        parents.Push((nn1, CGt.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn1, CGt.Right));
        Visit(context.expr(1));

        nn1.InferType();
        parents.Pop();
        break;
      case MiniCLexer.GTE:
        var nn2 = new CGte();
        var (parent2, ctx2) = parents.Peek();
        parent2.AddChild(nn2, ctx2);

        parents.Push((nn2, CGte.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn2, CGte.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn2.InferType();
        break;
      case MiniCLexer.LT:
        var nn3 = new CLt();
        var (parent3, ctx3) = parents.Peek();
        parent3.AddChild(nn3, ctx3);

        parents.Push((nn3, CLt.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn3, CLt.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn3.InferType();
        break;
      case MiniCLexer.LTE:
        var nn4 = new CLte();
        var (parent4, ctx4) = parents.Peek();
        parent4.AddChild(nn4, ctx4);

        parents.Push((nn4, CLte.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn4, CLte.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn4.InferType();
        break;
      case MiniCLexer.EQ:
        var nn5 = new CEq();
        var (parent5, ctx5) = parents.Peek();
        parent5.AddChild(nn5, ctx5);

        parents.Push((nn5, CEq.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn5, CEq.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn5.InferType();
        break;
      case MiniCLexer.NEQ:
        var nn6 = new CNeq();
        var (parent6, ctx6) = parents.Peek();
        parent6.AddChild(nn6, ctx6);

        parents.Push((nn6, CNeq.Left));
        Visit(context.expr(0));
        parents.Pop();

        parents.Push((nn6, CNeq.Right));
        Visit(context.expr(1));
        parents.Pop();

        nn6.InferType();
        break;
      }
      return 0;
    }

    public override int VisitExprAsgn(MiniCParser.ExprAsgnContext context) {
      var nn = new CAsgn();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CAsgn.Left));
      Visit(context.ID());
      parents.Pop();

      parents.Push((nn, CAsgn.Right));
      Visit(context.expr());
      parents.Pop();

      nn.InferType();
      return 0;
    }

    public override int VisitExprUnary(MiniCParser.ExprUnaryContext context) {
      switch (context.op.Type) {
      case MiniCLexer.PLUS:
        var nn1 = new CPlus();
        var (parent1, ctx1) = parents.Peek();
        parent1.AddChild(nn1, ctx1);

        parents.Push((nn1, CPlus.Operant));
        Visit(context.expr());
        parents.Pop();

        nn1.InferType();
        break;
      case MiniCLexer.MINUS:
        var nn2 = new CMinus();
        var (parent2, ctx2) = parents.Peek();
        parent2.AddChild(nn2, ctx2);

        parents.Push((nn2, CMinus.Operant));
        Visit(context.expr());
        parents.Pop();

        nn2.InferType();
        break;
      }
      return 0;
    }

    public override int VisitExprFuncCall(MiniCParser.ExprFuncCallContext context) {
      var nn = new CFuncCall();
      var (parent, ctx) = parents.Peek();
      parent.AddChild(nn, ctx);

      parents.Push((nn, CFuncCall.FName));
      Visit(context.ID());
      parents.Pop();

      parents.Push((nn, CFuncCall.Args));
      Visit(context.args());
      parents.Pop();

      nn.InferType();
      return 0;
    }
  }
}