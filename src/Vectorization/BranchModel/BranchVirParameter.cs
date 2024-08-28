namespace Vectorization.BranchModel
{
    public class BranchVirParameter : BranchVirNode
    {
        public string Name { get; set; }
        public BranchVirType Type { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}{Type} {Name}";
    }
}
