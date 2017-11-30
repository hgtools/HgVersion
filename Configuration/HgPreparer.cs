using System;
using System.Collections.Generic;
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
        }
    }
}
