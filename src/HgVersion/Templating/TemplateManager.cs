using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HgVersion.Templating
{
    internal class TemplateManager
    {
        private readonly Dictionary<string, string> _templates;
        private readonly Dictionary<string, string> _addFormats;

        public TemplateManager(TemplateType templateType)
        {
            _templates = GetEmbeddedTemplates(templateType, "Templates")
                .ToDictionary(k => Path.GetExtension(k), v => v, StringComparer.OrdinalIgnoreCase);

            _addFormats = GetEmbeddedTemplates(templateType, "AddFormats")
                .ToDictionary(k => Path.GetExtension(k), v => v, StringComparer.OrdinalIgnoreCase);
        }

        public string GetTemplateFor(string fileExtension)
        {
            if (fileExtension == null)
                throw new ArgumentNullException(nameof(fileExtension));

            if (_templates.TryGetValue(fileExtension, out var template) && template != null)
                return ReadAsStringFromEmbeddedResource<TemplateManager>(template);

            return null;
        }

        public string GetAddFormatFor(string fileExtension)
        {
            if (fileExtension == null)
                throw new ArgumentNullException(nameof(fileExtension));

            if (_addFormats.TryGetValue(fileExtension, out var addFormat) && addFormat != null)
                return ReadAsStringFromEmbeddedResource<TemplateManager>(addFormat);

            return null;
        }

        public bool IsSupported(string fileExtension)
        {
            if (fileExtension == null)
                throw new ArgumentNullException(nameof(fileExtension));

            return _templates.ContainsKey(fileExtension);
        }

        private static IEnumerable<string> GetEmbeddedTemplates(TemplateType templateType, string templateCategory)
        {
            foreach (var name in typeof(TemplateManager).Assembly.GetManifestResourceNames())
            {
                if (name.Contains(templateType.ToString()) && name.Contains(templateCategory))
                {
                    yield return name;
                }
            }
        }

        private static string ReadAsStringFromEmbeddedResource<T>(string resourceName)
        {
            using (var stream = ReadFromEmbeddedResource<T>(resourceName))
            using (var rdr = new StreamReader(stream))
            {
                return rdr.ReadToEnd();
            }
        }

        private static Stream ReadFromEmbeddedResource<T>(string resourceName)
        {
            var assembly = typeof(T).Assembly;
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
