using System.IO;
using System.Reflection;

namespace ESOAPIExplorer.Models
{
    public static class EmbeddedResourceReader
    {
        public static string ReadEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.Load("ESOAPIExplorer");

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
