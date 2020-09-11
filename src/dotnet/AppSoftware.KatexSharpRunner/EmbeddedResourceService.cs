using System.IO;
using System.Linq;
using System.Reflection;

namespace AppSoftware.KatexSharpRunner
{
    public static class EmbeddedResourceService
    {
        public static string GetResourceContent(string resourceFileName, Assembly assembly)
        {
            string resourceContent;

            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceFileName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    resourceContent = reader.ReadToEnd();
                }
            }

            return resourceContent;
        }
    }
}