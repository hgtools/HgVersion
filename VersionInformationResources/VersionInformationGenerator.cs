using HgVersion.Templating;
using System.IO;
using VCSVersion.Helpers;
using VCSVersion.Output;

namespace HgVersion.VersionInformationResources
{
    public class VersionInformationGenerator
    {
        private string _fileName;
        private string _directory;
        private VersionVariables _variables;
        private IFileSystem _fileSystem;
        private TemplateManager _templateManager;

        public VersionInformationGenerator(string fileName, string directory, VersionVariables variables, IFileSystem fileSystem)
        {
            _fileName = fileName;
            _directory = directory;
            _variables = variables;
            _fileSystem = fileSystem;
            _templateManager = new TemplateManager(TemplateType.VersionInformationResources);
        }

        public void Generate()
        {
            var filePath = Path.Combine(_directory, _fileName);

            string originalFileContents = null;

            if (File.Exists(filePath))
            {
                originalFileContents = _fileSystem.ReadAllText(filePath);
            }

            var fileExtension = Path.GetExtension(filePath);
            var template = _templateManager.GetTemplateFor(fileExtension);
            var addFormat = _templateManager.GetAddFormatFor(fileExtension);
            var members = _variables.ToString(addFormat);

            var fileContents = string.Format(template, members);

            if (fileContents != originalFileContents)
            {
                _fileSystem.WriteAllText(filePath, fileContents);
            }
        }
    }
}
