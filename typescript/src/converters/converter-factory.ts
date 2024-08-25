import * as ts from 'typescript';
import { NodeConverter } from './NodeConverter';
import { ExpressionConverter } from './ExpressionConverter';
import { StatementConverter } from './StatementConverter';

export class ConverterFactory {
    private sourceFile: ts.SourceFile;
    private typeChecker: ts.TypeChecker;

    constructor(sourceFile: ts.SourceFile, program: ts.Program) {
        this.sourceFile = sourceFile;
        this.typeChecker = program.getTypeChecker();
    }

    public createConverter(node: ts.Node): NodeConverter {
        if (this.isExpression(node)) {
            return new ExpressionConverter(this.sourceFile, this.typeChecker);
        } else if (this.isStatement(node)) {
            return new StatementConverter(this.sourceFile, this.typeChecker);
        }
        return new NodeConverter(this.sourceFile, this.typeChecker);
    }

    private isExpression(node: ts.Node): boolean {
        return ts.isExpression(node);
    }

    private isStatement(node: ts.Node): boolean {
        return ts.isStatement(node);
    }
}
