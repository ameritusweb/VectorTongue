using System.Text;
using Vectorization.BranchModel;

namespace Vectorization.Optimizers
{
    public static class ExpressionKeyGenerator
    {
        public static string GenerateKey(BranchVirExpression expression)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashInput = GenerateHashInput(expression);
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static string GenerateHashInput(BranchVirExpression expression)
        {
            return expression switch
            {
                BranchVirConstant constant => $"C:{constant.Value}",
                BranchVirVariable variable => $"V:{variable.Name}",
                BranchVirOperation operation => GenerateOperationHashInput(operation),
                _ => throw new ArgumentException($"Unsupported expression type: {expression.GetType()}")
            };
        }

        private static string GenerateOperationHashInput(BranchVirOperation operation)
        {
            var inputHashes = operation.Inputs.Select(GenerateHashInput);
            return $"O:{operation.OperationName}({string.Join(",", inputHashes)})";
        }
    }
}
