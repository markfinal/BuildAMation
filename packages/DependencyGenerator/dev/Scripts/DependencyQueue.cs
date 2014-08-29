#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
                IncludeDependencyGeneration.GenerateDepFile(cast, IncludeDependencyGeneration.Style.Bam);
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
                    IncludeDependencyGeneration.GenerateDepFile(cast, IncludeDependencyGeneration.Style.Bam);
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
