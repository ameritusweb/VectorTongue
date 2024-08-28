namespace Vectorization.BranchModel
{
    public class BranchVirVariable : BranchVirExpression
    {
        public string Name { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}Variable: {Name}";
    }
}
