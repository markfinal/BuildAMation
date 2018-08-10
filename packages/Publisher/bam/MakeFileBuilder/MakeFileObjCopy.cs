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
namespace Publisher
{
#if BAM_V2
#if false
    public static partial class MakeFileSupport
    {
        public static void
        ObjCopy(
            ObjCopyModule module)
        {
            MakeFileBuilder.MakeFileMeta meta;
            MakeFileBuilder.Rule rule;

            var variableName = new System.Text.StringBuilder();
            variableName.Append((module as ICollatedObject).SourceModule.GetType().Name);

            if (module is MakeDebugSymbolFile)
            {
                meta = new MakeFileBuilder.MakeFileMeta(module);
                rule = meta.AddRule();

                rule.AddTarget(
                    module.GeneratedPaths[MakeDebugSymbolFile.DebugSymbolFileKey],
                    variableName: "DebugSymbols" + variableName
                );
            }
            else if (module is LinkBackDebugSymbolFile)
            {
                // append to the strip rule
                System.Diagnostics.Debug.Assert((module as ICollatedObject).SourceModule is StripModule);
                meta = (module as ICollatedObject).SourceModule.MetaData as MakeFileBuilder.MakeFileMeta;
                rule = meta.Rules[0];

                rule.AddTarget(
                    module.GeneratedPaths[LinkBackDebugSymbolFile.UpdateOriginalExecutable],
                    variableName: "LinkDebugSymbols" + variableName
                );
            }
            else
            {
                throw new Bam.Core.Exception(
                    "Unsupported type of ObjCopy module: {0}",
                    module.GetType().ToString()
                );
            }

            foreach (var input in module.InputModules)
            {
                rule.AddPrerequisite(input.Value, input.Key);
            }

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            rule.AddShellCommand(System.String.Format("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(module.Tool as Bam.Core.ICommandLineTool))
            );
        }
    }
#endif
#else
    public sealed class MakeFileObjCopy :
        IObjCopyToolPolicy
    {
        void
        IObjCopyToolPolicy.ObjCopy(
            ObjCopyModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString originalPath,
            Bam.Core.TokenizedString copiedPath)
        {
            var mode = (sender.Settings as IObjCopyToolSettings).Mode;

            // if linking debug data, add to the strip
            var collatedObjectInterface = sender as ICollatedObject;
            var meta = (EObjCopyToolMode.AddGNUDebugLink == mode) ? collatedObjectInterface.SourceModule.MetaData as MakeFileBuilder.MakeFileMeta : new MakeFileBuilder.MakeFileMeta(sender);
            var rule = (EObjCopyToolMode.AddGNUDebugLink == mode) ? meta.Rules[0] :meta.AddRule();

            if (EObjCopyToolMode.AddGNUDebugLink == mode)
            {
                rule.AddOrderOnlyDependency(copiedPath.ToString());
            }
            else
            {
                var dir = sender.CreateTokenizedString("@dir($(0))", copiedPath);
                dir.Parse();
                meta.CommonMetaData.AddDirectory(dir.ToString());

                var sourceFilename = System.IO.Path.GetFileName(originalPath.ToString());
                rule.AddTarget(copiedPath, variableName: "objcopy_" + sourceFilename);
                rule.AddPrerequisite(originalPath);
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            rule.AddShellCommand(System.String.Format("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                commandLine.ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
        }
    }
#endif
}
