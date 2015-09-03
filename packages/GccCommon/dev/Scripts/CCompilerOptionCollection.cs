#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace GccCommon
{
namespace V2
{
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void
            Defaults(this GccCommon.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
            {
                settings.PositionIndependentCode = false;
            }

            public static void
            Defaults(this GccCommon.V2.ICommonLinkerOptions settings, Bam.Core.V2.Module module)
            {
                settings.CanUseOrigin = false;
                settings.RPath = new Bam.Core.StringArray();
                settings.RPathLink = new Bam.Core.StringArray();
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this GccCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.PositionIndependentCode)
            {
                if (true == options.PositionIndependentCode)
                {
                    commandLine.Add("-fPIC");
                }
            }
        }

        public static void
        Convert(
            this GccCommon.V2.ICommonLinkerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.CanUseOrigin)
            {
                if (true == options.CanUseOrigin)
                {
                    commandLine.Add("-Wl,-z,origin");
                }
            }
            foreach (var rpath in options.RPath)
            {
                commandLine.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
            foreach (var rpath in options.RPathLink)
            {
                commandLine.Add(System.String.Format("-Wl,-rpath-link,{0}", rpath));
            }
        }
    }
}
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var localCompilerOptions = this as ICCompilerOptions;
            localCompilerOptions.AllWarnings = true;
            localCompilerOptions.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            var target = node.Target;
            localCompilerOptions.SixtyFourBit = Bam.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                localCompilerOptions.StrictAliasing = false;
                localCompilerOptions.InlineFunctions = false;
            }
            else
            {
                localCompilerOptions.StrictAliasing = true;
                localCompilerOptions.InlineFunctions = true;
            }

            localCompilerOptions.PositionIndependentCode = false;

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)node.Target));

            cCompilerOptions.TargetLanguage = C.ETargetLanguage.C;

            localCompilerOptions.Pedantic = true;

            if (null != node.Children)
            {
                // we use gcc as the compile - if there is ObjectiveC code included, disable ignoring standard include paths as it gets complicated otherwise
                foreach (var child in node.Children)
                {
                    if (child.Module is C.ObjC.ObjectFile || child.Module is C.ObjC.ObjectFileCollection ||
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        cCompilerOptions.IgnoreStandardIncludePaths = false;
                        break;
                    }
                }
            }
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
