import * as ts from 'typescript';
import { NodeConverter } from './NodeConverter';

export class StatementConverter extends NodeConverter {
    public convertNode(node: ts.Node): string {
        switch (node.kind) {
            case ts.SyntaxKind.IfStatement:
                return this.convertIfStatement(node as ts.IfStatement);
            case ts.SyntaxKind.ForStatement:
                return this.convertForStatement(node as ts.ForStatement);
            case ts.SyntaxKind.WhileStatement:
                return this.convertWhileStatement(node as ts.WhileStatement);
            case ts.SyntaxKind.SwitchStatement:
                return this.convertSwitchStatement(node as ts.SwitchStatement);
            case ts.SyntaxKind.TryStatement:
                return this.convertTryStatement(node as ts.TryStatement);
            default:
                return super.convertNode(node);
        }
    }

    protected convertIfStatement(node: ts.IfStatement): string {
        const condition = super.convertNode(node.expression);
        return `IF (${condition})`;
    }

    protected convertForStatement(node: ts.ForStatement): string {
        const initializer = node.initializer ? super.convertNode(node.initializer) : "";
        const condition = node.condition ? super.convertNode(node.condition) : "";
        const incrementor = node.incrementor ? super.convertNode(node.incrementor) : "";
        return `FOR (${initializer}; ${condition}; ${incrementor})`;
    }

    protected convertWhileStatement(node: ts.WhileStatement): string {
        const condition = super.convertNode(node.expression);
        return `WHILE (${condition})`;
    }

    protected convertSwitchStatement(node: ts.SwitchStatement): string {
        const expression = super.convertNode(node.expression);
        return `SWITCH (${expression})`;
    }

    protected convertTryStatement(node: ts.TryStatement): string {
        const catchClause = node.catchClause ? "CATCH" : "";
        const finallyBlock = node.finallyBlock ? "FINALLY" : "";
        return `TRY ${catchClause} ${finallyBlock}`;
    }
}
