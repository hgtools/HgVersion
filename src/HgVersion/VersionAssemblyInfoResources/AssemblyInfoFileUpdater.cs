using HgVersion.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VCSVersion;
using VCSVersion.Helpers;
using VCSVersion.Output;

namespace HgVersion.VersionAssemblyInfoResources
{
    public class AssemblyInfoFileUpdater : IDisposable
    {
        private readonly List<Action> _restoreBackupTasks = new List<Action>();
        private readonly List<Action> _cleanupBackupTasks = new List<Action>();

        private readonly ISet<string> _assemblyInfoFileNames;
        private readonly string _workingDirectory;
        private readonly VersionVariables _variables;
        private readonly IFileSystem _fileSystem;
        private readonly bool _ensureAssemblyInfo;
        private readonly TemplateManager _templateManager;

        public AssemblyInfoFileUpdater(string assemblyInfoFileName, string workingDirectory, VersionVariables variables, IFileSystem fileSystem, bool ensureAssemblyInfo) :
            this(new HashSet<string> { assemblyInfoFileName }, workingDirectory, variables, fileSystem, ensureAssemblyInfo)
        { }

        public AssemblyInfoFileUpdater(ISet<string> assemblyInfoFileNames, string workingDirectory, VersionVariables variables, IFileSystem fileSystem, bool ensureAssemblyInfo)
        {
            _assemblyInfoFileNames = assemblyInfoFileNames;
            _workingDirectory = workingDirectory;
            _variables = variables;
            _fileSystem = fileSystem;
            _ensureAssemblyInfo = ensureAssemblyInfo;
            _templateManager = new TemplateManager(TemplateType.VersionAssemblyInfoResources);
        }

        public void Update()
        {
            Logger.WriteInfo("Updating assembly info files");

            var assemblyInfoFiles = GetAssemblyInfoFiles(_workingDirectory, _assemblyInfoFileNames, _fileSystem, _ensureAssemblyInfo).ToList();
            Logger.WriteInfo($"Found {assemblyInfoFiles.Count} files");

            var assemblyVersion = _variables.AssemblySemVer;
            var assemblyVersionRegex = new Regex(@"AssemblyVersion(Attribute)?\s*\(.*\)\s*");
            var assemblyVersionString = !string.IsNullOrWhiteSpace(assemblyVersion) ? $"AssemblyVersion(\"{assemblyVersion}\")" : null;

            var assemblyInfoVersion = _variables.InformationalVersion;
            var assemblyInfoVersionRegex = new Regex(@"AssemblyInformationalVersion(Attribute)?\s*\(.*\)\s*");
            var assemblyInfoVersionString = $"AssemblyInformationalVersion(\"{assemblyInfoVersion}\")";

            var assemblyFileVersion = _variables.AssemblyFileSemVer;
            var assemblyFileVersionRegex = new Regex(@"AssemblyFileVersion(Attribute)?\s*\(.*\)\s*");
            var assemblyFileVersionString = !string.IsNullOrWhiteSpace(assemblyFileVersion) ? $"AssemblyFileVersion(\"{assemblyFileVersion}\")" : null;

            foreach (var assemblyInfoFile in assemblyInfoFiles)
            {
                var backupAssemblyInfo = assemblyInfoFile.FullName + ".bak";
                var localAssemblyInfo = assemblyInfoFile.FullName;
                _fileSystem.Copy(assemblyInfoFile.FullName, backupAssemblyInfo, true);

                _restoreBackupTasks.Add(() =>
                {
                    if (_fileSystem.Exists(localAssemblyInfo))
                    {
                        _fileSystem.Delete(localAssemblyInfo);
                    }

                    _fileSystem.Move(backupAssemblyInfo, localAssemblyInfo);
                });

                _cleanupBackupTasks.Add(() => _fileSystem.Delete(backupAssemblyInfo));

                var originalFileContents = _fileSystem.ReadAllText(assemblyInfoFile.FullName);
                var fileContents = originalFileContents;
                var appendedAttributes = false;

                if (!string.IsNullOrWhiteSpace(assemblyVersion))
                {
                    fileContents = ReplaceOrAppend(assemblyVersionRegex, fileContents, assemblyVersionString, assemblyInfoFile.Extension, ref appendedAttributes);
                }

                if (!string.IsNullOrWhiteSpace(assemblyFileVersion))
                {
                    fileContents = ReplaceOrAppend(assemblyFileVersionRegex, fileContents, assemblyFileVersionString, assemblyInfoFile.Extension, ref appendedAttributes);
                }

                fileContents = ReplaceOrAppend(assemblyInfoVersionRegex, fileContents, assemblyInfoVersionString, assemblyInfoFile.Extension, ref appendedAttributes);

                if (appendedAttributes)
                {
                    // If we appended any attributes, put a new line after them
                    fileContents += Environment.NewLine;
                }

                if (originalFileContents != fileContents)
                {
                    _fileSystem.WriteAllText(assemblyInfoFile.FullName, fileContents);
                }
            }
        }

