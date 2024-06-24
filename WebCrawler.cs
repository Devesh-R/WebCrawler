// using System.Collections.Concurrent;
// using HtmlAgilityPack;

// namespace WebCrawlerApp
// {
//     public class WebCrawler
//     {
//         private readonly ConcurrentBag<(string RootUrl, string FetchedUrl)> _results;

//         public WebCrawler()
//         {
//             _results = new ConcurrentBag<(string RootUrl, string FetchedUrl)>();
//         }

//         public async Task CrawlAndSaveAsync(string[] rootUrls, string outputFilePath)
//         {
//             // Validate root URLs
//             List<string> validRootUrls = rootUrls.Where(url => IsValidUrl(url)).ToList();

//             // Run crawling tasks concurrently
//             Task[] tasks = validRootUrls.Select(url => Task.Run(async () =>
//             {
//                 IEnumerable<string> fetchedUrls = await CrawlAsync(url);
//                 foreach (string fetchedUrl in fetchedUrls)
//                 {
//                     _results.Add((url, fetchedUrl));
//                 }
//             })).ToArray();

//             // Wait for all tasks to complete
//             await Task.WhenAll(tasks);

//             // Save results to output file
//             await SaveResultsToFileAsync(outputFilePath);
//         }

//         private bool IsValidUrl(string url)
//         {
//             Uri uriResult;
//             return Uri.TryCreate(url, UriKind.Absolute, out uriResult!)
//                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
//         }

//         private async Task<IEnumerable<string>> CrawlAsync(string rootUrl)
//         {
//             List<string> urls = new List<string>();
//             try
//             {
//                 HtmlWeb web = new HtmlWeb();
//                 HtmlDocument document = await web.LoadFromWebAsync(rootUrl);

//                 urls.AddRange(ExtractUrlsFromTags(document, "//a[@href]", "href"));
//                 urls.AddRange(ExtractUrlsFromTags(document, "//img[@src]", "src"));
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error crawling {rootUrl}: {ex.Message}");
//             }
//             return urls;
//         }

//         private IEnumerable<string> ExtractUrlsFromTags(HtmlDocument document, string xpath, string attributeName)
//         {
//             IEnumerable<string> urls = document.DocumentNode.SelectNodes(xpath)
//                 ?.Select(node => node.GetAttributeValue(attributeName, null))
//                 .Where(url => IsValidUrl(url)) ?? Enumerable.Empty<string>();
//             return urls;
//         }

//         private async Task SaveResultsToFileAsync(string filePath)
//         {
//             var groupedResults = _results.GroupBy(r => r.RootUrl);

//             using (StreamWriter writer = new StreamWriter(filePath))
//             {
//                 foreach (var group in groupedResults)
//                 {
//                     await writer.WriteLineAsync(group.Key);
//                     foreach (var url in group)
//                     {
//                         await writer.WriteLineAsync($"    {url.FetchedUrl}");
//                     }
//                     await writer.WriteLineAsync();
//                 }
//             }
//         }
//     }
// }



using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebCrawlerApp
{
    public class WebCrawler
    {
        private readonly ConcurrentDictionary<string, List<string>> _resultsDictionary;

        public WebCrawler()
        {
            _resultsDictionary = new ConcurrentDictionary<string, List<string>>();
        }

        public async Task CrawlAndSaveAsync(string[] rootUrls, string outputFilePath)
        {
            // Validate and filter root URLs
            List<string> validRootUrls = rootUrls.Where(url => IsValidUrl(url)).ToList();

            // Run crawling tasks concurrently
            Task[] tasks = validRootUrls.Select(url => Task.Run(async () =>
            {
                IEnumerable<string> fetchedUrls = await CrawlAsync(url);
                foreach (string fetchedUrl in fetchedUrls)
                {
                    AddUrlToDictionary(url, fetchedUrl);
                }
            })).ToArray();

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Save results to output file
            await SaveResultsToFileAsync(outputFilePath);
        }

        private void AddUrlToDictionary(string rootUrl, string fetchedUrl)
        {
            _resultsDictionary.AddOrUpdate(rootUrl,
                new List<string> { fetchedUrl },
                (key, existingList) =>
                {
                    existingList.Add(fetchedUrl);
                    return existingList;
                });
        }

        private bool IsValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult!)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private async Task<IEnumerable<string>> CrawlAsync(string rootUrl)
        {
            List<string> urls = new List<string>();
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = await web.LoadFromWebAsync(rootUrl);

                urls.AddRange(ExtractUrlsFromTags(document, "//a[@href]", "href"));
                urls.AddRange(ExtractUrlsFromTags(document, "//img[@src]", "src"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crawling {rootUrl}: {ex.Message}");
            }
            return urls;
        }

        private IEnumerable<string> ExtractUrlsFromTags(HtmlDocument document, string xpath, string attributeName)
        {
            return document.DocumentNode.SelectNodes(xpath)?
                .Select(node => node.GetAttributeValue(attributeName, null))
                .Where(url => IsValidUrl(url)) ?? Enumerable.Empty<string>();
        }

        private async Task SaveResultsToFileAsync(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (KeyValuePair<string, List<string>> kvp in _resultsDictionary)
                {
                    await writer.WriteLineAsync(kvp.Key);
                    foreach (string fetchedUrl in kvp.Value)
                    {
                        await writer.WriteLineAsync($"    {fetchedUrl}");
                    }
                    await writer.WriteLineAsync();
                }
            }
        }
    }
}

