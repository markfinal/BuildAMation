#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace CodeGenTest
{
    namespace DefaultSettings
    {
        public static class DefaultSettingsExtensions
        {
            public static void
            Defaults(
                this IGeneratedSourceSettings settings,
                Bam.Core.Module module)
            {
                settings.Keyword = module.Macros["GenerateBasename"].ToString();
            }
        }
    }

    [Bam.Core.SettingsExtensions(typeof(DefaultSettings.DefaultSettingsExtensions))]
    public interface IGeneratedSourceSettings :
        Bam.Core.ISettingsBase
    {
        string Keyword
        {
            get;
            set;
        }
    }

    [CommandLineProcessor.OutputPath(GeneratedSourceModule.SourceFileKey, " ")]
    public sealed class GeneratedSourceSettings :
        Bam.Core.Settings,
        IGeneratedSourceSettings
    {
        public GeneratedSourceSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.String("")]
        string IGeneratedSourceSettings.Keyword
        {
            get;
            set;
        }

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Inputs_Outputs_Cmds;
        }
    }
}
