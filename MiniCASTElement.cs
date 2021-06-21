using System;
using System.CodeDom;
using System.Collections.Generic;

namespace MiniC {
  public enum MiniCNodeType {
    Na,

    // Non-terminals
    CompileUnit, FuncDef, Block, If, Ret, While, Not, Plus, Minus, Mult, Div,
    Add, Sub, And, Or, Gt, Gte, Lt, Lte, Eq, Neq, Asgn, FuncCall,
    
    // Terminals
    BREAK, ID, INT, FLOAT
  }

  public abstract class MiniCASTElement : ASTVisitableElement {
    public MiniCNodeType Nt { get; }
    protected MiniCASTElement(int context, MiniCNodeType type) : base(context) {
      Nt = type;
    }
  }

  public enum MiniCLitType {
    Na, Int, Float
  }

  public abstract class MiniCExpr : MiniCASTElement {
    public MiniCLitType Type { get; set; }
    protected MiniCExpr(int context, MiniCNodeType type) : base(context, type) { }
    public abstract MiniCLitType InferType();
  }

  public abstract class MiniCScope : MiniCASTElement {
    public Dictionary<string, MiniCASTElement> varSymbolTable = new Dictionary<string, MiniCASTElement>();
    protected MiniCScope(int context, MiniCNodeType type) : base(context, type) { }

    public bool IsVarUpstairs(string varname) {
      if (Parents.Count == 0) {
        return false;
      }

      var p = Parents[0] as MiniCScope;
      if (p.varSymbolTable.ContainsKey(varname)) {
        return true;
      }
      return p.IsVarUpstairs(varname);
    }
    public MiniCASTElement CacheVar(string varname) {
      if (varSymbolTable.ContainsKey(varname)) {
        return varSymbolTable[varname];
      }
      if (IsVarUpstairs(varname)) {
        var p = Parents[0] as MiniCScope;
        while (!p.varSymbolTable.ContainsKey(varname)) {
          p = p.Parents[0] as MiniCScope;
        }
        return p.varSymbolTable[varname];
      }
      var cid = new CID(varname);
      varSymbolTable[varname] = cid;
      return cid;
    }
  }

  public class CCompileUnit : MiniCScope {
    public const int Stmts = 0, FuncDefs = 1;
    public override string[] ContextNames { get; } = { "Stmts", "FuncDefs" };
    public CCompileUnit() : base(2, MiniCNodeType.CompileUnit) {}
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitCompileUnit(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"CompileUnit{base.GenerateNodeName()}";
    }
  }

  public class CFuncDef : MiniCScope {
    public const int FName = 0, Params = 1, Body = 2;
    public override string[] ContextNames { get; } = { "Name", "Params", "Body" };
    public CFuncDef() : base(3, MiniCNodeType.FuncDef) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitFuncDef(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"FuncDef{base.GenerateNodeName()}";
    }
  }

  public class CBlock : MiniCScope {
    public const int Body = 0;
    public override string[] ContextNames { get; } = { "Body" };
    public CBlock() : base(1, MiniCNodeType.Block) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitBlock(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Block{base.GenerateNodeName()}";
    }
  }

  public class CIf : MiniCScope {
    public const int Cond = 0, BodyTrue = 1, BodyFalse = 2;
    public override string[] ContextNames { get; } = { "Cond", "BodyTrue", "BodyFalse" };
    public CIf() : base(3, MiniCNodeType.If) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitIf(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"If{base.GenerateNodeName()}";
    }
  }

  public class CRet : MiniCASTElement {
    public const int Expr = 0;
    public override string[] ContextNames { get; } = { "Expr" };
    public CRet() : base(1, MiniCNodeType.Ret) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitRet(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Ret{base.GenerateNodeName()}";
    }
  }

  public class CWhile : MiniCScope {
    public const int Cond = 0, Body = 1;
    public override string[] ContextNames { get; } = { "Cond", "Body" };
    public CWhile() : base(2, MiniCNodeType.While) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitWhile(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"While{base.GenerateNodeName()}";
    }
  }

  public class CNot : MiniCExpr {
    public const int Operant = 0;
    public override string[] ContextNames { get; } = { "Operant" };
    public CNot() : base(1, MiniCNodeType.Not) { }
    public override MiniCLitType InferType() {
      Type = (GetChild(Operant, 0) as MiniCExpr).InferType();
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitNot(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Not{base.GenerateNodeName()}";
    }
  }

  public class CPlus : MiniCExpr {
    public const int Operant = 0;
    public override string[] ContextNames { get; } = { "Operant" };
    public CPlus() : base(1, MiniCNodeType.Plus) { }
    public override MiniCLitType InferType() {
      Type = (GetChild(Operant, 0) as MiniCExpr).InferType();
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitPlus(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Plus{base.GenerateNodeName()}";
    }
  }

  public class CMinus : MiniCExpr {
    public const int Operant = 0;
    public override string[] ContextNames { get; } = { "Operant" };
    public CMinus() : base(1, MiniCNodeType.Minus) { }
    public override MiniCLitType InferType() {
      Type = (GetChild(Operant, 0) as MiniCExpr).InferType();
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitMinus(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Minus{base.GenerateNodeName()}";
    }
  }

  public class CMult : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CMult() : base(2, MiniCNodeType.Mult) { }
    public override MiniCLitType InferType() {
      var left = (GetChild(Left, 0) as MiniCExpr).InferType();
      var right = (GetChild(Right, 0) as MiniCExpr).InferType();
      if (left == MiniCLitType.Float || right == MiniCLitType.Float) {
        Type = MiniCLitType.Float;
      } else {
        Type = MiniCLitType.Int;
      }
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitMult(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Mult{base.GenerateNodeName()}";
    }
  }

