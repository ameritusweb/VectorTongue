namespace Vectorization.Model
{
    public class VirFunction : VirNode
    {
        public string Name { get; set; }
        public List<VirParameter> Parameters { get; set; } = new List<VirParameter>();
        public VirExpression Body { get; set; }

        public override string ToString(int indent = 0)
        {
            var result = $"{Indent(indent)}Function {Name}:\n";
            result += $"{Indent(indent + 1)}Parameters:\n";
            foreach (var param in Parameters)
            {
                result += param.ToString(indent + 2) + "\n";
            }
            result += $"{Indent(indent + 1)}Body:\n";
            result += Body.ToString(indent + 2);
            return result;
        }
    }
}
