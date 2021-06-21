using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace MiniC {
  class Program {
    static void Main(string[] args) {
      var aStream = new StreamReader(args[0]);
      var antlrInputStream = new AntlrInputStream(aStream);
      var lexer = new MiniCLexer(antlrInputStream);
      var tokens = new CommonTokenStream(lexer);
      var parser = new MiniCParser(tokens);
      IParseTree tree = parser.compileUnit();
      //Console.WriteLine(tree.ToStringTree());

      var stPrinter = new STPrinter();
      stPrinter.Visit(tree);

      var ast = new ASTGenerator();
      ast.Visit(tree);

      var astPrinter = new ASTPrinter();
      astPrinter.Visit(ast.Root);

      var code = new CodeGenerator();
      code.Visit(ast.Root);

      var codePrinter = new CodePrinter();
      codePrinter.Visit(code.TranslatedFile);

      code.EmitToStdout();
    }
  }
}