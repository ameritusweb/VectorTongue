namespace Vectorization.Model
{
    public class VirParameter : VirNode
    {
        public string Name { get; set; }
        public VirType Type { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}{Type} {Name}";
        }
    }
}
