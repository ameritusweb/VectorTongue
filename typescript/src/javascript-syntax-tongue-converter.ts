import * as ts from 'typescript';
import { ConverterFactory } from './converters/converter-factory';

export class JavaScriptSyntaxTongueConverter {
    private sourceFile: ts.SourceFile;
    private program: ts.Program;
    private converterFactory: ConverterFactory;
    private errors: string[] = [];

    constructor(sourceFile: ts.SourceFile, program: ts.Program) {
        this.sourceFile = sourceFile;
        this.program = program;
        this.converterFactory = new ConverterFactory(sourceFile, program);
    }

    public convert(): { syntaxTongue: string[], errors: string[] } {
        const syntaxTongue: string[] = [];
        const visit = (node: ts.Node) => {
            try {
                const converter = this.converterFactory.createConverter(node);
                syntaxTongue.push(converter.convertNode(node));
            } catch (error) {
                const errorMessage = `Error converting node at line ${this.sourceFile.getLineAndCharacterOfPosition(node.pos).line + 1}: ${node.getText()} - ${error}`;
                this.errors.push(errorMessage);
                console.error(errorMessage);
                syntaxTongue.push("ERROR");
            }
            ts.forEachChild(node, visit);
        };
        visit(this.sourceFile);
        return { syntaxTongue, errors: this.errors };
    }
}
