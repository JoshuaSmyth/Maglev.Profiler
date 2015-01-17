using System.Linq;

namespace MaglevProfiler
{
    public class IntervalTree
    {
        public IntervalNode IntervalRoot;
        public IntervalNode CurrentNode;

        public IntervalTree()
        {
            
        }

        public IntervalNode FindNodeByThreadId(int threadId)
        {
            return FindNodeByThreadId(threadId, IntervalRoot);
        }

        public IntervalNode FindNodeByThreadId(int threadId, IntervalNode root)
        {
            if (root.ManagedThreadId == threadId)
                return root;

            return root.Children.Select(child => FindNodeByThreadId(threadId, child)).FirstOrDefault();
        }
    }
}
