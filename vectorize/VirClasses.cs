using System;
using System.Collections.Generic;

namespace Vectorization.IntermediateRepresentation
{
    public abstract class VirNode
    {
        public abstract string ToString(int indent = 0);

        protected string Indent(int indent)
        {
            return new string(' ', indent * 2);
        }
    }

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

    public class VirParameter : VirNode
    {
        public string Name { get; set; }
        public VirType Type { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}{Type} {Name}";
        }
    }

    public class VirType : VirNode
    {
        public string TypeName { get; set; }
        public bool IsScalar { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}{TypeName}{(IsScalar ? " (Scalar)" : " (Tensor)")}";
        }
    }

    public abstract class VirExpression : VirNode { }

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

    public enum ReductionType
    {
        Sum,
        Product,
        Max,
        Min
    }

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

    public enum VirOperator
    {
        Add, Subtract, Multiply, Divide, Power,
        Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual
    }

    public enum VirUnaryOperator
    {
        Negate, Abs, Sqrt, Plus, LogicalNot
    }

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

    public class VirVariable : VirExpression
    {
        public string Name { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Variable: {Name}";
        }
    }

    public class VirConstant : VirExpression
    {
        public object Value { get; set; }

        public override string ToString(int indent = 0)
        {
            return $"{Indent(indent)}Constant: {Value}";
        }
    }
}