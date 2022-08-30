using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SharpCompress.Archives;

namespace SharpFileSystem.SharpCompress
{
    public class SharpCompressFileSystem: IFileSystem
    {
        private IArchive _archive;

        private string _archiveName;

        private ICollection<FileSystemPath> _entities = new List<FileSystemPath>();

        private SharpCompressFileSystem(IArchive archive, string archiveName = "")
        {
            _archive = archive;
            _archiveName = archiveName;
            foreach (var file in _archive.Entries)
                AddEntity(GetVirtualFilePath(file));
        }

        public SharpCompressFileSystem(Stream stream, string archiveName = "")
            : this(ArchiveFactory.Open(stream), archiveName)
        {
        }

        public SharpCompressFileSystem(string physicalPath)
            : this(ArchiveFactory.Open(physicalPath),
                physicalPath.Substring(physicalPath.LastIndexOf(FileSystemPath.DirectorySeparator) + 1))
        {
        }

        public void AddEntity(FileSystemPath path)
        {
            if (!_entities.Contains(path))
                _entities.Add(path);
            if (!path.IsRoot)
                AddEntity(path.ParentPath);
        }

        public string GetArchivePath(FileSystemPath path)
        {
            return path.ToString().Remove(0, 1);
        }

        private IArchiveEntry GetEntryFromPath(FileSystemPath path)
        {
            var fileName = path.ToString().Remove(0, 1);
            foreach (var entry in _archive.Entries)
            {
                if (entry.Key == fileName)
                {
                    return entry;
                }
            }

            return null;
        }

        public FileSystemPath GetVirtualFilePath(IArchiveEntry archiveFile)
        {
            string path = FileSystemPath.DirectorySeparator + archiveFile.Key.Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparator);
            if (archiveFile.IsDirectory && path[path.Length - 1] != FileSystemPath.DirectorySeparator)
                path += FileSystemPath.DirectorySeparator;
            return FileSystemPath.Parse(path, _archiveName);
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            if (!path.IsDirectory)
                throw new ArgumentException("The specified path is not a directory.", "path");
            return _entities.Where(p => !p.IsRoot && p.ParentPath.Equals(path)).ToArray();
        }

        public bool Exists(FileSystemPath path)
        {
            return _entities.Contains(path);
        }

        public Stream CreateFile(FileSystemPath path)
        {
            throw new NotSupportedException();
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            if (access == FileAccess.Write)
                throw new NotSupportedException();

            var entry = GetEntryFromPath(path);

            Stream s = entry.OpenEntryStream();
            return s;
        }

        public void CreateDirectory(FileSystemPath path)
        {
            throw new NotSupportedException();
        }

        public void Delete(FileSystemPath path)
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly => true;

        public void Dispose()
        {
            _archive.Dispose();
        }
    }
}


