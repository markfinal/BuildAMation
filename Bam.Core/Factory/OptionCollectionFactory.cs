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
#endregion
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