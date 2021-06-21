using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiniC {
  class CodeGenerator : MiniCBaseVisitor<int> {
    public CodeASTElement TranslatedFile { get; set; }
    private readonly Stack<ValueTuple<CodeASTElement, int>> _stack = new Stack<ValueTuple<CodeASTElement, int>>();

    public void PrintStructure(StreamWriter file) { }

    public void EmitToStdout() {
      Console.Write(TranslatedFile.AssemblyCode().ToString());
    }

    public void EmitToFile(StreamWriter file) { }

    public override int VisitCompileUnit(CCompileUnit node) {
      TranslatedFile = new GFile();
      TranslatedFile.AddCode("#include <stdio.h>", GFile.Directives);
      TranslatedFile.AddCode("#include <stdlib.h>", GFile.Directives);

      var mainFunc = new GMainFuncDef();
      TranslatedFile.AddChild(mainFunc, GFile.FuncDefs);
      _stack.Push((mainFunc, GMainFuncDef.Body));
      VisitContext(node, CCompileUnit.Stmts);
      _stack.Pop();

      _stack.Push((TranslatedFile, GFile.FuncDefs));
      VisitContext(node, CCompileUnit.FuncDefs);
      _stack.Pop();

      return 0;
    }

    public override int VisitFuncDef(CFuncDef node) {
      var nn = new GFuncDef();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      var funcName = (node.GetChild(CFuncDef.FName, 0) as CID).Label;
      nn.AddCode(funcName, CFuncDef.FName);

      var decl = $"float {funcName}(";
      var n = node.ChildrenNumber(CFuncDef.Params);
      for (int i = 0; i < n; ++i) {
        var paramName = (node.GetChild(CFuncDef.Params, i) as CID).Label;
        parent.DeclareVar(paramName, "float");
        nn.AddCode(paramName, GFuncDef.Params);
        decl += $"float {paramName}";
        if (i != n - 1)
          decl += ", ";
      }

      decl += ")";
      parent.AddCode(decl, GFile.ForwardDecls);

      _stack.Push((nn, GFuncDef.Body));
      VisitContext(node, CFuncDef.Body);
      _stack.Pop();
      return 0;
    }

    public override int VisitAsgn(CAsgn node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      var cid = node.GetChild(CAsgn.Left, 0) as CID;
      var varname = cid.Label;
      string type = "na";
      switch (cid.Type) {
      case MiniCLitType.Int:
        type = "int";
        break;
      case MiniCLitType.Float:
        type = "float";
        break;
      }
      parent.DeclareVar(varname, type);
      parent.AddChild(nn, context);
      nn.AddCode($"{varname} = ");
      _stack.Push((nn, 0));
      VisitContext(node, CAsgn.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitBlock(CBlock node) {
      var nn = new GBlock();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CBlock.Body);
      _stack.Pop();
      return 0;
    }

    public override int VisitIf(CIf node) {
      var nn = new GIf();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, GIf.Cond));
      VisitContext(node, CIf.Cond);
      _stack.Pop();
      _stack.Push((nn, GIf.BodyTrue));
      VisitContext(node, CIf.BodyTrue);
      _stack.Pop();
      _stack.Push((nn, GIf.BodyFalse));
      VisitContext(node, CIf.BodyFalse);
      _stack.Pop();
      return 0;
    }

    public override int VisitRet(CRet node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      nn.AddCode("return ");
      _stack.Push((nn, 0));
      VisitContext(node, CRet.Expr);
      _stack.Pop();
      return 0;
    }

    public override int VisitWhile(CWhile node) {
      var nn = new GWhile();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, GWhile.Cond));
      VisitContext(node, CWhile.Cond);
      _stack.Pop();
      _stack.Push((nn, GWhile.Body));
      VisitContext(node, CWhile.Body);
      _stack.Pop();


      return 0;
    }

    public override int VisitNot(CNot node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      nn.AddCode("! ");
      _stack.Push((nn, 0));
      VisitContext(node, CNot.Operant);
      _stack.Pop();
      return 0;
    }

    public override int VisitPlus(CPlus node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      nn.AddCode("+ ");
      _stack.Push((nn, 0));
      VisitContext(node, CPlus.Operant);
      _stack.Pop();
      return 0;
    }

    public override int VisitMinus(CMinus node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      nn.AddCode("- ");
      _stack.Push((nn, 0));
      VisitContext(node, CMinus.Operant);
      _stack.Pop();
      return 0;
    }

    public override int VisitMult(CMult node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CMult.Left);
      _stack.Pop();
      nn.AddCode(" * ");
      _stack.Push((nn, 0));
      VisitContext(node, CMult.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitDiv(CDiv node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CDiv.Left);
      _stack.Pop();
      nn.AddCode(" / ");
      _stack.Push((nn, 0));
      VisitContext(node, CDiv.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitAdd(CAdd node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CAdd.Left);
      _stack.Pop();
      nn.AddCode(" + ");
      _stack.Push((nn, 0));
      VisitContext(node, CAdd.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitSub(CSub node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CSub.Left);
      _stack.Pop();
      nn.AddCode(" - ");
      _stack.Push((nn, 0));
      VisitContext(node, CSub.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitAnd(CAnd node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CAnd.Left);
      _stack.Pop();
      nn.AddCode(" && ");
      _stack.Push((nn, 0));
      VisitContext(node, CAnd.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitOr(COr node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, COr.Left);
      _stack.Pop();
      nn.AddCode(" || ");
      _stack.Push((nn, 0));
      VisitContext(node, COr.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitGt(CGt node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CGt.Left);
      _stack.Pop();
      nn.AddCode(" > ");
      _stack.Push((nn, 0));
      VisitContext(node, CGt.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitGte(CGte node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CGte.Left);
      _stack.Pop();
      nn.AddCode(" >= ");
      _stack.Push((nn, 0));
      VisitContext(node, CGte.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitLt(CLt node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CLt.Left);
      _stack.Pop();
      nn.AddCode(" < ");
      _stack.Push((nn, 0));
      VisitContext(node, CLt.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitLte(CLte node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CLte.Left);
      _stack.Pop();
      nn.AddCode(" <= ");
      _stack.Push((nn, 0));
      VisitContext(node, CLte.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitEq(CEq node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CEq.Left);
      _stack.Pop();
      nn.AddCode(" == ");
      _stack.Push((nn, 0));
      VisitContext(node, CEq.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitNeq(CNeq node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CNeq.Left);
      _stack.Pop();
      nn.AddCode(" != ");
      _stack.Push((nn, 0));
      VisitContext(node, CNeq.Right);
      _stack.Pop();
      return 0;
    }

    public override int VisitFuncCall(CFuncCall node) {
      var nn = new GCodeRepo();
      var (parent, context) = _stack.Peek();
      parent.AddChild(nn, context);
      _stack.Push((nn, 0));
      VisitContext(node, CFuncCall.FName);
      _stack.Pop();
      nn.AddCode("(");
      _stack.Push((nn, 0));
      for (int i = 0; i < node.ChildrenNumber(CFuncCall.Args); ++i) {
        //ASTVisitableElement child in node.GetChildren(CFuncCall.Args)) {
        Visit(node.GetChild(CFuncCall.Args, i) as ASTVisitableElement);
        if (i < node.ChildrenNumber(CFuncCall.Args) - 1)
          nn.AddCode(", ");
      }
      _stack.Pop();
      nn.AddCode(")");
      return 0;
    }

    public override int VisitTerminal(MiniCASTElement node) {
      var (parent, context) = _stack.Peek();
      switch (node.Nt) {
      case MiniCNodeType.ID:
        var cid = node as CID;
        var varname = cid.Label;
        var parentType = (node.Parents.Last() as MiniCASTElement).Nt;
        if (parentType == MiniCNodeType.FuncCall) {
          if (context != 0) {
            parent.DeclareVar(varname, "float");
          }
        }
        else if (parentType != MiniCNodeType.FuncDef) {
          parent.DeclareVar(varname, "float");
        }
        parent.AddCode(varname, context);
        break;
      case MiniCNodeType.BREAK:
        parent.AddCode("break", context);
        break;
      case MiniCNodeType.INT:
        parent.AddCode((node as CINT).Value, context);
        break;
      case MiniCNodeType.FLOAT:
        parent.AddCode((node as CFLOAT).Value, context);
        break;
      }
      return 0;
    }
  }
}