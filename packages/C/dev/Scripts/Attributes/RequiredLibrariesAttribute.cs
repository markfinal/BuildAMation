#region License
// Copyright 2010-2015 Mark Final
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
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredLibrariesAttribute :
        Bam.Core.BaseTargetFilteredAttribute,
        Bam.Core.IFieldAttributeProcessor
    {
        void
        Bam.Core.IFieldAttributeProcessor.Execute(
            object sender,
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (!Bam.Core.TargetUtilities.MatchFilters(target, this))
            {
                return;
            }

            var field = sender as System.Reflection.FieldInfo;
            string[] libraries = null;
            if (field.GetValue(module) is Bam.Core.Array<string>)
            {
                libraries = (field.GetValue(module) as Bam.Core.Array<string>).ToArray();
            }
            else if (field.GetValue(module) is string[])
            {
                libraries = field.GetValue(module) as string[];
            }
            else
            {
                throw new Bam.Core.Exception("RequiredLibrary field in {0} must be of type string[], Bam.Core.StringArray or Bam.Core.Array<string>", module.ToString());
            }

            module.UpdateOptions += delegate(Bam.Core.IModule dlgModule, Bam.Core.Target dlgTarget)
            {
                var linkerOptions = dlgModule.Options as ILinkerOptions;
                foreach (var library in libraries)
                {
                    linkerOptions.Libraries.Add(library);
                }
            };
        }
    }
}
