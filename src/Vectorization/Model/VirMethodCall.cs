namespace Vectorization.Model
{
    public class VirMethodCall : VirExpression
    {
        public string MethodName { get; set; }
        public List<VirExpression> Arguments { get; set; } = new List<VirExpression>();

        public override string ToString(int indent = 0)
        {
            var result = $"{Indent(indent)}MethodCall: {MethodName}\n";
            result += $"{Indent(indent + 1)}Arguments:\n";
            foreach (var arg in Arguments)
            {
                result += arg.ToString(indent + 2) + "\n";
            }
            return result;
        }
    }
}
