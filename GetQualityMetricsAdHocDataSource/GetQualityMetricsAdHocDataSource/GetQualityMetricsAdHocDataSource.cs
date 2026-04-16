using System;
using Skyline.DataMiner.Analytics.GenericInterface;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace GetQualityMetricsAdHocDataSource
{
    /// <summary>
    /// Represents a data source.
    /// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
    /// </summary>
    [GQIMetaData(Name = "GetQualityMetricsAdHocDataSource")]
    public sealed class GetQualityMetricsAdHocDataSource : IGQIDataSource
    {
        public GQIColumn[] GetColumns()
        {
			var columns = new GQIColumn[]
			{
				new GQIStringColumn("Task ID"),
				new GQIStringColumn("ID"),
				new GQIStringColumn("Protocol"),
				new GQIStringColumn("Version"),
				new GQIStringColumn("Branch"),
				new GQIStringColumn("Based On Version"),
				new GQIStringColumn("Vendor"),
				new GQIStringColumn("Tagger"),
				new GQIDateTimeColumn("Release Date"),
				new GQIDoubleColumn("Quality Score"),
				new GQIDoubleColumn("Quality Score Delta"),
				new GQIDoubleColumn("Unit Tests"),
				new GQIDoubleColumn("Unit Tests Delta"),
				new GQIDoubleColumn("Unit Tests Coverage"),
			};

			return columns;

			// Define data source columns
			//// See: https://aka.dataminer.services/igqidatasource-getcolumns
			//return Array.Empty<GQIColumn>();
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage

			List<ProtocolVersion> qualityMetrics = GetQualityMetrics();

			var rows = qualityMetrics.Select(CreateRow).ToArray();

            return new GQIPage(rows)
            {
                HasNextPage = false,
            };
        }

		public GQIRow CreateRow(ProtocolVersion protocolVersion)
		{
			var cells = new GQICell[]
			{
				new GQICell() { Value = protocolVersion.TaskId },
				new GQICell() { Value = protocolVersion.ID },
				new GQICell() { Value = protocolVersion.Name },
				new GQICell() { Value = protocolVersion.Version },
				new GQICell() { Value = protocolVersion.Branch },
				new GQICell() { Value = protocolVersion.BasedOnVersion },
				new GQICell() { Value = protocolVersion.Vendor },
				new GQICell() { Value = protocolVersion.Tagger },
				new GQICell() { Value = protocolVersion.ReleaseDate.Kind == DateTimeKind.Utc
					? protocolVersion.ReleaseDate
					: protocolVersion.ReleaseDate.ToUniversalTime() },
				new GQICell() { Value = protocolVersion.QualityScore },
				new GQICell() { Value = protocolVersion.QualityScoreDelta },
				new GQICell() { Value = protocolVersion.UnitTests },
				new GQICell() { Value = protocolVersion.UnitTestsDelta },
				new GQICell() { Value = protocolVersion.UnitTestsCoverage },
			};

			return new GQIRow(cells);
		}

		public List<ProtocolVersion> GetQualityMetrics()
		{
			// Use 'using' to ensure HttpClient is disposed properly.
			using (var client = new HttpClient())
			{
				

				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

				try
				{
					var response = client.GetAsync("https://slc-h53-g01.skyline.local/api/custom/GetProtocolQAMonitorMetrics").GetAwaiter().GetResult();
					response.EnsureSuccessStatusCode();

					string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					List<ProtocolVersion> protocolVersions = JsonConvert.DeserializeObject<List<ProtocolVersion>>(json);

					return protocolVersions;
				}
				catch (HttpRequestException ex)
				{
					// Log or handle the exception as needed.
					// For now, rethrow or return a meaningful message.
					throw new InvalidOperationException("Failed to retrieve quality metrics.", ex);
				}
			}
		}
	}

	public class ProtocolVersion
	{
		public string ID { get; set; }

		public string Name { get; set; }

		public string Version { get; set; }

		public string Branch { get; set; }

		public string BasedOnVersion { get; set; }

		public string Vendor { get; set; }

		public string Tagger { get; set; }

		public string TaskId { get; set; }

		public DateTime ReleaseDate { get; set; }

		public double QualityScore { get; set; }

		public double QualityScoreDelta { get; set; }

		public double UnitTests { get; set; }

		public double UnitTestsDelta { get; set; }

		public double UnitTestsCoverage { get; set; }
	}
}
