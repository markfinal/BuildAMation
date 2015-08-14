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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var includeSourceInProject = true;
            if (moduleToBuild is Bam.Core.IIsGeneratedSource)
            {
                var generatedSource = moduleToBuild as Bam.Core.IIsGeneratedSource;
                includeSourceInProject = !generatedSource.AutomaticallyHandledByBuilder(node.Target);
            }

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            if (includeSourceInProject)
            {
                var sourceLoc = moduleToBuild.SourceFileLocation;
                var sourceFilePath = sourceLoc.GetSinglePath();
                if ((moduleToBuild is C.ObjC.ObjectFile) || (moduleToBuild is C.ObjCxx.ObjectFile))
                {
                    data.ObjectiveSources.Add(sourceFilePath);
                }
                else
                {
                    data.Sources.Add(sourceFilePath);
                }
                data.Output = QMakeData.OutputType.ObjectFile;
            }

            var optionInterface = moduleToBuild.Options as C.ICCompilerOptions;

            data.ObjectsDir = moduleToBuild.Locations[C.ObjectFile.OutputDir];
            data.IncludePaths.AddRangeUnique(optionInterface.IncludePaths.ToStringArray());
            if (optionInterface.IgnoreStandardIncludePaths)
            {
                data.IncludePaths.AddRangeUnique(optionInterface.SystemIncludePaths.ToStringArray());
            }
            data.Defines.AddRangeUnique(optionInterface.Defines.ToStringArray());

            if (optionInterface is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineBuilder = new Bam.Core.StringArray();
                var target = node.Target;
                var commandLineOption = optionInterface as CommandLineProcessor.ICommandLineSupport;
                var excludedOptionNames = new Bam.Core.StringArray();
                excludedOptionNames.Add("OutputType");
                excludedOptionNames.Add("Defines");
                excludedOptionNames.Add("IncludePaths");
                if (optionInterface.IgnoreStandardIncludePaths)
                {
                    excludedOptionNames.Add("SystemIncludePaths");
                }
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptionNames);
                if (optionInterface is C.ICxxCompilerOptions)
                {
                    data.CXXFlags.AddRangeUnique(commandLineBuilder);
                }
                else
                {
                    data.CCFlags.AddRangeUnique(commandLineBuilder);
                }
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            success = true;
            return data;
        }
    }
}
