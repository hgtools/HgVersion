using System;
using System.Collections.Generic;
using System.Linq;
using HgVersion.VCS;
using Mercurial;
using VCSVersion;
using VCSVersion.VCS;

namespace HgVersionTests.VCS
{
    public sealed class RepositoryLogger : IHgRepository
    {
        private IHgRepository _repository;

        public RepositoryLogger(IHgRepository repository)
        {
            _repository = repository;
        }

        public string Path
        {
            get
            {
                using (Logger.IndentLog($"Get repository path"))
                {
                    return _repository.Path;
                }
            }
        }

        public IEnumerable<ICommit> Log()
        {
            using (Logger.IndentLog($"Get log"))
            {
                return _repository.Log()
                    .ToList();
            }
        }

        public IEnumerable<ICommit> Log(ILogQuery query)
        {
            using (Logger.IndentLog($"Get log by query: {query}"))
            {
                return _repository.Log(query)
                    .ToList();
            }
        }

        public IEnumerable<ICommit> Log(Func<ILogQueryBuilder, ILogQuery> config)
        {
            return _repository.Log(config);
        }

        public IEnumerable<ICommit> Heads()
        {
            using (Logger.IndentLog("Get repository heads"))
            {
                return _repository.Heads()
                    .ToList();
            }
        }

        public IBranchHead CurrentBranch()
        {
            using (Logger.IndentLog("Get current branch"))
            {
                return _repository.CurrentBranch();
            }
        }

        public ICommit CurrentCommit()
        {
            using (Logger.IndentLog("Get current commit"))
            {
                return _repository.CurrentCommit();
            }
        }

        public IEnumerable<IBranchHead> Branches()
        {
            using (Logger.IndentLog("Get repository branches"))
            {
                return _repository.Branches()
                    .ToList();
            }
        }

        public ICommit Tip()
        {
            using (Logger.IndentLog("Get tip commit"))
            {
                return _repository.Tip();
            }
        }

        public void Tag(string name)
        {
            using (Logger.IndentLog($"Add tag: {name} to current commit"))
            {
                _repository.Tag(name);
            }
        }

        public IEnumerable<ITag> Tags()
        {
            using (Logger.IndentLog("Get all repository tags"))
            {
                return _repository.Tags()
                    .ToList();
            }
        }

        public void AddRemove()
        {
            using (Logger.IndentLog("Add all new files, delete all missing files"))
            {
                _repository.AddRemove();
            }
        }

        public string Commit(string message)
        {
            using (Logger.IndentLog($"Commits all outstanding changes to the repository with message: {message}"))
            {
                return _repository.Commit(message);
            }
        }

        public IEnumerable<ICommit> Parents(ICommit commit)
        {
            using (Logger.IndentLog($"Get parents of commit: {commit.Hash}"))
            {
                return _repository.Parents(commit)
                    .ToList();
            }
        }

        public ICommit GetCommit(int revisionNumber)
        {
            using (Logger.IndentLog($"Get commit by rev: {revisionNumber}"))
            {
                return _repository.GetCommit(revisionNumber);
            }
        }

        public ICommit GetCommit(RevSpec revision)
        {
            using (Logger.IndentLog($"Get commit by rev: {revision}"))
            {
                return _repository.GetCommit(revision);
            }
        }

        public ICommit GetBranchHead(string branchName)
        {
            using (Logger.IndentLog($"Get head of the branch: {branchName}"))
            {
                return _repository.GetBranchHead(branchName);
            }
        }

        public void Branch(string branch)
        {
            using (Logger.IndentLog($"Create a branch with name: {branch}"))
            {
                _repository.Branch(branch);
            }
        }

        public void Update(RevSpec rev)
        {
            using (Logger.IndentLog($"Update working directory to a revision: {rev}"))
            {
                _repository.Update(rev);
            }
        }
    }
}