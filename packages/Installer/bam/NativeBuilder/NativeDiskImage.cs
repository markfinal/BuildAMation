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
namespace Installer
{
    public sealed class NativeDMG :
        IDiskImagePolicy
    {
        void
        IDiskImagePolicy.CreateDMG(
            DiskImage sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString sourceFolderPath,
            Bam.Core.TokenizedString outputPath)
        {
            var volumeNameTS = sender.CreateTokenizedString("$(OutputName)");
            lock (volumeNameTS)
            {
                if (!volumeNameTS.IsParsed)
                {
                    volumeNameTS.Parse();
                }
            }
            var volumeName = volumeNameTS.ToString();
            var tempDiskImagePathName = System.IO.Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".dmg"; // must have .dmg extension

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            // create the disk image
            {
                var settings = sender.Settings as IDiskImageSettings;

                var args = new Bam.Core.StringArray();
                args.Add("create");
                args.AddRange(commandLine);
                args.Add("-srcfolder");
                args.Add(System.String.Format("\"{0}\"", sourceFolderPath.ToString()));
                args.Add("-size");
                args.Add(settings.ImageSize);
                args.Add("-fs");
                args.Add("HFS+");
                args.Add("-volname");
                args.Add(System.String.Format("\"{0}\"", volumeName));
                args.Add(tempDiskImagePathName);
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            // mount disk image
            {
                var args = new Bam.Core.StringArray();
                args.Add("attach");
                args.AddRange(commandLine);
                args.Add(tempDiskImagePathName);
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            // TODO
            /// do a copy

            // unmount disk image
            {
                var args = new Bam.Core.StringArray();
                args.Add("detach");
                args.AddRange(commandLine);
                args.Add(System.String.Format("\"/Volumes/{0}\"", volumeName));
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            var diskImagePathName = outputPath.ToString();
            var dmgDir = System.IO.Path.GetDirectoryName(diskImagePathName);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(dmgDir);

            // hdiutil convert myimg.dmg -format UDZO -o myoutputimg.dmg
            // this will fail if the output DMG exists, so always write to a temporary
            // file and then move into place
            var tempDMGPath = System.IO.Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".dmg";
            {
                var args = new Bam.Core.StringArray();
                args.Add("convert");
                args.AddRange(commandLine);
                args.Add(tempDiskImagePathName);
                args.Add("-format");
                args.Add("UDZO");
                args.Add("-o");
                args.Add(tempDMGPath);
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            // move the temporary DMG to the expected location
            {
                var args = new Bam.Core.StringArray();
                args.Add("-f");
                args.Add("-v");
                args.Add(tempDMGPath);
                args.Add(outputPath.ToStringQuoteIfNecessary()); // diskImagePathName
                CommandLineProcessor.Processor.Execute(context, Bam.Core.OSUtilities.GetInstallLocation("mv").First(), args);
            }
        }
    }
}
