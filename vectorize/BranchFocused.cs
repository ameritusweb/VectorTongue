using System;
using System.Collections.Generic;
using System.Linq;

namespace Vectorization.IntermediateRepresentation.BranchFocused
{
    public abstract class BranchVirNode
    {
        public abstract string ToString(int indent = 0);
        protected string Indent(int indent) => new string(' ', indent * 2);
    }

    public class BranchVirFunction : BranchVirNode
    {
        public string Name { get; set; }
        public List<BranchVirParameter> Parameters { get; set; }
        public BranchVirExpression Body { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Function {Name}:\n" +
                   $"{Indent(indent + 1)}Parameters:\n" +
                   string.Join("\n", Parameters.Select(p => p.ToString(indent + 2))) +
                   $"\n{Indent(indent + 1)}Body:\n" +
                   Body.ToString(indent + 2);
        }
    }

    public class BranchVirParameter : BranchVirNode
    {
        public string Name { get; set; }
        public BranchVirType Type { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}{Type} {Name}";
    }

    public class BranchVirType : BranchVirNode
    {
        public string TypeName { get; set; }
        public bool IsScalar { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}{TypeName}{(IsScalar ? " (Scalar)" : " (Tensor)")}";
    }

    public abstract class BranchVirExpression : BranchVirNode { }

    public class BranchVirOperation : BranchVirExpression
    {
        public string OperationName { get; set; }
        public List<BranchVirExpression> Inputs { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Operation: {OperationName}\n" +
                   $"{Indent(indent + 1)}Inputs:\n" +
                   string.Join("\n", Inputs.Select(i => i.ToString(indent + 2)));
        }
    }

    public class BranchVirBranch : BranchVirExpression
    {
        public BranchVirExpression Source { get; set; }
        public List<BranchVirExpression> Branches { get; set; }
        public BranchVirExpression Combination { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Branch:\n" +
                   $"{Indent(indent + 1)}Source:\n{Source.ToString(indent + 2)}\n" +
                   $"{Indent(indent + 1)}Branches:\n" +
                   string.Join("\n", Branches.Select(b => b.ToString(indent + 2))) +
                   $"\n{Indent(indent + 1)}Combination:\n{Combination.ToString(indent + 2)}";
        }
    }

    public class BranchVirVariable : BranchVirExpression
    {
        public string Name { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}Variable: {Name}";
    }

    public class BranchVirConstant : BranchVirExpression
    {
        public object Value { get; set; }

        public override string ToString(int indent = 0) => $"{Indent(indent)}Constant: {Value}";
    }
}