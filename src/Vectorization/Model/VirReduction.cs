namespace Vectorization.Model
{
    public class VirReduction : VirExpression
    {
        public ReductionType Type { get; set; }
        public VirExpression Expression { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Reduction:\n" +
                   $"{Indent(indent + 1)}Type: {Type}\n" +
                   $"{Indent(indent + 1)}Expression:\n{Expression.ToString(indent + 2)}";
        }
    }
}
