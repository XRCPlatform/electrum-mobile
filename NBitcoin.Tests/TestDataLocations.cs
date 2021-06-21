using System.IO;

#if NOWEBCLIENT
//using nBRhodium.Tests;
#endif
namespace NBitcoin.Tests
{
    public class TestDataLocations
    {
        private static void EnsureDownloaded(string file, string url)
        {
            if (File.Exists(file))
                return;

            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(Path.GetDirectoryName(file));

            WebClient client = new WebClient();
            client.DownloadFile(url, file);
        }

        public static string AssemblyDirectory
        {
            get
            {
#if NETCORE
                return System.AppContext.BaseDirectory;
#else
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);

#endif
            }
        }
    }
}
