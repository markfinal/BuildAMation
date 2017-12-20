#region License
// Copyright (c) 2010-2017, Mark Final
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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Stack like behaviour (push/pop), but peeking is allowed anywhere along the length of the stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class PeekableStack<T>
    {
        private System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

        /// <summary>
        /// Push a new object of type T onto the end of the stack.
        /// </summary>
        /// <param name="obj">Object to push onto stack.</param>
        public void
        Push(
            T obj)
        {
            this.list.Add(obj);
        }

        /// <summary>
        /// Return the object on the end of the stack, and remove it from the stack.
        /// </summary>
        /// <returns>Object that was at the end of the stack.</returns>
        public T
        Pop()
        {
            var element = this.list.Last();
            this.list.RemoveAt(this.list.Count - 1);
            return element;
        }

        /// <summary>
        /// For compatibility with System.Collection.Generic.Stack<typeparamref name="T"/>.
        /// Return, but do not remove, the object on the end of the stack.
        /// </summary>
        /// <returns>Object that remains on the end of the stack.</returns>
        public T
        Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// More generalised peeking.
        /// Return, but do not remove, the object at the specified distance from the end of the stack.
        /// Using index = 0, is the same as calling Peek().
        /// </summary>
        /// <param name="index">Number of elements from the end of the stack to return.</param>
        /// <returns>Object, index elements from the end of the stack, but which remains on the stack.</returns>
        public T
        Peek(
            int index)
        {
            return this.list.ElementAt(this.list.Count - index - 1);
        }

        /// <summary>
        /// Returns the number of elements on the stack (read only).
        /// </summary>
        public int
        Count
        {
            get
            {
                return this.list.Count;
            }
        }

        /// <summary>
        /// Query if the stack contains the specified object.
        /// </summary>
        /// <param name="obj">Object to look for</param>
        /// <returns>true if the object is contained in the stack; false otherwise.</returns>
        public bool
        Contains(
            T obj)
        {
            return this.list.Contains(obj);
        }

        /// <summary>
        /// Convert the stack to a string, using the specified separator, from left (oldest) to right (newest).
        /// </summary>
        /// <param name="separator">Separator to use between elements.</param>
        /// <returns>Stringified stack.</returns>
        public string
        ToString(
            string separator)
        {
            return string.Join(separator, this.list);
        }
    }
}
