using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawlerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Read URLs from the input file
            string[] rootUrls = await File.ReadAllLinesAsync("urls.txt");

            // Create an instance of WebCrawler
            var crawler = new WebCrawler();

            // Run crawling tasks concurrently
            await crawler.CrawlAndSaveAsync(rootUrls, "output2.txt");
        }
    }
}
