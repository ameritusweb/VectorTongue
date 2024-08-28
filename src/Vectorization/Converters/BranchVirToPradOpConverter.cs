using System.Text;
using Vectorization.BranchModel;

namespace Vectorization.Converters
{
    public class BranchVirToPradOpConverter
    {
        private Dictionary<string, string> _variableNames = new Dictionary<string, string>();
        private HashSet<string> _inputVariables = new HashSet<string>();
        private int _tempVarCounter = 0;

        public string Convert(BranchVirFunction function)
        {
            _variableNames.Clear();
            _inputVariables.Clear();
            _tempVarCounter = 0;

            var sb = new StringBuilder();
            sb.AppendLine($"public static (PradResult Result, Dictionary<string, PradResult> InputGradients) {function.Name}(");
            sb.AppendLine(string.Join(",\n", function.Parameters.Select(p => $"    PradOp {p.Name}")));
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLine("    var inputGradients = new Dictionary<string, PradResult>();");

            foreach (var param in function.Parameters)
            {
                _variableNames[param.Name] = param.Name;
                _inputVariables.Add(param.Name);
                sb.AppendLine($"    PradResult? {param.Name}Result = null;");
            }

            string resultVar = ConvertExpression(function.Body, sb, 1);

            sb.AppendLine($"    return (Result: {resultVar}, InputGradients: inputGradients);");
            sb.AppendLine("}");

            return sb.ToString();
        }

        protected string ConvertExpression(BranchVirExpression expression, StringBuilder sb, int indent)
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

        private string ConvertOperation(BranchVirOperation operation, StringBuilder sb, int indent)
        {
            var inputs = operation.Inputs.Select(input => ConvertExpression(input, sb, indent)).ToList();
            string resultVar = GetTempVarName();

            switch (operation.OperationName)
            {
                case "Add":
                case "Subtract":
                case "Multiply":
                case "Divide":
                case "MatMul":
                case "Power":
                    sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {inputs[0]}.Then(result => result.Then(PradOp.{operation.OperationName}Op, {inputs[1]}.Result));");
                    break;
                case "Negate":
                case "Abs":
                case "Sqrt":
                case "Exp":
                case "Log":
                case "Sin":
                case "Cos":
                case "Tan":
                    sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {inputs[0]}.Then(PradOp.{operation.OperationName}Op);");
                    break;
                case "Reshape":
                case "Transpose":
                case "Sum":
                case "Mean":
                case "Max":
                case "Min":
                    sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {inputs[0]}.Then(result => result.Then(PradOp.{operation.OperationName}Op, {string.Join(", ", inputs.Skip(1))}));");
                    break;
                case "Compose":
                    sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {inputs[0]}.Then(result => {inputs[1]});");
                    break;
                default:
                    throw new NotImplementedException($"Operation {operation.OperationName} is not implemented.");
            }

            return resultVar;
        }

        private string ConvertBranch(BranchVirBranch branch, StringBuilder sb, int indent)
        {
            string sourceVar = ConvertExpression(branch.Source, sb, indent);
            string resultVar = GetTempVarName();

            sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {sourceVar}.ThenParallel(");

            for (int i = 0; i < branch.Branches.Count; i++)
            {
                string branchVar = ConvertExpression(branch.Branches[i], sb, indent + 1);
                sb.AppendLine($"{new string(' ', (indent + 1) * 4)}r => {branchVar}{(i < branch.Branches.Count - 1 ? "," : "")}");
            }

            sb.AppendLine($"{new string(' ', indent * 4)})).Then(results =>");
            sb.AppendLine($"{new string(' ', (indent + 1) * 4)}{{");

            string combinationVar = ConvertExpression(branch.Combination, sb, indent + 2);

            sb.AppendLine($"{new string(' ', (indent + 1) * 4)}    return {combinationVar};");
            sb.AppendLine($"{new string(' ', (indent + 1) * 4)}}}");
            sb.AppendLine($"{new string(' ', indent * 4)}));");

            return resultVar;
        }

        private string ConvertVariable(BranchVirVariable variable, StringBuilder sb, int indent)
        {
            if (_inputVariables.Contains(variable.Name))
            {
                string resultVar = GetTempVarName();
                sb.AppendLine($"{new string(' ', indent * 4)}{variable.Name}Result = {variable.Name}.Result;");
                sb.AppendLine($"{new string(' ', indent * 4)}inputGradients[\"{variable.Name}\"] = {variable.Name}Result;");
                sb.AppendLine($"{new string(' ', indent * 4)}var {resultVar} = {variable.Name}Result;");
                return resultVar;
            }
            return _variableNames[variable.Name];
        }

        private string GetTempVarName() => $"temp{_tempVarCounter++}";
    }
}
