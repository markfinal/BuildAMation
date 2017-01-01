#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace XcodeBuilder
{
    public class XcodeMeta :
        Bam.Core.IBuildModeMetaData
    {
        public static void
        PreExecution()
        {
            Bam.Core.Graph.Instance.MetaData = new WorkspaceMeta();
        }

        public static void
        PostExecution()
        {
            // TODO: some alternatives
            // all modules in the same namespace -> targets in the .xcodeproj
            // one .xcodeproj for all modules -> each a target
            // one project per module, each with one target

            var workspaceMeta = Bam.Core.Graph.Instance.MetaData as WorkspaceMeta;
            var workspaceDir = workspaceMeta.Serialize();
            var workspaceSettings = new WorkspaceSettings(workspaceDir);
            workspaceSettings.Serialize();

            Bam.Core.Log.Info("Successfully created Xcode workspace for package '{0}'\n\t{1}", Bam.Core.Graph.Instance.MasterPackage.Name, workspaceDir);
        }

        Bam.Core.TokenizedString
        Bam.Core.IBuildModeMetaData.ModuleOutputDirectory(
            Bam.Core.Module currentModule,
            Bam.Core.Module encapsulatingModule)
        {
            // Note:
            // 1: only using the current configuration because Xcode is rather limited in the directory hierarchy of where to write files to,
            //    otherwise files cannot be found. Keep to the lowest common denominator
            // 2: not using Bam.Core.Module.ModuleSpecificOutputSubDirectory for the same reason
            return Bam.Core.TokenizedString.CreateVerbatim(currentModule.BuildEnvironment.Configuration.ToString());
        }

        bool Bam.Core.IBuildModeMetaData.PublishBesideExecutable
        {
            get
            {
                return true;
            }
        }

        bool Bam.Core.IBuildModeMetaData.CanCreatePrebuiltProjectForAssociatedFiles
        {
            get
            {
                return false;
            }
        }
    }
}
