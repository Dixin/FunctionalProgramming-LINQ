namespace Tutorial.Introduction
{
    using System;
    using System.IO;

    internal static partial class Functional
    {
        internal static FileInfo Download(Uri sourceUri, DirectoryInfo downloadDirectory)
        {
            throw new NotImplementedException();
        }

        internal static FileInfo Convert(FileInfo sourceFile, FileInfo templateFile)
        {
            throw new NotImplementedException();
        }

        internal static Func<Uri, DirectoryInfo, FileInfo, FileInfo> CreateDocumentBuilder(
            Func<Uri, DirectoryInfo, FileInfo> download, Func<FileInfo, FileInfo, FileInfo> convert)
        {
            return (sourceUri, downloadDirectory, templateFile) =>
            {
                FileInfo sourceFile = download(sourceUri, downloadDirectory);
                return convert(sourceFile, templateFile);
            };
        }
    }

    internal static partial class Functional
    {
        internal static void BuildDocument(Uri sourceUri, DirectoryInfo downloadDirectory, FileInfo templateFile)
        {
            Func<Uri, DirectoryInfo, FileInfo, FileInfo> buildDocument = CreateDocumentBuilder(Download, Convert);
            FileInfo resultFile = buildDocument(sourceUri, downloadDirectory, templateFile);
        }
    }
}