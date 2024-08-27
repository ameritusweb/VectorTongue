namespace Vectorization.IntermediateRepresentation.BranchFocused
{
    public class BranchVirOperation : BranchVirExpression
    {
        public string OperationName { get; set; }
        public List<BranchVirExpression> Inputs { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        public override string ToString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Indent(indent)}Operation: {OperationName}");
            sb.AppendLine($"{Indent(indent + 1)}Inputs:");
            foreach (var input in Inputs)
            {
                sb.AppendLine(input.ToString(indent + 2));
            }
            if (Attributes.Count > 0)
            {
                sb.AppendLine($"{Indent(indent + 1)}Attributes:");
                foreach (var attr in Attributes)
                {
                    sb.AppendLine($"{Indent(indent + 2)}{attr.Key}: {attr.Value}");
                }
            }
            return sb.ToString();
        }
    }

    public static class BranchVirOperations
    {
        // Basic arithmetic operations
        public const string Add = "Add";
        public const string Subtract = "Subtract";
        public const string Multiply = "Multiply";
        public const string Divide = "Divide";
        public const string Power = "Power";

        // Trigonometric operations
        public const string Sin = "Sin";
        public const string Cos = "Cos";
        public const string Tan = "Tan";
        public const string Asin = "Asin";
        public const string Acos = "Acos";
        public const string Atan = "Atan";

        // Exponential and logarithmic operations
        public const string Exp = "Exp";
        public const string Log = "Log";
        public const string Log10 = "Log10";

        // Matrix operations
        public const string MatMul = "MatMul";
        public const string Transpose = "Transpose";
        public const string Inverse = "Inverse";
        public const string Determinant = "Determinant";

        // Element-wise operations
        public const string ElementWiseMultiply = "ElementWiseMultiply";
        public const string ElementWiseDivide = "ElementWiseDivide";

        // Reduction operations
        public const string Sum = "Sum";
        public const string Mean = "Mean";
        public const string Max = "Max";
        public const string Min = "Min";

        // Shape operations
        public const string Reshape = "Reshape";
        public const string Concat = "Concat";
        public const string Split = "Split";

        // Neural network operations
        public const string Convolution = "Convolution";
        public const string MaxPool = "MaxPool";
        public const string ReLU = "ReLU";
        public const string Sigmoid = "Sigmoid";
        public const string Tanh = "Tanh";
        public const string Softmax = "Softmax";
        public const string BatchNormalization = "BatchNormalization";
        public const string Dropout = "Dropout";

        // Optimization operations
        public const string GradientDescent = "GradientDescent";
        public const string Adam = "Adam";

        // Composition operation
        public const string Compose = "Compose";

        // Merged branch operation
        public const string MergedBranch = "MergedBranch";
    }
}