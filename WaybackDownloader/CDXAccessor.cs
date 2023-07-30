using AngleSharp;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using WaybackMachine.DotNet.Client;
using WaybackMachine.DotNet.Client.Models;

namespace WaybackDownloader
{
	// The Wayback CDX API is the means of querying what is available, and is thus the way to
	// download wildcards.
	public class CDXAccessor
	{
		public HttpClient Client { get; private set; }

		public const string BaseURL = "https://web.archive.org/cdx/search/cdx";

		public CDXAccessor()
		{
			Client = new HttpClient();
		}

		private string GetLocalPath(string url)
		{
			return GetLocalPath(new Uri(url));
		}

		private string GetLocalPath(Uri uri)
		{
			string path = uri.LocalPath;
			string query = Regex.Replace(uri.Query, @"[<>:\""/\\\|\?\*]", "_");
			path += query;
			path = Regex.Replace(path, @"^.*https?://.*?/", "");

			if(path.EndsWith("/") || Regex.IsMatch(path, @"/[-\w_]+$"))
			{
				path += "_.html";
			}

			if(Regex.IsMatch(path, @"\.(aspx|asp|php)"))
			{
				path = path.Replace("aspx_ID=", "_");
				path += ".html";
			}

			path = Regex.Replace(path, @"[<>:\""\|\?\*]", "_");

			return path;
		}

		private const string ErrorLogPath = "output/errors.txt";
		private async Task LogError(string contents)
		{
			if (!File.Exists(ErrorLogPath))
			{
				File.Create(ErrorLogPath);
				Thread.Sleep(1000);
			}
			string timestamp = $"[{DateTime.Now}] - ";
			await File.AppendAllTextAsync(ErrorLogPath, $"{timestamp}{contents}\n");
			Console.WriteLine(contents);
		}

		private List<string> KnownBadURLs = new List<string>()
		{
			"/large/",
			"/small/",
			"aboutdecipher/termsandusage/parentalconsent",
			"/thetroublewithtribbles/starterpages/",
			"/tournaments/summercons2001/",
			"/youngjedi/cardlists/menaceofdarthmaul/foil"
		};

		private List<string> BinaryFileTypes = new List<string>()
		{
			@".ra",
			@".rm",
			@".ram",
			@".mp3",
			@".ldc",
			@".doc",
			@".mov",
			@".rtf",
			@".zip",
			@".css",
		};

