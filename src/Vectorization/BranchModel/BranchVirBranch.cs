namespace Vectorization.BranchModel
{
    public class BranchVirBranch : BranchVirExpression
    {
        public BranchVirExpression Source { get; set; }
        public List<BranchVirExpression> Branches { get; set; }
        public BranchVirExpression Combination { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Branch:\n" +
                   $"{Indent(indent + 1)}Source:\n{Source.ToString(indent + 2)}\n" +
                   $"{Indent(indent + 1)}Branches:\n" +
                   string.Join("\n", Branches.Select(b => b.ToString(indent + 2))) +
                   $"\n{Indent(indent + 1)}Combination:\n{Combination.ToString(indent + 2)}";
        }
    }
}
