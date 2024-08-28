namespace Vectorization.Model
{
    public class VirUnaryOperation : VirExpression
    {
        public VirExpression Operand { get; set; }
        public VirUnaryOperator Operator { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}UnaryOperation:\n" +
                   $"{Indent(indent + 1)}Operator: {Operator}\n" +
                   $"{Indent(indent + 1)}Operand:\n{Operand.ToString(indent + 2)}";
        }
    }

}
