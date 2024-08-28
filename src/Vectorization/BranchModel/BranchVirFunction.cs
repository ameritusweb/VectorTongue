namespace Vectorization.BranchModel
{
    public class BranchVirFunction : BranchVirNode
    {
        public string Name { get; set; }
        public List<BranchVirParameter> Parameters { get; set; }
        public BranchVirExpression Body { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Function {Name}:\n" +
                   $"{Indent(indent + 1)}Parameters:\n" +
                   string.Join("\n", Parameters.Select(p => p.ToString(indent + 2))) +
                   $"\n{Indent(indent + 1)}Body:\n" +
                   Body.ToString(indent + 2);
        }
    }
}
