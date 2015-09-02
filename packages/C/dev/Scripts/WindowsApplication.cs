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
using Bam.Core.V2;
namespace C
{
namespace V2
{
    public class GUIApplication :
        ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.V2.ILinkerOptionsWin;
                    if (linker != null)
                    {
                        linker.SubSystem = ESubsystem.Windows;
                    }
                });
        }

        protected Bam.Core.V2.Module.PrivatePatchDelegate WindowsPreprocessor = settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Remove("_CONSOLE");
                compiler.PreprocessorDefines.Add("_WINDOWS");
            };

        public override CObjectFileCollection
        CreateCSourceContainer()
        {
            var container = base.CreateCSourceContainer();
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                container.PrivatePatch(this.WindowsPreprocessor);
            }
            return container;
        }
    }
}
    namespace Cxx
    {
    namespace V2
    {
        public class GUIApplication :
            C.V2.GUIApplication
        {
            public override Cxx.V2.ObjectFileCollection
            CreateCxxSourceContainer()
            {
                var container = base.CreateCxxSourceContainer();
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    container.PrivatePatch(this.WindowsPreprocessor);
                }
                return container;
            }
        }
    }
    }
    /// <summary>
    /// C/C++ Windows application
    /// </summary>
    public class WindowsApplication : Application
    {
        [LocalCompilerOptionsDelegate]
        private static void WindowsApplicationCompilerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_WINDOWS");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void WindowApplicationLinkerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = ESubsystem.Windows;
            }
        }
    }
}