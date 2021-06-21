parser grammar MiniCParser;

options {
    tokenVocab = MiniCLexer;
}

compileUnit : (stmt|funcDef)+;
block : LB (stmt)* RB;
funcDef : FUNC ID LP parameters? RP block;
parameters : ID (COMMA ID)*;
args : expr (COMMA expr)*;
stmts : stmt*;

stmt
  : BREAK SC                                      #StmtBREAK
  | block                                         #StmtBlock
  | expr SC                                       #StmtExpr
  | IF LP expr RP LB stmts RB (ELSE LB stmts RB)? #StmtIf
  | RET expr SC                                   #StmtRet
  | WHILE LP expr RP LB stmts RB                  #StmtWhile
  ;

expr
  : ID                                  #ExprID
  | INT                                 #ExprINT
  | FLOAT                               #ExprFLOAT
  | NOT expr                            #ExprNot
  | op=(PLUS|MINUS) expr                #ExprUnary
  | expr op=(MULT|DIV) expr             #ExprMulDiv
  | expr op=(PLUS|MINUS) expr           #ExprAddSub
  | expr AND expr                       #ExprAnd
  | expr OR expr                        #ExprOr
  | expr op=(GT|GTE|LT|LTE|EQ|NEQ) expr #ExprComp
  | <assoc=right> ID ASGN expr          #ExprAsgn
  | ID LP args? RP                      #ExprFuncCall
  | LP expr RP                          #ExprPar
  ;