// <copyright file="DependencyQueue.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DependencyGenerator package</summary>
// <author>Mark Final</author>
namespace DependencyGenerator
{
    public class DependencyQueue<Data> where Data : class
    {
        private System.Collections.Generic.Queue<Data> data = new System.Collections.Generic.Queue<Data>();
        private System.Threading.ManualResetEvent alive = new System.Threading.ManualResetEvent(false);

        public
        DependencyQueue(
            bool isThreaded)
        {
            this.IsThreaded = isThreaded;
        }

        private bool IsThreaded
        {
            get;
            set;
        }

        public System.Threading.ManualResetEvent IsAlive
        {
            get
            {
                return this.alive;
            }
        }

        public void
        Enqueue(
            Data value)
        {
            if (this.IsThreaded)
            {
                lock (this.data)
                {
                    this.data.Enqueue(value);
                    this.alive.Set();
                }
            }
            else
            {
                // do the work now!
                var cast = value as IncludeDependencyGeneration.Data;
                IncludeDependencyGeneration.GenerateDepFile(cast, IncludeDependencyGeneration.Style.Opus);
            }
        }

        public void
        Enqueue(
            Bam.Core.Array<Data> values)
        {
            if (this.IsThreaded)
            {
                lock (this.data)
                {
                    // TODO: change to var?
                    foreach (Data value in values)
                    {
                        this.data.Enqueue(value);
                    }
                    this.alive.Set();
                }
            }
            else
            {
                // do the work now!
                foreach (var value in values)
                {
                    var cast = value as IncludeDependencyGeneration.Data;
                    IncludeDependencyGeneration.GenerateDepFile(cast, IncludeDependencyGeneration.Style.Opus);
                }
            }
        }

        public Data
        Dequeue()
        {
            Data result = null;
            lock (this.data)
            {
                if (1 == this.data.Count)
                {
                    this.alive.Reset();
                }

                result = this.data.Dequeue();
            }
            return result;
        }
    }
}
