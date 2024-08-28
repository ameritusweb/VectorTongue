namespace Vectorization.Model
{
    public class VirConstant : VirExpression
    {
        public object Value { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Constant: {Value}";
        }
    }
}
