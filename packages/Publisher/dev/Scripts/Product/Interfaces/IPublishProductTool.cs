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
namespace Publisher
{
    static class ToolsetProvider
    {
        static string GetToolsetName(System.Type toolType)
        {
            return "Publish";
        }
    }

    public class ExportPublishOptionsDelegateAttribute :
        System.Attribute
    {}

    public class LocalPublishOptionsDelegateAttribute :
        System.Attribute
    {}

    [Bam.Core.LocalAndExportTypes(typeof(LocalPublishOptionsDelegateAttribute),
                                   typeof(ExportPublishOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetToolsetName")]
    public interface IPublishProductTool
    {}
}
