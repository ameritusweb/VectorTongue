import * as ts from 'typescript';

export class NodeConverter {
    constructor(protected sourceFile: ts.SourceFile, protected typeChecker: ts.TypeChecker) {}

    public convertNode(node: ts.Node): string {
        const leadingComments = this.getLeadingComments(node);
        const nodeConversion = this.convertSpecificNode(node);
        const trailingComments = this.getTrailingComments(node);

        return [leadingComments, nodeConversion, trailingComments].filter(Boolean).join(" ");
    }

    protected convertSpecificNode(node: ts.Node): string {
        switch (node.kind) {
            case ts.SyntaxKind.ClassDeclaration:
                return this.convertClassDeclaration(node as ts.ClassDeclaration);
            case ts.SyntaxKind.MethodDeclaration:
                return this.convertMethodDeclaration(node as ts.MethodDeclaration);
            case ts.SyntaxKind.PropertyDeclaration:
                return this.convertPropertyDeclaration(node as ts.PropertyDeclaration);
            case ts.SyntaxKind.VariableStatement:
                return this.convertVariableStatement(node as ts.VariableStatement);
            case ts.SyntaxKind.FunctionDeclaration:
                return this.convertFunctionDeclaration(node as ts.FunctionDeclaration);
            case ts.SyntaxKind.ArrowFunction:
                return this.convertArrowFunction(node as ts.ArrowFunction);
            case ts.SyntaxKind.ReturnStatement:
                return this.convertReturnStatement(node as ts.ReturnStatement);
            case ts.SyntaxKind.ExportAssignment:
                return this.convertExportAssignment(node as ts.ExportAssignment);
            default:
                return this.defaultConversion(node);
        }
    }

    protected convertExportAssignment(node: ts.ExportAssignment): string {
        const expression = this.convertSpecificNode(node.expression);
        return `EXPORT DEFAULT ${expression}`;
    }

    protected convertClassDeclaration(node: ts.ClassDeclaration): string {
        const name = node.name ? node.name.getText() : "anonymous";
        return `CLASS ${name}`;
    }

    protected convertMethodDeclaration(node: ts.MethodDeclaration): string {
        const name = node.name.getText();
        const parameters = node.parameters.map(param => `PARAM ${param.name.getText()}`).join(" ");
        return `METHOD ${name} (${parameters})`;
    }

    protected convertPropertyDeclaration(node: ts.PropertyDeclaration): string {
        const name = node.name.getText();
        return `PROPERTY ${name}`;
    }

    protected convertVariableStatement(node: ts.VariableStatement): string {
        const declarations = node.declarationList.declarations.map(decl => {
            const name = decl.name.getText();
            const initializer = decl.initializer ? this.convertSpecificNode(decl.initializer) : "";
            return `${this.getDeclarationKind(node.declarationList)} ${name} ${initializer}`;
        });
        return declarations.join(" ");
    }

    protected convertFunctionDeclaration(node: ts.FunctionDeclaration): string {
        const name = node.name ? node.name.getText() : "anonymous";
        const parameters = node.parameters.map(param => `PARAM ${param.name.getText()}`).join(" ");
        return `FUNCTION ${name} (${parameters})`;
    }

    protected convertArrowFunction(node: ts.ArrowFunction): string {
        const parameters = node.parameters.map(param => `PARAM ${param.name.getText()}`).join(" ");
        return `ARROW_FUNCTION (${parameters}) => ${this.convertSpecificNode(node.body)}`;
    }

    protected convertReturnStatement(node: ts.ReturnStatement): string {
        const expression = node.expression ? this.convertSpecificNode(node.expression) : "";
        return `RETURN ${expression}`;
    }

    protected getDeclarationKind(declarationList: ts.VariableDeclarationList): string {
        if (declarationList.flags & ts.NodeFlags.Let) return "LET";
        if (declarationList.flags & ts.NodeFlags.Const) return "CONST";
        return "VAR";
    }

    protected defaultConversion(node: ts.Node): string {
        return ts.SyntaxKind[node.kind];
    }

    // Comment Handling Methods
    protected getLeadingComments(node: ts.Node): string | undefined {
        return this.formatComments(ts.getLeadingCommentRanges(this.sourceFile.text, node.pos));
    }

    protected getTrailingComments(node: ts.Node): string | undefined {
        return this.formatComments(ts.getTrailingCommentRanges(this.sourceFile.text, node.end));
    }

    protected formatComments(commentRanges: ts.CommentRange[] | undefined): string | undefined {
        if (!commentRanges || commentRanges.length === 0) {
            return undefined;
        }

        return commentRanges.map(range => {
            const commentText = this.sourceFile.text.substring(range.pos, range.end).trim();
            if (range.kind === ts.SyntaxKind.SingleLineCommentTrivia) {
                return `SINGLE_LINE_COMMENT ${commentText.substring(2)}`; // Remove //
            } else if (range.kind === ts.SyntaxKind.MultiLineCommentTrivia) {
                if (commentText.startsWith('/**')) {
                    return `JSDOC_COMMENT ${commentText.substring(3, commentText.length - 2)}`;
                } else {
                    return `MULTI_LINE_COMMENT ${commentText.substring(2, commentText.length - 2)}`;
                }
            }
            return `UNKNOWN_COMMENT ${commentText}`;
        }).join(' ');
    }
}
