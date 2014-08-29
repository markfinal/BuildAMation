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
namespace VSSolutionBuilder
{
    public interface IProject
    {
        string Name
        {
            get;
        }

        string PathName
        {
            get;
        }

        System.Guid Guid
        {
            get;
        }

        System.Collections.Generic.List<string> Platforms
        {
            get;
        }

        ProjectConfigurationCollection Configurations
        {
            get;
        }

        ProjectFileCollection SourceFiles
        {
            get;
        }

        System.Collections.Generic.List<IProject> DependentProjects
        {
            get;
        }

        VisualStudioProcessor.EVisualStudioTarget VSTarget
        {
            get;
        }

        Bam.Core.UniqueList<string> References
        {
            get;
        }

        string GroupName
        {
            get;
            set;
        }

        void
        Serialize();
    }
}
