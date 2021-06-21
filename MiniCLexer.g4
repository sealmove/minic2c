lexer grammar MiniCLexer;

FUNC : 'function';
RET : 'return'; 
IF : 'if';
ELSE : 'else';
WHILE : 'while';
BREAK : 'break';

PLUS : '+'; 
MINUS : '-';
DIV : '/';
MULT : '*';
OR : '||';
AND : '&&';
NOT : '!';
EQ : '==';
NEQ : '!='; 
GT : '>';
LT : '<';
GTE : '>=';
LTE : '<=';
LP : '(';
RP : ')';
LB : '{';
RB : '}';
SC : ';';
COMMA : ',';
PERIOD : '.';
ASGN : '=';

INT : [0-9]+;
FLOAT : INT* PERIOD INT*;
ID : [a-z][a-zA-Z0-9_]*;
WS
	:	[\r\n\t ] -> skip
	;