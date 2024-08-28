namespace Vectorization.BranchModel
{
    public abstract class BranchVirNode
    {
        public abstract string ToString(int indent = 0);
        protected string Indent(int indent) => new string(' ', indent * 2);
    }
}
