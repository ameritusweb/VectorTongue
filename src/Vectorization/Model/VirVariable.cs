namespace Vectorization.Model
{
    public class VirVariable : VirExpression
    {
        public string Name { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Variable: {Name}";
        }
    }
}