		public async Task FetchAllFilesFromURL(string url, string outputPath)
		{
			string domain = url.Replace("www.", "");
			domain = Regex.Replace(domain, @":\d+", "");
			domain = Regex.Match(domain, @"https?://([-_\w\.\*]+)/.*").Groups[1].Value;

			var fileInfo = await FetchAllFileInfoFromURL(url, outputPath);

			var wayback = new WaybackMachineService();

			foreach (var info in fileInfo)
			{
				string fileURL = info.OriginalURL;
				
				try
				{

					string path = $"{outputPath}/{domain}/{GetLocalPath(fileURL)}";

					List<string> existingPaths = new List<string>()
					{
						$"{outputPath}/decktech.net/{GetLocalPath(fileURL)}",
						$"{outputPath}/decipher.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/decipher.fanhq.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/decipherstore.fanhq.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/fanhq.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/lotrtcg.decipher.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/lotrtcg.fanhq.com/{GetLocalPath(fileURL)}",
						$"{outputPath}/shop.decipher.com/{GetLocalPath(fileURL)}",
					};


					if (File.Exists(path) || existingPaths.Any(x => File.Exists(x)))
					{
						Console.WriteLine($"'{path}' exists.  Skipping...");
						continue;
					}

					if ((KnownBadURLs.Any(x => fileURL.Contains(x)) && fileURL.EndsWith(".html"))
						|| fileURL.EndsWith("%20"))
					{
						await LogError($"Skipping errored URL '{fileURL}'.");
						continue;
					}

					if (fileURL.Contains("/cgi-bin/"))
					{
						await LogError($"Skipping unnecessary cgi-bin file '{fileURL}'.");
						continue;
					}

					await Task.Delay(4000);

					Console.WriteLine($"Fetching '{fileURL}'. . .");

					Func<string, DateTime, Snapshot> waybackMethod = (url, timestamp) => wayback.GetSnapshotClosestToDateAsync(url, timestamp).Result;

					var snapshot = waybackMethod(fileURL, info.Timestamp);
					if (snapshot == null || snapshot.ArchivedSnapshots == null || snapshot.ArchivedSnapshots.Closest == null)
					{
						if(!await wayback.HasSnapshot(fileURL))
						{
							fileURL = WebUtility.UrlEncode(fileURL);

							if (!await wayback.HasSnapshot(fileURL))
							{
								await LogError($"Failed to find file '{fileURL}'");
								continue;
							}
							else
							{
								waybackMethod = (url, timestamp) => wayback.GetMostRecentSnapshotAsync(url).Result;
								snapshot = waybackMethod(fileURL, info.Timestamp);
							}
							
						}
						else
						{
							waybackMethod = (url, timestamp) => wayback.GetMostRecentSnapshotAsync(url).Result;
						}
					}


					int attempts = 1;
					while(snapshot == null || snapshot.ArchivedSnapshots == null || snapshot.ArchivedSnapshots.Closest == null)
					{
						//if (fileURL.Contains(".php"))
						//	break;

						Console.WriteLine($"Retry {attempts} ...");
						await Task.Delay(60000);
						attempts++;
						snapshot = waybackMethod(fileURL, info.Timestamp);

						if(attempts > 3)
						{
							await LogError($"Aborting '{fileURL}' after 3 attempts\n");
							break;
						}
						
					}
					var snapshotURL = snapshot.ArchivedSnapshots.Closest.Url;

					if(info.MIMEType.Contains("image") || info.MIMEType.Contains("pdf") || BinaryFileTypes.Any(x => info.OriginalURL.EndsWith(x)))
					{
						snapshotURL = new Uri(snapshotURL.AbsoluteUri.Replace("/http", "if_/http"));
					}

					

					var result = await Client.GetAsync(snapshotURL);

					if (result.IsSuccessStatusCode)
					{
						//string path = $"{outputPath}/{domain}/{GetLocalPath(fileURL)}";

						var fi = new FileInfo(path);

						if (!fi.Exists)
						{
							Directory.CreateDirectory(fi.Directory.FullName);
						}

						if (info.MIMEType.Contains("text"))
						{
							string encoding = result.Content.Headers.ContentType.CharSet;

							switch (encoding)
							{
								case "maccentraleurope":
									encoding = "x-mac-ce";
									break;
								case "iso-8859-16":
									encoding = "windows-1250";
									break;
								case "iso-8859-10":
									encoding = "utf-8";
									break;
								default:
									encoding = "windows-1250";
									break;
							}
							string content = null;
							using (var sr = new StreamReader(await result.Content.ReadAsStreamAsync(), Encoding.GetEncoding(encoding)))
							{
								content = sr.ReadToEnd();
							}
							//string content = await result.Content.ReadAsStringAsync();
							string stripped = Regex.Replace(content, @"<!-- BEGIN WAYBACK.*END WAYBACK TOOLBAR INSERT-->", "", RegexOptions.Singleline);
							await File.WriteAllTextAsync(path, stripped);
						}
						//else if(BinaryFileTypes.Any(x => info.OriginalURL.EndsWith(x)))
						//{
						//	string content = await result.Content.ReadAsStringAsync();
						//	string actualFile = Regex.Match(content, @"iframe id=""playback"" src=""(.*?)""").Groups[1].Value;

						//	var actualResult = await Client.GetAsync(actualFile);

						//	using (var fs = new FileStream(path, FileMode.Create))
						//	{
						//		await actualResult.Content.CopyToAsync(fs);
						//	}

						//}
						else //if (info.MIMEType.Contains("image"))
						{
							//string content = await result.Content.ReadAsStringAsync();
							//string actualImage = Regex.Match(content, @"iframe id=""playback"" src=""(.*?)"")

							using (var fs = new FileStream(path, FileMode.Create))
							{
								await result.Content.CopyToAsync(fs);
							}
						}
						

					}
					else
					{
						await LogError($"Wayback returned status '{result.StatusCode}' for '{fileURL}':\n{result}\n\n");
					}
				}
				catch(Exception ex)
				{
					await LogError($"Failed to get file '{fileURL}':\n{ex}\n\n");
				}
			}
			
		}

