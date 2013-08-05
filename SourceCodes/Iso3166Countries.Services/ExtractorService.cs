using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using Iso3166Countries.Services.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Iso3166Countries.Services
{
	/// <summary>
	/// This represents the extractor service entity.
	/// </summary>
	public class ExtractorService
	{
		#region Constructors

		/// <summary>
		/// Initialises a new instance of the ExtractorService object.
		/// </summary>
		/// <param name="settings">Configuration settings instance.</param>
		public ExtractorService(Settings settings)
		{
			this._settings = settings;
		}

		#endregion Constructors

		#region Properties

		private readonly Settings _settings;

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the list of XML elements from ISO.
		/// </summary>
		/// <returns>Returns the list of XML elements from ISO.</returns>
		public IList<XElement> GetIso3166CountriesFromIso()
		{
			var url = this._settings.Iso3166CountriesUrl;
			XDocument xml;
			var request = WebRequest.Create(url);
			using (var response = request.GetResponse())
			using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(this._settings.Iso3166CountriesXmlEncoding)))
			{
				xml = XDocument.Load(reader);
			}
			if (xml.Root == null)
				return null;

			if (xml.Root.Name != this._settings.Iso3166CountriesXmlRootElement)
				return null;

			var elements = xml.Root.Elements(this._settings.Iso3166CountriesXmlEntryElement).ToList();
			return elements.Any() ? elements : null;
		}

		/// <summary>
		/// Converts the list of XML elements to the list of countries.
		/// </summary>
		/// <param name="entries">List of XML elements derived from ISO.</param>
		/// <returns>Returns the list of countries.</returns>
		public IList<Country> Convert(IList<XElement> entries)
		{
			if (entries == null || !entries.Any())
				return null;

			var countries = entries.Select(p =>
										   new Country()
											   {
												   Name = p.Element(this._settings.Iso3166CountriesXmlCountryNameElement).Value,
												   Code = p.Element(this._settings.Iso3166CountriesXmlCountryCodeElement).Value
											   })
								   .ToList();

			foreach (var order in this._settings.CountriesInOrder)
			{
				var country = countries.SingleOrDefault(p => p.Code.ToUpper() == order.Key);
				if (country == null)
					continue;

				countries.Single(p => p.Code.ToUpper() == order.Key).SortOrder = this._settings.CountriesInOrder.IndexOf(order) + 1;
				countries.Single(p => p.Code.ToUpper() == order.Key).Name = order.Value;
			}
			return countries.OrderByDescending(p => p.SortOrder.HasValue)
							.ThenBy(p => p.SortOrder)
							.ThenBy(p => p.Name)
							.ToList();
		}

		/// <summary>
		/// Exports the list of countries to XML.
		/// </summary>
		/// <param name="countries">List of countries.</param>
		public void Export(IList<Country> countries)
		{
			if (countries == null || !countries.Any())
				return;

			var filepath = this._settings.ExportFilepath + "\\" + this._settings.ExportFilename;
			using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
			{
				var collection = new Countries() { Country = countries.ToArray() };
				var serialiser = new XmlSerializer(typeof(Countries));
				serialiser.Serialize(stream, collection);
			}
		}

		#endregion Methods
	}
}