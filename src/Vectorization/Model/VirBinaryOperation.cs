namespace Vectorization.Model
{
    public class VirBinaryOperation : VirExpression
    {
        public VirExpression Left { get; set; }
        public VirExpression Right { get; set; }
        public VirOperator Operator { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}BinaryOperation:\n" +
                   $"{Indent(indent + 1)}Operator: {Operator}\n" +
                   $"{Indent(indent + 1)}Left:\n{Left.ToString(indent + 2)}\n" +
                   $"{Indent(indent + 1)}Right:\n{Right.ToString(indent + 2)}";
        }
    }
}
