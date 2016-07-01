using Bam.Core;
namespace CyclicDependenciesTest1
{
    class A :
        Bam.Core.Module
    {
        protected override void Init(Module parent)
        {
            base.Init(parent);

            var depB = Bam.Core.Graph.Instance.FindReferencedModule<B>();
            this.DependsOn(depB);
        }

        public override void Evaluate()
        {
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
        }

        protected override void GetExecutionPolicy(string mode)
        {
        }
    }

    class B :
        Bam.Core.Module
    {
        protected override void Init(Module parent)
        {
            base.Init(parent);

            var depA = Bam.Core.Graph.Instance.FindReferencedModule<A>();
            this.DependsOn(depA);
        }

        public override void Evaluate()
        {
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
        }

        protected override void GetExecutionPolicy(string mode)
        {
        }
    }

    sealed class C :
        Bam.Core.Module
    {
        protected override void Init(Module parent)
        {
            base.Init(parent);

            var depB = Bam.Core.Graph.Instance.FindReferencedModule<B>();
            this.DependsOn(depB);
        }

        public override void Evaluate()
        {
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
        }

        protected override void GetExecutionPolicy(string mode)
        {
        }
    }
}
