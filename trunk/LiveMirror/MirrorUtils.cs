using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveMirror
{
    class MirrorUtils
    {
        string fromDir, toDir;
        public MirrorUtils(string fromDir, string toDir)
        {
            this.fromDir = fromDir;
            this.toDir = toDir;
        }

        public string NewPath(string file)
        {
            string filePath = file;
            filePath = filePath.Remove(0, fromDir.Length);
            filePath = toDir + filePath;
            return filePath;
        }
        public string BasePath(string file)
        {
            if (file.StartsWith(fromDir))
                return file.Remove(0, fromDir.Length).TrimStart('\\');
            else if (file.StartsWith(toDir))
                return file.Remove(0, toDir.Length).TrimStart('\\');
            else return file;
        }
        public void CopyDirectory(string source, string target)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(source);
            DirectoryInfo targetDir;
            if (Directory.Exists(target))
                targetDir = new DirectoryInfo(target);
            else
                targetDir = Directory.CreateDirectory(target);
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                file.CopyTo(Path.Combine(target, file.Name));
            }
            foreach (DirectoryInfo dir in sourceDir.GetDirectories())
            {
                CopyDirectory(dir.FullName, Path.Combine(target, dir.Name));
            }
        }
        public bool FilesContentsEqual(FileInfo fileInfo1, FileInfo fileInfo2)
        {
            try
            {
                using (var file1 = fileInfo1.OpenRead())
                {
                    using (var file2 = fileInfo2.OpenRead())
                    {
                        const int buffersize = 2048;
                        byte[] buffer1 = new byte[buffersize];
                        byte[] buffer2 = new byte[buffersize];
                        while (true)
                        {
                            int count1 = file1.Read(buffer1, 0, buffersize);
                            int count2 = file2.Read(buffer2, 0, buffersize);

                            if (count1 != count2)
                                return false;

                            if (count1 == 0)
                                return true;

                            if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
                                return false;
                        }
                    }
                }
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(50);
                return FilesContentsEqual(fileInfo1, fileInfo2);
            }
        }
    }
}