		//web.archive.org/cdx/search/cdx?url=http://lotrtcg.fanhq.com/Resources/CardImages/*&output=json&page=1&fl=original,timestamp,mimetype,statuscode,length&collapse=timestamp:6
		public async Task<List<CDXResponse>> FetchAllFileInfoFromURL(string url, string outputPath)
		{
			string domain = Regex.Match(url, @"https?://([-_\w\.\*]+)/.*").Groups[1].Value;

			int pageCount = await FetchPageCount(url);

			var args = new Dictionary<string, string>()
			{
				["url"] = url,
				// API defaults to a TSV-like output
				["output"] = "json",
				// Restricts the return values to only use these fields; don't need the rest which are mostly redundant
				["fl"] = "original,timestamp,mimetype,statuscode,length",
				// Strips out redundant line stored items that share the same year
				["collapse"] = "digest",

				//Unfortunately, using the built-in filter functionality causes some legitimate 200 responses to get ommitted on
				// Wayback's side.  We have to filter them out ourselves.
				//["filter"] = "statuscode:2.."
			};

			string query = "?" + await new FormUrlEncodedContent(args).ReadAsStringAsync();

			var jsonSettings = new JsonSerializerSettings()
			{ 
				DateFormatString = "yyyyMMddhhmmss"
			};


			var totalResult = new List<CDXResponse>();

			for(int i = 0; i < pageCount; i++)
			{
				Console.WriteLine($"Fetching page {i} of '{url}'. . .");

				args["page"] = $"{i}";
				try
				{
					string cdxURL = await GetQueryURL(args);
					var results = await Client.GetAsync(cdxURL);
					string output = await results.Content.ReadAsStringAsync();
					// The first line is a header of sorts, which messes with deserialization
					output = output.Replace("[\"original\",\"timestamp\",\"mimetype\",\"statuscode\",\"length\"],", "");

					var items = JsonConvert.DeserializeObject<List<List<string>>>(output);
					foreach(var item in items)
					{
						totalResult.Add(new CDXResponse(item));
					}
					//totalResult.AddRange(items);

					
				}
				catch(Exception ex)
				{
					Console.WriteLine($"Failed to get page {i} of '{url}': \n\n{ex}\n\n");
				}
			}

			var finalResult = totalResult
				.Where(x => x.StatusCode == 200)
				.GroupBy(x => x.OriginalURL)
				.Select(x => x.Max())
				.ToList();

			await File.WriteAllTextAsync($"{outputPath}/{domain}.json", JsonConvert.SerializeObject(finalResult, jsonSettings));
			
			return finalResult;
		}

		//&showNumPages=true
		public async Task<int> FetchPageCount(string url)
		{
			var args = new Dictionary<string, string>()
			{
				["url"] = url,
				["showNumPages"] = "true"
			};

			string query = "?" + await new FormUrlEncodedContent(args).ReadAsStringAsync();

			var result = await Client.GetAsync(await GetQueryURL(args));

			if(result.IsSuccessStatusCode)
			{
				return Int32.Parse(await result.Content.ReadAsStringAsync());
			}
			else
			{
				throw new Exception($"Failed to get page count for '{url}': \n\n{result.StatusCode}\n{result}");
			}
		}

		private async Task<string> GetQueryURL(Dictionary<string, string> args, string url=BaseURL)
		{
			return $"{url}?{await new FormUrlEncodedContent(args).ReadAsStringAsync()}";
		}
	}
}
