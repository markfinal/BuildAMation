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

        public System.Threading.ManualResetEvent IsAlive
        {
            get
            {
                return this.alive;
            }
        }

        public void Enqueue(Data value)
        {
            lock (this.data)
            {
                this.data.Enqueue(value);
                this.alive.Set();
            }
        }

        public void Enqueue(Opus.Core.Array<Data> values)
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

        public Data Dequeue()
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
