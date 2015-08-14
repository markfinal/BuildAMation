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
namespace Bam.Core
{
    public static class OptionCollectionFactory
    {
        private readonly static System.Type optionCollectionType = typeof(BaseOptionCollection);

        public static BaseOptionCollection
        CreateOptionCollection(
            System.Type requiredOptionCollectionType)
        {
            if (!requiredOptionCollectionType.IsSubclassOf(optionCollectionType))
            {
                throw new Exception("Type '{0}' does not derive from the base class {1}", requiredOptionCollectionType.ToString(), optionCollectionType.ToString());
            }

            if (null == requiredOptionCollectionType.GetConstructor(new System.Type[] { typeof(DependencyNode) }))
            {
                throw new Exception("Missing constructor: '{0}(DependencyNode)'", requiredOptionCollectionType.ToString());
            }

            var optionCollection = System.Activator.CreateInstance(requiredOptionCollectionType, new object[] { null }) as BaseOptionCollection;
            return optionCollection;
        }

        public static DerivedType
        CreateOptionCollection<DerivedType>(
            DependencyNode owningNode) where DerivedType : BaseOptionCollection
        {
            var requiredOptionCollectionType = typeof(DerivedType);
            if (!requiredOptionCollectionType.IsSubclassOf(optionCollectionType))
            {
                throw new Exception("Type '{0}' does not derive from the base class {1}", requiredOptionCollectionType.ToString(), optionCollectionType.ToString());
            }

            if (null == requiredOptionCollectionType.GetConstructor(new System.Type[] { typeof(DependencyNode) }))
            {
                throw new Exception("Missing constructor: '{0}(DependencyNode)'", requiredOptionCollectionType.ToString());
            }

            var optionCollection = System.Activator.CreateInstance(requiredOptionCollectionType, new object[] { owningNode }) as BaseOptionCollection;

            var derivedOptionCollection = optionCollection as DerivedType;
            return derivedOptionCollection;
        }
    }
}