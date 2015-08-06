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
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this VisualCCommon.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
        {
            settings.NoLogo = true;
        }

        public static void
        Delta(
            this VisualCCommon.V2.ICommonCompilerOptions settings,
            VisualCCommon.V2.ICommonCompilerOptions delta,
            VisualCCommon.V2.ICommonCompilerOptions other)
        {
            if (settings.NoLogo != other.NoLogo)
            {
                delta.NoLogo = settings.NoLogo;
            }
        }

        public static void
        Clone(
            this VisualCCommon.V2.ICommonCompilerOptions settings,
            VisualCCommon.V2.ICommonCompilerOptions other)
        {
            settings.NoLogo = other.NoLogo;
        }
    }
}

    [Bam.Core.V2.SettingsExtensions(typeof(VisualCCommon.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        bool? NoLogo
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        int VCCommonCOnly
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICxxOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        string VCCommonCxxOnly
        {
            get;
            set;
        }
    }
}
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
