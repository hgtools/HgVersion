using HgVersion.Helpers;
using HgVersion.VCS;

namespace HgVersion
{
    public interface IVersionContext
    {
        IRepository Repository { get; }
        IFileSystem FileSystem { get; }
    }
}