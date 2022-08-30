using System.Collections.Generic;
using System.IO;
using SharpFileSystem.FileSystems;

namespace SharpFileSystem.SharpCompress
{
    public class SeamlessSharpCompressFileSystem : SeamlessArchiveFileSystem
    {
        public ICollection<string> ArchiveExtensions { get; set; }

        public SeamlessSharpCompressFileSystem(IFileSystem fileSystem)
            : base(fileSystem)
        {
            ArchiveExtensions = new[]
                                    {
                                        ".zip",
                                        ".7z",
                                        ".rar",
                                        ".tar",
                                        ".gz",
                                        ".tar.gz"
                                    };
        }

        protected override bool IsArchiveFile(IFileSystem fileSystem, FileSystemPath path)
        {
            return path.IsFile
                && ArchiveExtensions.Contains(path.GetExtension())
                && !HasArchive(path)// HACK: Disable ability to open archives inside archives (SharpCompress's stream does not have the ability to trace at the moment).
                ;
        }

        protected override IFileSystem CreateArchiveFileSystem(File archiveFile)
        {
            SharpCompressFileSystem archiveFs;
            if (archiveFile.FileSystem is PhysicalFileSystem)
                archiveFs = new SharpCompressFileSystem(((PhysicalFileSystem)archiveFile.FileSystem).GetPhysicalPath(archiveFile.Path));
            else
            {
                Stream archiveStream = archiveFile.FileSystem.OpenFile(archiveFile.Path, FileAccess.Read);
                archiveFs = new SharpCompressFileSystem(archiveStream);
            }
            return archiveFs;
        }
    }
}


