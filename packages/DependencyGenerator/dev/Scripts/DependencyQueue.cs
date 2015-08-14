#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