  public class CDiv : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CDiv() : base(2, MiniCNodeType.Div) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Float;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitDiv(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Div{base.GenerateNodeName()}";
    }
  }

  public class CAdd : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CAdd() : base(2, MiniCNodeType.Add) { }
    public override MiniCLitType InferType() {
      var left = (GetChild(Left, 0) as MiniCExpr).InferType();
      var right = (GetChild(Right, 0) as MiniCExpr).InferType();
      if (left == MiniCLitType.Float || right == MiniCLitType.Float) {
        Type = MiniCLitType.Float;
      } else {
        Type = MiniCLitType.Int;
      }
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitAdd(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Add{base.GenerateNodeName()}";
    }
  }

  public class CSub : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CSub() : base(2, MiniCNodeType.Sub) { }
    public override MiniCLitType InferType() {
      var left = (GetChild(Left, 0) as MiniCExpr).InferType();
      var right = (GetChild(Right, 0) as MiniCExpr).InferType();
      if (left == MiniCLitType.Float || right == MiniCLitType.Float) {
        Type = MiniCLitType.Float;
      } else {
        Type = MiniCLitType.Int;
      }
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitSub(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Sub{base.GenerateNodeName()}";
    }
  }

  public class CAnd : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CAnd() : base(2, MiniCNodeType.And) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitAnd(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"And{base.GenerateNodeName()}";
    }
  }

  public class COr : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public COr() : base(2, MiniCNodeType.Or) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitOr(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Or{base.GenerateNodeName()}";
    }
  }

  public class CGt : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CGt() : base(2, MiniCNodeType.Gt) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitGt(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Gt{base.GenerateNodeName()}";
    }
  }

  public class CGte : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CGte() : base(2, MiniCNodeType.Gte) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitGte(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Gte{base.GenerateNodeName()}";
    }
  }

  public class CLt : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CLt() : base(2, MiniCNodeType.Lt) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitLt(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Lt{base.GenerateNodeName()}";
    }
  }

  public class CLte : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CLte() : base(2, MiniCNodeType.Lte) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitLte(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Lte{base.GenerateNodeName()}";
    }
  }

  public class CEq : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CEq() : base(2, MiniCNodeType.Eq) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitEq(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Eq{base.GenerateNodeName()}";
    }
  }

  public class CNeq : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CNeq() : base(2, MiniCNodeType.Neq) { }
    public override MiniCLitType InferType() {
      Type = MiniCLitType.Int;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitNeq(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Neq{base.GenerateNodeName()}";
    }
  }

  public class CAsgn : MiniCExpr {
    public const int Left = 0, Right = 1;
    public override string[] ContextNames { get; } = { "Left", "Right" };
    public CAsgn() : base(2, MiniCNodeType.Asgn) { }
    public override MiniCLitType InferType() {
      var left = (GetChild(Left, 0) as MiniCExpr).InferType();
      var right = (GetChild(Right, 0) as MiniCExpr).InferType();
      if (left == MiniCLitType.Na) {
        Type = right;
      } else if (left == MiniCLitType.Float || right == MiniCLitType.Float) {
        Type = MiniCLitType.Float;
      }
      else {
        Type = MiniCLitType.Int;
      }
      (GetChild(Left, 0) as MiniCExpr).Type = Type;
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitAsgn(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Asgn{base.GenerateNodeName()}";
    }
  }

  public class CFuncCall : MiniCExpr {
    public const int FName = 0, Args = 1;
    public override string[] ContextNames { get; } = { "Name", "Args" };
    public CFuncCall() : base(2, MiniCNodeType.FuncCall) {
      Type = MiniCLitType.Float; // TODO
    }
    public override MiniCLitType InferType() {
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitFuncCall(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"FuncCall{base.GenerateNodeName()}";
    }
  }

  public class CBREAK : MiniCASTElement {
    public override string[] ContextNames { get; } = { };
    public CBREAK() : base(0, MiniCNodeType.BREAK) { }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitTerminal(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"BREAK{base.GenerateNodeName()}";
    }
  }

  public class CID : MiniCExpr {
    public string Label { get; }
    public override string[] ContextNames { get; } = { };

    public CID(string s) : base(0, MiniCNodeType.ID) {
      Label = s;
      Name = GenerateNodeName();    }
    public override MiniCLitType InferType() {
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitTerminal(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"ID{base.GenerateNodeName()}_{Label}";
    }
  }

  public class CINT : MiniCExpr {
    public string Value { get; }

    public override string[] ContextNames { get; } = { };

    public CINT(string s) : base(0, MiniCNodeType.INT) {
      Value = s;
      Type = MiniCLitType.Int;
    }
    public override MiniCLitType InferType() {
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitTerminal(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"INT{base.GenerateNodeName()}";
    }
  }

  public class CFLOAT : MiniCExpr {
    public string Value { get; }
    public override string[] ContextNames { get; } = { };
    public CFLOAT(string s) : base(0, MiniCNodeType.FLOAT) {
      Value = s;
      Type = MiniCLitType.Float;
    }
    public override MiniCLitType InferType() {
      return Type;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is MiniCBaseVisitor<T> miniCBaseVisitor) {
        return miniCBaseVisitor.VisitTerminal(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"FLOAT{base.GenerateNodeName()}";
    }
  }
}