namespace Vectorization.BranchModel
{
    public class BranchVirOperation : BranchVirExpression
    {
        public string OperationName { get; set; }
        public List<BranchVirExpression> Inputs { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Operation: {OperationName}\n" +
                   $"{Indent(indent + 1)}Inputs:\n" +
                   string.Join("\n", Inputs.Select(i => i.ToString(indent + 2)));
        }
    }
}
