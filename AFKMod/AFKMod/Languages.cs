using System.IO;
using R2API;

namespace AFKMod
{
    public static class Languages
    {
        public static void Init()
        {
            string languageFilePath = Path.Combine(Path.GetDirectoryName(typeof(Languages).Assembly.Location), "Languages", "lang.language");

            if (File.Exists(languageFilePath))
            {
                LanguageAPI.AddPath(languageFilePath);
                UnityEngine.Debug.Log($"Nested language file loaded: {languageFilePath}");
            }
            else
            {
                UnityEngine.Debug.LogError($"Language file not found at: {languageFilePath}");
            }
        }
    }
}
