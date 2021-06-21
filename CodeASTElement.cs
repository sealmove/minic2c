using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniC {
  public enum CodeNodeType {
    Na,
    File,
    FuncDef,
    MainFuncDef,
    While,
    If,
    Block,
    CodeRepo
  }

  public abstract class CodeASTElement : ASTVisitableElement {
    public CodeNodeType Nt { get; }
    public static int NestingLevel { get; set; } = 0;

    public bool IsComplex() {
      return Nt == CodeNodeType.File ||
             Nt == CodeNodeType.FuncDef ||
             Nt == CodeNodeType.MainFuncDef ||
             Nt == CodeNodeType.Block ||
             Nt == CodeNodeType.If ||
             Nt == CodeNodeType.While;
    }

    public abstract StringBuilder AssemblyCode();

    public virtual void DeclareVar(string varname, string type) {
      (Parents.Last() as CodeASTElement)?.DeclareVar(varname, type);
    }

    public virtual bool HasVar(string varname) {
      return false;
    }

    public void AddCode(string code, int context) {
      AddChild(new GCodeRepo(code), context);
    }

    protected CodeASTElement(int context, CodeNodeType type) : base(context) {
      Nt = type;
    }

    public void EnterScope() { ++NestingLevel; }
    public void LeaveScope() { --NestingLevel; }

    public StringBuilder Nest(StringBuilder s) {
      return new StringBuilder(new string('\t', NestingLevel) + s);
    }

    public string Nest(string s) {
      return new string('\t', NestingLevel) + s;
    }
  }

  public class GFile : CodeASTElement {
    public HashSet<string> GlobalVarSymbolTable { get; set; } = new HashSet<string>();
    // private HashSet<string> _functionsSymbolTable { get; set; } = new HashSet<string>();
    public const int Directives = 0, ForwardDecls = 1, Globals = 2, FuncDefs = 3;
    public override string[] ContextNames { get; } = { "Directives", "ForwardDecls", "Globals", "FuncDefs" };

    public GFile() : base(context: 4, CodeNodeType.File) { }

    public override void DeclareVar(string varname, string type) {
      if (!GlobalVarSymbolTable.Contains(varname))
        AddCode(type + " " + varname + " = 0", Globals);
      GlobalVarSymbolTable.Add(varname);
    }
    public override bool HasVar(string varname) {
      return GlobalVarSymbolTable.Contains(varname);
    }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      foreach (CodeASTElement child in GetChildren(Directives)) {
        result.Append(child.AssemblyCode() + "\n");
      }
      if (ChildrenNumber(Directives) != 0)
        result.Append('\n');
      foreach (CodeASTElement child in GetChildren(ForwardDecls)) {
        result.Append(child.AssemblyCode() + ";\n");
      }
      if (ChildrenNumber(ForwardDecls) != 0)
        result.Append('\n');
      foreach (CodeASTElement child in GetChildren(Globals)) {
        result.Append(child.AssemblyCode() + ";\n");
      }
      if (ChildrenNumber(Globals) != 0)
        result.Append('\n');
      foreach (CodeASTElement child in GetChildren(FuncDefs)) {
        result.Append(child.AssemblyCode());
        result.Append('\n');
      }
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> codeBaseVisitor) {
        return codeBaseVisitor.VisitFile(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"File{base.GenerateNodeName()}";
    }
  }

  public class GFuncDef : CodeASTElement {
    private HashSet<string> _varSymbolTable = new HashSet<string>();
    public const int FName = 0, Params = 1, Body = 2, Decls = 3;
    public override string[] ContextNames { get; } = { "Name", "Params", "Body", "Decls"};
    public GFuncDef() : base(context: 4, CodeNodeType.FuncDef) { }

    public override void DeclareVar(string varname, string type) {
      bool isUpstairs = false;
      CodeASTElement p = this;
      while (p.Parents.Count() != 0 && (p.Parents.Last() as CodeASTElement).IsComplex()) {
        if ((p.Parents.Last() as CodeASTElement).HasVar(varname)) {
          isUpstairs = true;
          break;
        }
        p = p.Parents.Last() as CodeASTElement;
      }
      if (!isUpstairs) {
        if (!_varSymbolTable.Contains(varname))
          AddCode(type + " " + varname + " = 0", Decls);
        _varSymbolTable.Add(varname);
      }
    }

    public override bool HasVar(string varname) {
      return _varSymbolTable.Contains(varname);
    }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      result.Append("float ");
      result.Append((GetChild(FName, 0) as GCodeRepo).AssemblyCode());
      result.Append("(");
      var parameters = GetChildren(Params);
      var argCount = parameters.Count();
      for (int i = 0; i < argCount; ++i) {
        result.Append($"float {(parameters.ElementAt(i) as GCodeRepo).AssemblyCode()}");
        if (i != argCount - 1) {
          result.Append(", ");
        }
      }
      result.Append(") {\n");
      EnterScope();
      foreach (CodeASTElement child in GetChildren(Decls)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      foreach (CodeASTElement child in GetChildren(Body)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      LeaveScope();
      result.Append("}\n");
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> codeBaseVisitor) {
        return codeBaseVisitor.VisitFuncDef(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"FuncDef{base.GenerateNodeName()}";
    }
  }

  public class GMainFuncDef : CodeASTElement {
    public const int Body = 0, Decls = 1;
    public override string[] ContextNames { get; } = { "Body", "Decls" };
    public GMainFuncDef() : base(context: 2, CodeNodeType.MainFuncDef) { }
    public override void DeclareVar(string varname, string type) {
      var p = (Parents.Last() as GFile);
      if (!p.GlobalVarSymbolTable.Contains(varname))
        p.AddCode(type + " " + varname + " = 0", GFile.Globals);
      p.GlobalVarSymbolTable.Add(varname);
    }
    public override bool HasVar(string varname) {
      return (Parents.Last() as GFile).GlobalVarSymbolTable.Contains(varname);
    }
    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      result.Append("int main(void) {\n");
      EnterScope();
      foreach (CodeASTElement child in GetChildren(Decls)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      foreach (CodeASTElement child in GetChildren(Body)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      LeaveScope();
      result.Append("}\n");
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> CodeBaseVisitor) {
        return CodeBaseVisitor.VisitMainFuncDef(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"MainFuncDef{base.GenerateNodeName()}";
    }
  }

  public class GWhile : CodeASTElement {
    private HashSet<string> _varSymbolTable = new HashSet<string>();
    public const int Cond = 0, Body = 1, Decls = 2;
    public override string[] ContextNames { get; } = { "Cond", "Body", "Decls" };
    public GWhile() : base(context: 3, CodeNodeType.While) { }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      result.Append("while (");
      result.Append((GetChild(Cond, 0) as CodeASTElement).AssemblyCode());
      result.Append(") {\n");
      EnterScope();
      foreach (CodeASTElement child in GetChildren(Decls)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      foreach (CodeASTElement child in GetChildren(Body)) {
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      }
      LeaveScope();
      result.Append(Nest("}"));
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> CodeBaseVisitor) {
        return CodeBaseVisitor.VisitWhile(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"While{base.GenerateNodeName()}";
    }
  }

  public class GIf : CodeASTElement {
    public const int Cond = 0, BodyTrue = 1, BodyFalse = 2, BodyTrueDecls = 3;
    public override string[] ContextNames { get; } = { "Cond", "BodyTrue", "BodyFalse" };
    public GIf() : base(context: 3, CodeNodeType.If) { }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder("if (");
      result.Append((GetChild(GIf.Cond, 0) as CodeASTElement).AssemblyCode());
      result.Append(") {\n");
      EnterScope();
      foreach (CodeASTElement child in GetChildren(GIf.BodyTrue))
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      LeaveScope();
      result.Append(Nest("}"));
      if (GetChildren(CIf.BodyFalse).Count() != 0) {
        result.Append(" else {\n");
        EnterScope();
        foreach (CodeASTElement child in GetChildren(GIf.BodyFalse))
          result.Append(Nest(child.AssemblyCode()) + ";\n"); 
        LeaveScope();
        result.Append(Nest("}"));
      }
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> CodeBaseVisitor) {
        return CodeBaseVisitor.VisitIf(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"If{base.GenerateNodeName()}";
    }
  }

  public class GBlock : CodeASTElement {
    private HashSet<string> _varSymbolTable = new HashSet<string>();
    public const int Code = 0, Decls = 1;
    public override string[] ContextNames { get; } = { "Code", "Decls" };
    public GBlock() : base(context: 2, CodeNodeType.Block) { }
    public override void DeclareVar(string varname, string type) {
      bool isUpstairs = false;
      CodeASTElement p = this;
      while (p.Parents.Count() != 0 && (p.Parents.Last() as CodeASTElement).IsComplex()) {
        if ((p.Parents.Last() as CodeASTElement).HasVar(varname)) {
          isUpstairs = true;
          break;
        }
        p = p.Parents.Last() as CodeASTElement;
      }

      if (!isUpstairs) {
        if (!_varSymbolTable.Contains(varname))
          AddCode(type + " " + varname + " = 0", Decls);
        _varSymbolTable.Add(varname);
      }
    }

    public override bool HasVar(string varname) {
      return _varSymbolTable.Contains(varname);
    }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      result.Append("{\n");
      EnterScope();
      foreach (CodeASTElement child in GetChildren(Decls))
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      foreach (CodeASTElement child in GetChildren(Code))
        result.Append(Nest(child.AssemblyCode()) + ";\n");
      LeaveScope();
      result.Append(Nest("}"));
      return result;
    }
    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> CodeBaseVisitor) {
        return CodeBaseVisitor.VisitBlock(this);
      }
      return default;
    }
    public override string GenerateNodeName() {
      return $"Block{base.GenerateNodeName()}";
    }
  }

  public class GCodeRepo : CodeASTElement {
    public const int Code = 0;
    public override string[] ContextNames { get; } = { "Code" };
    private StringBuilder code;

    public GCodeRepo() : base(context: 1, CodeNodeType.CodeRepo) {
      code = new StringBuilder();
    }

    public GCodeRepo(string code) : base(context: 1, CodeNodeType.CodeRepo) {
      this.code = new StringBuilder();
      this.code.Append(code);
    }

    public override StringBuilder AssemblyCode() {
      var result = new StringBuilder();
      result.Append(code);
      foreach (CodeASTElement child in GetChildren(0))
        result.Append(child.AssemblyCode());
      return result;
    }

    public override T Accept<T>(ASTBaseVisitor<T> visitor) {
      if (visitor is CodeBaseVisitor<T> codeBaseVisitor) {
        return codeBaseVisitor.VisitCodeRepo(this);
      }
      return default;
    }

    public override string GenerateNodeName() {
      return $"CodeRepo{base.GenerateNodeName()}";
    }

    public void AddCode(string code) {
      AddChild(new GCodeRepo(code), 0);
    }

    public static GCodeRepo operator+(GCodeRepo b, GCodeRepo c) {
      var repo = new GCodeRepo();
      repo.Append(b.code.ToString());
      repo.Append(c.code.ToString());
      return repo;
    }

    public static implicit operator GCodeRepo(string s) {
      var repo = new GCodeRepo(s);
      return repo;
    }

    public string ToString(int startIndex, int length) {
      return code.ToString(startIndex, length);
    }

    public GCodeRepo Append(string value) {
      code.Append(value);
      return this;
    }
  }
}