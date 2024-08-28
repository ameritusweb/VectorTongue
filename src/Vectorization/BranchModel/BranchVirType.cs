namespace Vectorization.BranchModel
{
    public class BranchVirType : BranchVirNode
    {
        public string TypeName { get; set; }
        public bool IsScalar { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}{TypeName}{(IsScalar ? " (Scalar)" : " (Tensor)")}";
    }
}
