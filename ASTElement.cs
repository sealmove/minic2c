using System.Collections.Generic;
using System.Linq;

namespace MiniC {
  public abstract class ASTElement {
    private List<ASTElement>[] children = null;
    public List<ASTElement> Parents = new List<ASTElement>();
    public readonly int Serial;
    private static int counter = 0;

    public string Name { get; set; }
    public abstract string[] ContextNames { get; }

    public IEnumerable<ASTElement> GetChildren() {
      return children.SelectMany(t => t);
    }

    public IEnumerable<ASTElement> GetChildren(int context) {
      return children[context];
    }

    protected ASTElement(int context) {
      Serial = counter++;
      Name = GenerateNodeName();
      if (context == 0) return;
      children = new List<ASTElement>[context];
      for (int i = 0; i < context; i++) {
        children[i] = new List<ASTElement>();
      }
    }

    public void AddChild(ASTElement child, int contextIndex) {
      child.Parents.Add(this);
      children[contextIndex].Add(child);
    }

    public ASTElement GetChild(int context, int index) {
      return children[context][index];
    }

    public int ContextNumber() {
      return children.Length;
    }

    public int ChildrenNumber(int context) {
      return children[context].Count;
    }

    public virtual string GenerateNodeName() {
      return "_" + Serial;
    }
  }
}