namespace MiniC {
  public abstract class CodeBaseVisitor<T> : ASTBaseVisitor<T> {
    public virtual T VisitFile(GFile node) {
      return VisitChildren(node);
    }

    public virtual T VisitMainFuncDef(GMainFuncDef node) {
      return VisitChildren(node);
    }

    public virtual T VisitFuncDef(GFuncDef node) {
      return VisitChildren(node);
    }

    public virtual T VisitCodeRepo(GCodeRepo node) {
      return default;
    }

    public virtual T VisitWhile(GWhile node) {
      return VisitChildren(node);
    }

    public virtual T VisitIf(GIf node) {
      return VisitChildren(node);
    }

    public virtual T VisitBlock(GBlock node) {
      return VisitChildren(node);
    }
    /*
    public virtual T VisitExpr(GExpr node) {
      return VisitChildren(node);
    }
    */
    //public virtual T VisitRet(GRet node) {
    //  return VisitChildren(node);
    //}

    public virtual T VisitTerminal(CodeASTElement node) {
      return default;
    }
  }
}