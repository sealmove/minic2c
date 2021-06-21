namespace MiniC {
  public abstract class ASTBaseVisitor<T> {
    public virtual T Visit(ASTVisitableElement node) {
      return node.Accept(this);
    }

    public virtual T VisitChildren(ASTVisitableElement node) {
      T netResult = default;
      foreach (ASTVisitableElement child in node.GetChildren()) {
        netResult = AggregateResult(netResult, child.Accept(this));
      }
      return netResult;
    }

    public virtual T VisitContext(ASTVisitableElement node, int context) {
      foreach (ASTVisitableElement child in node.GetChildren(context)) {
        child.Accept(this);
      }
      return default;
    }

    public virtual T AggregateResult(T oldResult, T value) {
      return value;
    }
  }

  public abstract class ASTVisitableElement : ASTElement {
    protected ASTVisitableElement(int context) : base(context) { }
    public abstract T Accept<T>(ASTBaseVisitor<T> visitor);
  }
}