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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.ArchiverOptionCollection;

            var data = new QMakeData(node);
            foreach (var child in node.Children)
            {
                var childData = child.Data as QMakeData;
                if (null != childData)
                {
                    data.Merge(childData);
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependent in node.ExternalDependents)
                {
                    var depData = dependent.Data as QMakeData;
                    if (null != depData)
                    {
                        data.Merge(depData, QMakeData.OutputType.StaticLibrary | QMakeData.OutputType.DynamicLibrary | QMakeData.OutputType.HeaderLibrary);
                    }
                }
            }

            // find headers
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    data.Headers.AddRangeUnique(headerFileCollection.ToStringArray());
                }
            }

            data.Target = options.OutputName;
            data.Output = QMakeData.OutputType.StaticLibrary;
            data.DestDir = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey];

            success = true;
            return data;
        }
    }
}
