namespace Vectorization.Model
{
    public class VirType : VirNode
    {
        public string TypeName { get; set; }
        public bool IsScalar { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}{TypeName}{(IsScalar ? " (Scalar)" : " (Tensor)")}";
        }
    }
}
