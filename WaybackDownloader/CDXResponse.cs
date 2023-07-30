using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace WaybackDownloader
{
	public class CDXResponse : IComparable<CDXResponse>
	{
		//[JsonProperty(PropertyName="urlkey")]
		//public string URLKey { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime Timestamp { get; set; }

		[JsonProperty(PropertyName = "original")]
		public string OriginalURL { get; set; }

		[JsonProperty(PropertyName = "mimetype")]
		public string MIMEType { get; set; }

		[JsonProperty(PropertyName = "statuscode")]
		public int StatusCode { get; set; }

		//[JsonProperty(PropertyName = "digest")]
		//public string Digest { get; set; }

		[JsonProperty(PropertyName = "length")]
		public int Length { get; set; }

		public CDXResponse(List<string> entries)
		{
			OriginalURL = entries[0];
			// Y2K??
			Timestamp = DateTime.ParseExact(entries[1].Replace("200000", "200001"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
			MIMEType = entries[2];
			if(entries[3] == "-")
			{
				StatusCode = 0;
			}
			else
			{
				StatusCode = int.Parse(entries[3]);
			}
			
			Length = int.Parse(entries[4]);
		}

		public int CompareTo(object obj)
		{
			if (obj is CDXResponse other)
				return CompareTo(other);

			return Timestamp.CompareTo(obj);
		}

		public int CompareTo([AllowNull] CDXResponse other)
		{
			if (other == null)
				return -1;

			return Timestamp.CompareTo(other.Timestamp);
		}
	}
}
