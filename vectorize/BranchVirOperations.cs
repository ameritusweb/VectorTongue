public static class BranchVirOperations
{
    // ... (existing operation constants)

    private static readonly HashSet<string> ValidOperations = new HashSet<string>
    {
        Add, Subtract, Multiply, Divide, Power, Sin, Cos, Tan, Exp, Log, Sqrt, Abs, Negate, MatMul, Parallel, MergedBranch
        // Add any other valid operations here
    };

    public static bool IsValidOperation(string operationName)
    {
        return ValidOperations.Contains(operationName);
    }
}