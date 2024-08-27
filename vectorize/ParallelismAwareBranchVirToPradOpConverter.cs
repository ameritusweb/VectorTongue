using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vectorization.IntermediateRepresentation.BranchFocused;

namespace Vectorization.Converters
{
    public class ParallelismAwareBranchVirToPradOpConverter : BranchVirToPradOpConverter
    {
        protected override string ConvertExpression(BranchVirExpression expression, StringBuilder sb, int indent)
        {
            if (expression is BranchVirOperation operation && operation.OperationName == "Parallel")
            {
                return ConvertParallelOperation(operation, sb, indent);
            }
            
            return base.ConvertExpression(expression, sb, indent);
        }

        private string ConvertParallelOperation(BranchVirOperation operation, StringBuilder sb, int indent)
        {
            var resultVar = GetTempVarName();
            var inputs = operation.Inputs.Select(input => ConvertExpression(input, sb, indent)).ToList();
            var combination = ConvertExpression((BranchVirExpression)operation.Attributes["Combination"], sb, indent);

            sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = PradOp.DoParallel(");
            for (int i = 0; i < inputs.Count; i++)
            {
                sb.AppendLine($"{new string(' ', (indent + 1) * 4)}x => {inputs[i]}{(i < inputs.Count - 1 ? "," : "")}");
            }
            sb.AppendLine($"{new string(' ', indent * 4)})).Then(results =>");
            sb.AppendLine($"{new string(' ', (indent + 1) * 4)}{{");
            sb.AppendLine($"{new string(' ', (indent + 2) * 4)}return {combination};");
            sb.AppendLine($"{new string(' ', (indent + 1) * 4)}}}");
            sb.AppendLine($"{new string(' ', indent * 4)}));");

            return resultVar;
        }
    }
}