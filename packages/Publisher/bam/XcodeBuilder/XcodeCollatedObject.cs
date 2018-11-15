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
using System.Linq;
namespace Publisher
{
    public static partial class XcodeSupport
    {
        public static void
        CollateObject(
            CollatedObject module)
        {
            if (module.Ignore)
            {
                return;
            }

            var collatedInterface = module as ICollatedObject;
            var targetModule = collatedInterface.SourceModule;
            var arePostBuildCommands = true;
            if (null == targetModule.MetaData)
            {
                if (null != collatedInterface.Anchor)
                {
                    // this can happen for prebuilt frameworks
                    targetModule = collatedInterface.Anchor.SourceModule;
                }
                else
                {
                    if (collatedInterface.SourceModule is PreExistingObject)
                    {
                        targetModule = (collatedInterface.SourceModule as PreExistingObject).ParentOfCollationModule;

                        var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
                        workspace.EnsureTargetExists(targetModule);

                        arePostBuildCommands = false;
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            $"No anchor set on '{module.GetType().ToString()}' with source path '{module.SourcePath}'"
                        );
                    }
                }
            }

            var target = targetModule.MetaData as XcodeBuilder.Target;

            System.Diagnostics.Debug.Assert(null != collatedInterface.SourceModule);
            if (module.IsAnchor && !(collatedInterface.SourceModule is PreExistingObject))
            {
                if (module.IsAnchorAnApplicationBundle)
                {
                    // application bundles are a different output type in Xcode
                    target.MakeApplicationBundle();
                }

                // since all dependents are copied _beside_ their anchor, the anchor copy is a no-op
                return;
            }

            if (module.IsInAnchorPackage)
            {
                // additionally, any built module-based dependents in the same package as the anchor do not need copying as they
                // are built into the right directory (since Xcode module build dirs do not include the module name)
                System.Diagnostics.Debug.Assert(1 == module.OutputDirectories.Count());
                var output_dir = module.OutputDirectories.First().ToString();
                var input_dir = System.IO.Path.GetDirectoryName(module.SourcePath.ToString());
                if (output_dir.Equals(input_dir, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            var copyFileTool = module.Tool as CopyFileTool;

            var commands = new Bam.Core.StringArray();
            foreach (var dir in module.OutputDirectories)
            {
                commands.Add(
                    System.String.Format(
                        "[[ ! -d {0} ]] && mkdir -p {0}",
                        copyFileTool.EscapePath(dir.ToString())
                    )
                );
            }
            commands.Add(
                System.String.Format("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(copyFileTool as Bam.Core.ICommandLineTool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(copyFileTool as Bam.Core.ICommandLineTool))
            );

            var configuration = target.GetConfiguration(targetModule);
            if (!target.IsUtilityType && arePostBuildCommands)
            {
                target.AddPostBuildCommands(commands, configuration);
            }
            else
            {
                target.AddPreBuildCommands(commands, configuration, null);
            }
        }
    }
}
