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
namespace Publisher
{
    public sealed class NativePackager :
        IPackagePolicy
    {
        static private void
        CopyFile(
            Package sender,
            Bam.Core.ExecutionContext context,
            string sourcePath,
            string destinationDir)
        {
            // TODO: convert this to a command line tool as well
            // but it would require a module to have more than one tool
            if (!System.IO.Directory.Exists(destinationDir))
            {
                System.IO.Directory.CreateDirectory(destinationDir);
            }
            var destinationPath = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(sourcePath));

            var commandLine = new Bam.Core.StringArray();
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, commandLine as object });
            }

            commandLine.Add(sourcePath);
            commandLine.Add(destinationPath);
            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }

        void
        IPackagePolicy.Package(
            Package sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>> packageObjects)
        {
            var root = packageRoot.Parse();
            foreach (var module in packageObjects)
            {
                foreach (var path in module.Value)
                {
                    var sourcePath = path.Key.ToString();
                    if (path.Value.IsMarker)
                    {
                        var destinationDir = root;
                        if (null != path.Value.SubDirectory)
                        {
                            destinationDir = System.IO.Path.Combine(destinationDir, path.Value.SubDirectory);
                        }
                        CopyFile(sender, context, sourcePath, destinationDir);
                        path.Value.DestinationDir = destinationDir;
                    }
                    else
                    {
                        var subdir = path.Value.SubDirectory;
                        foreach (var reference in path.Value.References)
                        {
                            var destinationDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(reference.DestinationDir, path.Value.SubDirectory));
                            CopyFile(sender, context, sourcePath, destinationDir);
                            path.Value.DestinationDir = destinationDir;
                        }
                    }
                }
            }
        }
    }

    public sealed class NativeInnoSetup :
        IInnoSetupPolicy
    {
        void
        IInnoSetupPolicy.CreateInstaller(
            InnoSetupInstaller sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString scriptPath)
        {
            var args = new Bam.Core.StringArray();
            args.Add(scriptPath.Parse());
            CommandLineProcessor.Processor.Execute(context, compiler, args);
        }
    }

    public sealed class NativeNSIS :
        INSISPolicy
    {
        void
        INSISPolicy.CreateInstaller(
            NSISInstaller sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString scriptPath)
        {
            var args = new Bam.Core.StringArray();
            args.Add(scriptPath.Parse());
            CommandLineProcessor.Processor.Execute(context, compiler, args);
        }
    }

    public sealed class NativeTarBall :
        ITarPolicy
    {
        void
        ITarPolicy.CreateTarBall(
            TarBall sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool compiler,
            Bam.Core.TokenizedString scriptPath,
            Bam.Core.TokenizedString outputPath)
        {
            var args = new Bam.Core.StringArray();
            args.Add("-c");
            args.Add("-v");
            args.Add("-T");
            args.Add(scriptPath.Parse());
            args.Add("-f");
            args.Add(outputPath.ToString());
            CommandLineProcessor.Processor.Execute(context, compiler, args);
        }
    }

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
            var volumeName = "My Volume";
            var tempDiskImagePathName = System.IO.Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".dmg"; // must have .dmg extension
            var diskImagePathName = outputPath.ToString();

            // create the disk image
            {
                var args = new Bam.Core.StringArray();
                args.Add("create");
                args.Add("-quiet");
                args.Add("-srcfolder");
                args.Add(System.String.Format("\"{0}\"", sourceFolderPath.ToString()));
                args.Add("-size");
                args.Add("32m");
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
                args.Add("-quiet");
                args.Add(tempDiskImagePathName);
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            // TODO
            /// do a copy

            // unmount disk image
            {
                var args = new Bam.Core.StringArray();
                args.Add("detach");
                args.Add("-quiet");
                args.Add(System.String.Format("\"/Volumes/{0}\"", volumeName));
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }

            // hdiutil convert myimg.dmg -format UDZO -o myoutputimg.dmg
            {
                var args = new Bam.Core.StringArray();
                args.Add("convert");
                args.Add("-quiet");
                args.Add(tempDiskImagePathName);
                args.Add("-format");
                args.Add("UDZO");
                args.Add("-o");
                args.Add(diskImagePathName);
                CommandLineProcessor.Processor.Execute(context, compiler, args);
            }
        }
    }
}
