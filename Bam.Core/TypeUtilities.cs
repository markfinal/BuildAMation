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
    public static class TypeUtilities
    {
        public static void
        CheckTypeDerivesFrom(
            System.Type type,
            System.Type baseClass)
        {
            if (!baseClass.IsAssignableFrom(type))
            {
                throw new Exception("Type '{0}' is not derived from {1}", type.ToString(), baseClass.ToString());
            }
        }

        public static void
        CheckTypeImplementsInterface(
            System.Type type,
            System.Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("Type '{0}' is not an interface", interfaceType.ToString());
            }

            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new Exception("Type '{0}' does not implement the interface {1}", type.ToString(), interfaceType.ToString());
            }
        }

        public static void
        CheckTypeIsNotAbstract(
            System.Type type)
        {
            if (type.IsAbstract)
            {
                throw new Exception("Type '{0}' is abstract", type.ToString());
            }
        }
    }
}
