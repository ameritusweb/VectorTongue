namespace Vectorization.Model
{
    public class VirLoop : VirExpression
    {
        public VirVariable LoopVariable { get; set; }
        public VirExpression StartValue { get; set; }
        public VirExpression EndValue { get; set; }
        public VirExpression StepValue { get; set; }
        public VirExpression Body { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Loop:\n" +
                   $"{Indent(indent + 1)}Variable: {LoopVariable}\n" +
                   $"{Indent(indent + 1)}Start: {StartValue}\n" +
                   $"{Indent(indent + 1)}End: {EndValue}\n" +
                   $"{Indent(indent + 1)}Step: {StepValue}\n" +
                   $"{Indent(indent + 1)}Body:\n{Body.ToString(indent + 2)}";
        }
    }
}
