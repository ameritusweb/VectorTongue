namespace Vectorization.Model
{
    public class VirConditional : VirExpression
    {
        public VirExpression Condition { get; set; }
        public VirExpression TrueBranch { get; set; }
        public VirExpression FalseBranch { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Conditional:\n" +
                   $"{Indent(indent + 1)}Condition:\n{Condition.ToString(indent + 2)}\n" +
                   $"{Indent(indent + 1)}TrueBranch:\n{TrueBranch.ToString(indent + 2)}\n" +
                   $"{Indent(indent + 1)}FalseBranch:\n{FalseBranch.ToString(indent + 2)}";
        }
    }
}
