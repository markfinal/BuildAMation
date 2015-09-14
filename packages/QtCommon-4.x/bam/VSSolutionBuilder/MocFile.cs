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
namespace QtCommon
{
namespace V2
{
    public sealed class VSSolutionMocGeneration :
        IMocGenerationPolicy
    {
        void
        IMocGenerationPolicy.Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source)
        {
#if true
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.V2.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var output = generatedMocSource.Parse();

            var args = new Bam.Core.StringArray();
            args.Add(mocCompiler.Executable.Parse());
            args.Add(System.String.Format("-o{0}", output));
            args.Add("%(FullPath)");

            var customBuild = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.CustomBuild, include: source.InputPath, uniqueToProject: true);
            customBuild.AddSetting("Command", args.ToString(' '), condition: config.ConditionText);
            customBuild.AddSetting("Message", System.String.Format("Moccing {0}", System.IO.Path.GetFileName(source.InputPath.Parse())), condition: config.ConditionText);
            customBuild.AddSetting("Outputs", output, condition: config.ConditionText);
#else
            if (null != source.MetaData)
            {
                throw new Bam.Core.Exception("Header file {0} already has VSSolution metadata", source.InputPath.Parse());
            }

            // TODO: this is hardcoded - needed a better way to figure this out
            var platform = VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour;

            var output = generatedMocSource.Parse();

            var args = new Bam.Core.StringArray();
            args.Add(mocCompiler.Executable.Parse());
            args.Add(System.String.Format("-o{0}", output));
            args.Add("%(FullPath)");

            var customBuild = new VSSolutionBuilder.V2.VSProjectCustomBuild(source, platform);
            customBuild.Message = "Moccing";
            customBuild.Command = args.ToString(' ');
            customBuild.Outputs.AddUnique(generatedMocSource.Parse());

            var headerFile = new VSSolutionBuilder.V2.VSProjectHeaderFile(source, platform);
            headerFile.Source = source.GeneratedPaths[C.V2.HeaderFile.Key];
            headerFile.CustomBuild = customBuild;
#endif
        }
    }
}
}
