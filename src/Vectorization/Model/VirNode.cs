namespace Vectorization.Model
{
    public abstract class VirNode
    {
        public abstract string ToString(int indent = 0);

        protected string Indent(int indent)
        {
            return new string(' ', indent * 2);
        }
    }
}
