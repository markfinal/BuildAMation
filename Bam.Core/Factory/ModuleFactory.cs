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
namespace Bam.Core
{
    public static class ModuleFactory
    {
        public static BaseModule
        CreateModule(
            System.Type moduleType,
            Target target)
        {
            TypeUtilities.CheckTypeDerivesFrom(moduleType, typeof(BaseModule));
            TypeUtilities.CheckTypeIsNotAbstract(moduleType);

            BaseModule module = null;
            try
            {
                if (null != moduleType.GetConstructor(new System.Type[] { typeof(Target) }))
                {
                    module = System.Activator.CreateInstance(moduleType, new object[] { target }) as BaseModule;
                }
                else
                {
                    module = System.Activator.CreateInstance(moduleType) as BaseModule;
                }
            }
            catch (System.MissingMethodException)
            {
                throw new Exception("Cannot construct object of type '{0}'. Missing public constructor?", moduleType.ToString());
            }

            return module;
        }
    }
}