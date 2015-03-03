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
namespace VisualCCommon
{
    // TODO: extend with runtime library option
    public interface ICCompilerOptions
    {
        bool NoLogo
        {
            get;
            set;
        }

        bool MinimalRebuild
        {
            get;
            set;
        }

        VisualCCommon.EWarningLevel WarningLevel
        {
            get;
            set;
        }

        VisualCCommon.EDebugType DebugType
        {
            get;
            set;
        }

        VisualCCommon.EBrowseInformation BrowseInformation
        {
            get;
            set;
        }

        bool StringPooling
        {
            get;
            set;
        }

        bool DisableLanguageExtensions
        {
            get;
            set;
        }

        bool ForceConformanceInForLoopScope
        {
            get;
            set;
        }

        bool UseFullPaths
        {
            get;
            set;
        }

        EManagedCompilation CompileAsManaged
        {
            get;
            set;
        }

        EBasicRuntimeChecks BasicRuntimeChecks
        {
            get;
            set;
        }

        bool SmallerTypeConversionRuntimeCheck
        {
            get;
            set;
        }

        EInlineFunctionExpansion InlineFunctionExpansion
        {
            get;
            set;
        }

        bool EnableIntrinsicFunctions
        {
            get;
            set;
        }

        ERuntimeLibrary RuntimeLibrary
        {
            get;
            set;
        }

        Bam.Core.StringArray ForcedInclude
        {
            get;
            set;
        }
    }
}
