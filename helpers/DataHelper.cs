using System.IO;
using System.Reflection;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class DataHelper
    {
        public static string FindResourceName(string key, string type, Assembly target)
        {
            string lowerKey = $".{key.ToLowerInvariant()}.{type.ToLowerInvariant()}";
            foreach (string resourceName in target.GetManifestResourceNames())
                if (resourceName.ToLowerInvariant().EndsWith(lowerKey))
                    return resourceName;

            return default(string);
        }

        public static string GetResourceString(string key, string type)
        {
            Assembly target = Assembly.GetExecutingAssembly();
            string resourceName = FindResourceName(key, type, target);

            if (string.IsNullOrEmpty(resourceName))
            {
                string errorHelp = "";
                foreach (string testName in target.GetManifestResourceNames())
                    errorHelp += "," + testName;
                throw new InvalidDataException($"Could not find resource matching {key}. This is what I have: {errorHelp}");
            }

            using (Stream resourceStream = target.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}