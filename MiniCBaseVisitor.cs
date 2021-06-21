namespace MiniC {
  public abstract class MiniCBaseVisitor<T> : ASTBaseVisitor<T> {
    public virtual T VisitCompileUnit(CCompileUnit node) {
      return VisitChildren(node);
    }

    public virtual T VisitFuncDef(CFuncDef node) {
      return VisitChildren(node);
    }

    public virtual T VisitBlock(CBlock node) {
      return VisitChildren(node);
    }

    public virtual T VisitIf(CIf node) {
      return VisitChildren(node);
    }

    public virtual T VisitRet(CRet node) {
      return VisitChildren(node);
    }

    public virtual T VisitWhile(CWhile node) {
      return VisitChildren(node);
    }

    public virtual T VisitNot(CNot node) {
      return VisitChildren(node);
    }

    public virtual T VisitPlus(CPlus node) {
      return VisitChildren(node);
    }

    public virtual T VisitMinus(CMinus node) {
      return VisitChildren(node);
    }

    public virtual T VisitMult(CMult node) {
      return VisitChildren(node);
    }

    public virtual T VisitDiv(CDiv node) {
      return VisitChildren(node);
    }

    public virtual T VisitAdd(CAdd node) {
      return VisitChildren(node);
    }

    public virtual T VisitSub(CSub node) {
      return VisitChildren(node);
    }

    public virtual T VisitAnd(CAnd node) {
      return VisitChildren(node);
    }

    public virtual T VisitOr(COr node) {
      return VisitChildren(node);
    }

    public virtual T VisitGt(CGt node) {
      return VisitChildren(node);
    }

    public virtual T VisitGte(CGte node) {
      return VisitChildren(node);
    }

    public virtual T VisitLt(CLt node) {
      return VisitChildren(node);
    }

    public virtual T VisitLte(CLte node) {
      return VisitChildren(node);
    }

    public virtual T VisitEq(CEq node) {
      return VisitChildren(node);
    }

    public virtual T VisitNeq(CNeq node) {
      return VisitChildren(node);
    }

    public virtual T VisitAsgn(CAsgn node) {
      return VisitChildren(node);
    }

    public virtual T VisitFuncCall(CFuncCall node) {
      return VisitChildren(node);
    }

    public virtual T VisitTerminal(MiniCASTElement node) {
      return default;
    }
  }
}