        private string ReplaceOrAppend(Regex replaceRegex, string inputString, string replaceString, string fileExtension, ref bool appendedAttributes)
        {
            var assemblyAddFormat = _templateManager.GetAddFormatFor(fileExtension);

            if (replaceRegex.IsMatch(inputString))
            {
                inputString = replaceRegex.Replace(inputString, replaceString);
            }
            else
            {
                inputString += Environment.NewLine + string.Format(assemblyAddFormat, replaceString);
                appendedAttributes = true;
            }

            return inputString;
        }

        private IEnumerable<FileInfo> GetAssemblyInfoFiles(string workingDirectory, ISet<string> assemblyInfoFileNames, IFileSystem fileSystem, bool ensureAssemblyInfo)
        {
            if (assemblyInfoFileNames != null && assemblyInfoFileNames.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                foreach (var item in assemblyInfoFileNames)
                {
                    var fullPath = Path.Combine(workingDirectory, item);

                    if (EnsureVersionAssemblyInfoFile(ensureAssemblyInfo, fileSystem, fullPath))
                    {
                        yield return new FileInfo(fullPath);
                    }
                }
            }
            else
            {
                foreach (var item in fileSystem.DirectoryGetFiles(workingDirectory, "AssemblyInfo.*", SearchOption.AllDirectories))
                {
                    var assemblyInfoFile = new FileInfo(item);

                    if (_templateManager.IsSupported(assemblyInfoFile.Extension))
                    {
                        yield return assemblyInfoFile;
                    }
                }
            }
        }

        private bool EnsureVersionAssemblyInfoFile(bool ensureAssemblyInfo, IFileSystem fileSystem, string fullPath)
        {
            if (fileSystem.Exists(fullPath))
            {
                return true;
            }

            if (!ensureAssemblyInfo)
            {
                return false;
            }

            var assemblyInfoSource = _templateManager.GetTemplateFor(Path.GetExtension(fullPath));

            if (!string.IsNullOrWhiteSpace(assemblyInfoSource))
            {
                var fileInfo = new FileInfo(fullPath);

                if (!fileSystem.DirectoryExists(fileInfo.Directory.FullName))
                {
                    fileSystem.CreateDirectory(fileInfo.Directory.FullName);
                }

                fileSystem.WriteAllText(fullPath, assemblyInfoSource);
                return true;
            }

            Logger.WriteWarning($"No version assembly info template available to create source file '{fullPath}'");
            return false;
        }

        public void Dispose()
        {
            foreach (var restoreBackup in _restoreBackupTasks)
            {
                restoreBackup();
            }

            _cleanupBackupTasks.Clear();
            _restoreBackupTasks.Clear();
        }

        public void CommitChanges()
        {
            foreach (var cleanupBackupTask in _cleanupBackupTasks)
            {
                cleanupBackupTask();
            }

            _cleanupBackupTasks.Clear();
            _restoreBackupTasks.Clear();
        }
    }
}
