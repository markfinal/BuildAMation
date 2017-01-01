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
namespace Bam.Core
{
    /// <summary>
    /// Iterating over interfaces for a concrete settings class does not return those interfaces in a
    /// deterministic order (as defined by the C# GetInterfaces() function). Since some interfaces contain
    /// properties that do need to be provided in a specific order when serialized as a command line, e.g.
    /// warning suppressions and enabling, each interface can specify a precedence, and this is used to force
    /// the order in which they are processed.
    /// The default precedence is assumed to be zero.
    /// A positive precedence will result in the interface being processed earlier than others.
    /// A negative precedence will result in the interface being processed later than others.
    /// Any two interfaces with the same precedence may be processed in any order.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public sealed class SettingsPrecedenceAttribute :
        System.Attribute
    {
        /// <summary>
        /// Create an instance of the attribute.
        /// </summary>
        /// <param name="order">The precedence value.</param>
        public SettingsPrecedenceAttribute(
            int order)
        {
            this.Order = order;
        }

        /// <summary>
        /// Retrieve the precedence value.
        /// </summary>
        /// <value>Precedence</value>
        public int Order
        {
            get;
            private set;
        }
    }
}
