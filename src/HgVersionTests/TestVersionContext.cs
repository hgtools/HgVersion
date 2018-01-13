using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HgVersion;
using HgVersion.VCS;
using Mercurial;
using VCSVersion;
using VCSVersion.Configuration;
using VCSVersion.Helpers;
using VCSVersion.SemanticVersions;
using VCSVersion.VCS;

namespace HgVersionTests
{
    public sealed class TestVesionContext : IVersionContext, IDisposable
    {
        private HgVersionContext _context;
        private IHgRepository _repository;
        
        public TestVesionContext(bool inited = true)
        {
            _repository = CreateTempRepository(inited)
                .WithLogger();
        }
        
        public void WriteTextAndCommit(string fileName, string content, string commitMessage = null)
        {
            var path = Path.Combine(_repository.Path, fileName);
            var fileInfo = new FileInfo(path);
            var message = GetCommitMessage(fileInfo, commitMessage);

            Directory.CreateDirectory(fileInfo.Directory.FullName);
            File.WriteAllText(fileInfo.FullName, content);

            _repository.AddRemove();
            _repository.Commit(message);
        }
        
        public void MakeTaggedCommit(string tag)
        {
            _repository.Tag(tag);
        }
        
        public void MakeCommit(string message = null)
        {
            WriteTextAndCommit("dummy.txt", Guid.NewGuid().ToString(), message ?? "Empty commit");
        }
        
        public void CreateBranch(string branch)
        {
            _repository.Branch(branch);
        }

        public ICommit Tip()
        {
            return _repository.Tip();
        }

        public void Update(RevSpec rev)
        {
            _repository.Update(rev);
        }
        
        private HgVersionContext GetContext()
        {
            return LazyInitializer.EnsureInitialized(ref _context, () => new HgVersionContext(_repository));
        }
        
        private static string GetCommitMessage(FileInfo fileInfo, string commitMessage)
        {
            if (!string.IsNullOrEmpty(commitMessage))
                return commitMessage;

            return fileInfo.Exists ? $"change {fileInfo.Name}" : $"create {fileInfo.Name}";
        }

        private static IHgRepository CreateTempRepository(bool inited)
        {
            var repoPath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid()
                    .ToString()
                    .Replace("-", string.Empty)
                    .ToLowerInvariant());

            Directory.CreateDirectory(repoPath);
            
            var repository = new Repository(repoPath);

            if (inited)
            {
                repository.Init();
            }

            return (HgRepository)repository;
        }

        private static void DeleteTempRepository(IRepository repository)
        {
            for (int index = 1; index < 5; index++)
            {
                try
                {
                    if (Directory.Exists(repository.Path))
                        Directory.Delete(repository.Path, true);
                    break;
                }
                catch (DirectoryNotFoundException)
                { }
                catch (Exception ex)
                {
                    Debug.WriteLine("exception while cleaning up repository directory: "
                                    + ex.GetType().Name + ": " +
                                    ex.Message);

                    Thread.Sleep(1000);
                }
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DeleteTempRepository(_repository);
                }
                
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region IVersionContext implementation
        public IRepository Repository => GetContext().Repository;
        public IFileSystem FileSystem => GetContext().FileSystem;
        public Config FullConfiguration => GetContext().FullConfiguration;
        public EffectiveConfiguration Configuration => GetContext().Configuration;
        public IBranchHead CurrentBranch => GetContext().CurrentBranch;
        public ICommit CurrentCommit => GetContext().CurrentCommit;
        public IRepositoryMetadataProvider RepositoryMetadataProvider => GetContext().RepositoryMetadataProvider;
        public bool IsCurrentCommitTagged => GetContext().IsCurrentCommitTagged;
        public SemanticVersion CurrentCommitTaggedVersion => GetContext().CurrentCommitTaggedVersion;
        #endregion
    }
}
