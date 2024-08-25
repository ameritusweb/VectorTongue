import * as ts from 'typescript';
import { JavaScriptSyntaxTongueConverter } from './JavaScriptSyntaxTongueConverter';

function convertJavaScriptToSyntaxTongue(fileName: string): { syntaxTongue: string[], errors: string[] } {
    const compilerOptions: ts.CompilerOptions = {
        allowJs: true,
        target: ts.ScriptTarget.ESNext,
    };

    const program = ts.createProgram([fileName], compilerOptions);
    const sourceFile = program.getSourceFile(fileName);

    if (sourceFile) {
        const converter = new JavaScriptSyntaxTongueConverter(sourceFile, program);
        return converter.convert();
    } else {
        throw new Error(`Could not find source file: ${fileName}`);
    }
}

// Example usage
const { syntaxTongue, errors } = convertJavaScriptToSyntaxTongue('path/to/your/javascript/file.js');
console.log("Syntax Tongue:");
console.log(syntaxTongue.join('\n'));
console.log("\nErrors:");
console.log(errors.join('\n'));
