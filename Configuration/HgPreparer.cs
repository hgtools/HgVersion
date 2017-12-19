using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HgVersion.Configuration
{
    public sealed class HgPreparer
    {
        public string WorkingDirectory { get; }
        public string ProjectRootDirectory { get; }

        public HgPreparer(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            ProjectRootDirectory = GetProjectRootDirectory(workingDirectory);
        }

        private string GetProjectRootDirectory(string workingDirectory)
        {
            var dotHgDirectory = GetDotHgDirectory(workingDirectory);
            var directoryInfo = Directory.GetParent(dotHgDirectory);
            
            return directoryInfo.FullName;
        }

        private static string GetDotHgDirectory(string workingDirectory)
        {
            var directories = Directory.GetDirectories(workingDirectory, ".hg");

            if (directories.Length == 0)
                throw new DirectoryNotFoundException("Can not find the .hg directory in " + workingDirectory);

            if (directories.Length > 1)
            {
                return directories
                    .OrderBy(dir => dir.Length)
                    .First();
            }

            return directories.First();
        }
    }
}
