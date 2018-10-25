#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace C
{
    public abstract class CompilerVersion
    {
        public sealed class IncompatibleTypesException :
            Bam.Core.Exception
        {
            public IncompatibleTypesException(
                System.Type baseType,
                System.Type comparingType)
                :
                base($"Unable to compare compiler versions of different types: '{baseType.ToString()}' vs '{comparingType.ToString()}'")
            {
                this.BaseType = baseType;
                this.ComparingType = comparingType;
            }

            public System.Type BaseType
            {
                get;
                private set;
            }

            public System.Type ComparingType
            {
                get;
                private set;
            }
        }

        protected int combinedVersion;

        public bool
        Match(
            CompilerVersion compare)
        {
            return this.combinedVersion == (compare as CompilerVersion).combinedVersion;
        }

        public bool
        AtLeast(
            CompilerVersion minimum)
        {
            if (this.GetType() != minimum.GetType())
            {
                throw new IncompatibleTypesException(this.GetType(), minimum.GetType());
            }
            return this.combinedVersion >= (minimum as CompilerVersion).combinedVersion;
        }

        public bool
        AtMost(
            CompilerVersion maximum)
        {
            if (this.GetType() != maximum.GetType())
            {
                throw new IncompatibleTypesException(this.GetType(), maximum.GetType());
            }
            return this.combinedVersion <= (maximum as CompilerVersion).combinedVersion;
        }

        public bool
        InRange(
            CompilerVersion minimum,
            CompilerVersion maximum)
        {
            return this.AtLeast(minimum) &&
                   this.AtMost(maximum);
        }

        public override string
        ToString()
        {
            return combinedVersion.ToString();
        }
    }

    public interface IToolchainDiscovery
    {
        void
        discover(
            EBit? depth);
    }
}
