namespace Vectorization.BranchModel
{
    public class BranchVirConstant : BranchVirExpression
    {
        public object Value { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}Constant: {Value}";
    }
}
