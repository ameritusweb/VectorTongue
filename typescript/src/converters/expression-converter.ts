import * as ts from 'typescript';
import { NodeConverter } from './NodeConverter';

export class ExpressionConverter extends NodeConverter {
    public convertNode(node: ts.Node): string {
        switch (node.kind) {
            case ts.SyntaxKind.ObjectLiteralExpression:
                return this.convertObjectLiteralExpression(node as ts.ObjectLiteralExpression);
            case ts.SyntaxKind.ArrayLiteralExpression:
                return this.convertArrayLiteralExpression(node as ts.ArrayLiteralExpression);
            case ts.SyntaxKind.BinaryExpression:
                return this.convertBinaryExpression(node as ts.BinaryExpression);
            case ts.SyntaxKind.CallExpression:
                return this.convertCallExpression(node as ts.CallExpression);
            case ts.SyntaxKind.DestructuringAssignment:
                return this.convertDestructuringAssignment(node as ts.DestructuringAssignment);
            default:
                return super.convertNode(node);
        }
    }

    protected convertObjectLiteralExpression(node: ts.ObjectLiteralExpression): string {
        const properties = node.properties.map(prop => {
            if (ts.isPropertyAssignment(prop)) {
                return `${prop.name.getText()}: ${super.convertNode(prop.initializer)}`;
            }
            return super.convertNode(prop);
        });
        return `OBJECT { ${properties.join(", ")} }`;
    }

    protected convertArrayLiteralExpression(node: ts.ArrayLiteralExpression): string {
        const elements = node.elements.map(elem => super.convertNode(elem));
        return `ARRAY [ ${elements.join(", ")} ]`;
    }

    protected convertBinaryExpression(node: ts.BinaryExpression): string {
        const left = super.convertNode(node.left);
        const right = super.convertNode(node.right);
        const operator = ts.tokenToString(node.operatorToken.kind);
        return `${left} ${operator} ${right}`;
    }

    protected convertCallExpression(node: ts.CallExpression): string {
        const expression = super.convertNode(node.expression);
        const args = node.arguments.map(arg => super.convertNode(arg)).join(", ");
        return `CALL ${expression}(${args})`;
    }

    // Destructuring Assignment Conversion
    protected convertDestructuringAssignment(node: ts.DestructuringAssignment): string {
        const left = super.convertNode(node.left);
        const right = super.convertNode(node.right);
        return `DESTRUCTURE ${left} = ${right}`;
    }
}
