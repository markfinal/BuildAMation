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
namespace QtCommon
{
    public abstract class Core :
        Base
    {
        public
        Core()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtCore_IncludePaths);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtCore_VisualCWarningLevel);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtCore_GccLongLongWarnings);
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(QtCore_LinkerOptions);
        }

        public override void
        RegisterOutputFiles(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.Target target,
            string modulePath)
        {
            this.GetModuleDynamicLibrary(target, "Qt5Core");
            base.RegisterOutputFiles(options, target, modulePath);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        QtCore_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            if (null != options)
            {
                this.AddLibraryPath(options, target);
                this.AddModuleLibrary(options, target, true, "Core");
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtCore_VisualCWarningLevel(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as VisualCCommon.ICCompilerOptions;
            if (null != options)
            {
                // QtCore headers do not compile at warning level 4
                options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtCore_GccLongLongWarnings(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (target.HasToolsetType(typeof(Gcc.Toolset)))
            {
                var options = module.Options as C.ICCompilerOptions;
                if (null != options)
                {
                    // QtCore headers do not compile with -Wall, because of
                    // QtCore/qglobal.h:720:1: error: use of C++0x long long integer constant [-Werror=long-long]
                    options.DisableWarnings.Add("long-long");
                }
            }
        }

        [C.ExportCompilerOptionsDelegate]
        void
        QtCore_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var osxOptions = module.Options as C.ICCompilerOptionsOSX;
            if (osxOptions != null)
            {
                this.AddFrameworkIncludePath(osxOptions, target);
            }
            else
            {
                var options = module.Options as C.ICCompilerOptions;
                if (null != options)
                {
                    this.AddIncludePath(options, target, "QtCore");
                }
            }
        }

        [Bam.Core.RequiredModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[] {typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(
            typeof(ICU.ICUIN),
            typeof(ICU.ICUUC),
            typeof(ICU.ICUDT)
            );
    }
}
