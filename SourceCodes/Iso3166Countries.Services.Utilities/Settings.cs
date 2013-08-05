using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Iso3166Countries.Services.Utilities
{
	/// <summary>
	/// This represents the application configuration settings entity.
	/// </summary>
	public class Settings
	{
		#region Constants

		#endregion Constants

		#region Constructors

		/// <summary>
		///	Initialises a new instance of the Settings object as private.
		/// </summary>
		private Settings()
		{
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets the instance of the settings object.
		/// </summary>
		public static Settings Instance
		{
			get
			{
				if (_instance == null)
					_instance = new Settings();
				return _instance;
			}
		}

		private static Settings _instance;

		private HttpContextBase _httpContext;

		/// <summary>
		/// Gets or sets the current HttpContext instance.
		/// </summary>
		public HttpContextBase HttpContext
		{
			get
			{
				if (this._httpContext == null)
				{
					try
					{
						this._httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
					}
					catch
					{
					}
				}
				return this._httpContext;
			}
			set
			{
				this._httpContext = value;
			}
		}

		/// <summary>
		/// Gets the ISO 3166 country list URL.
		/// </summary>
		public string Iso3166CountriesUrl
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesUrl"];
				return String.IsNullOrWhiteSpace(value) ? null : value;
			}
		}

		/// <summary>
		/// Gets the encoding of ISO 3166 country list XML document.
		/// </summary>
		public string Iso3166CountriesXmlEncoding
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesXmlEncoding"];
				return String.IsNullOrWhiteSpace(value) ? "utf-8" : value;
			}
		}

		/// <summary>
		/// Gets the name of the root element of ISO 3166 country list XML document.
		/// </summary>
		public string Iso3166CountriesXmlRootElement
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesXmlRootElement"];
				return String.IsNullOrWhiteSpace(value) ? "ISO_3166-1_List_en" : value;
			}
		}

		/// <summary>
		/// Gets the name of the entry element of ISO 3166 country list XML document.
		/// </summary>
		public string Iso3166CountriesXmlEntryElement
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesXmlEntryElement"];
				return String.IsNullOrWhiteSpace(value) ? "ISO_3166-1_Entry" : value;
			}
		}

		/// <summary>
		/// Gets the name of the country name element of ISO 3166 country list XML document.
		/// </summary>
		public string Iso3166CountriesXmlCountryNameElement
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesXmlCountryNameElement"];
				return String.IsNullOrWhiteSpace(value) ? "ISO_3166-1_Country_name" : value;
			}
		}

		/// <summary>
		/// Gets the name of the country code element of ISO 3166 country list XML document.
		/// </summary>
		public string Iso3166CountriesXmlCountryCodeElement
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.Iso3166CountriesXmlCountryCodeElement"];
				return String.IsNullOrWhiteSpace(value) ? "ISO_3166-1_Alpha-2_Code_element" : value;
			}
		}

		/// <summary>
		/// Gets the list of countries to display prior to others.
		/// </summary>
		public IList<KeyValuePair<string, string>> CountriesInOrder
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.CountriesInOrder"];
				if (String.IsNullOrWhiteSpace(value))
					return null;
				return value.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries)
				            .Select(country => country.Trim('{', '}')
				                                      .Split(new string[] {":"}, StringSplitOptions.RemoveEmptyEntries))
				            .Select(entry => new KeyValuePair<string, string>(entry[0], entry[1].ToUpper()))
				            .ToList();
			}
		}

		/// <summary>
		/// Gets the XML export file path.
		/// </summary>
		public string ExportFilepath
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.ExportFilepath"];
				if (String.IsNullOrWhiteSpace(value))
					value = "~/Export";
				value = this.GetFullPath(value);
				if (!Directory.Exists(value))
					Directory.CreateDirectory(value);
				return value;
			}
		}

		/// <summary>
		/// Gets the XML export filename.
		/// </summary>
		public string ExportFilename
		{
			get
			{
				var value = ConfigurationManager.AppSettings["App.ExportFilename"];
				if (String.IsNullOrWhiteSpace(value))
					value = "Iso3166Countries.xml";
				return value;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets the full path of the given application root path.
		/// </summary>
		/// <param name="path">Application root path.</param>
		/// <returns>Returns the full path of the given application root path.</returns>
		private string GetFullPath(string path)
		{
			if (Regex.IsMatch(path, @"^[a-z]://", RegexOptions.Compiled | RegexOptions.IgnoreCase))
				return path;

			if (path.StartsWith("/"))
				path = "~" + path;
			else if (!path.StartsWith("~/"))
				path = "~/" + path;

			var context = this.HttpContext;
			if (context == null)
			{
				var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				if (!String.IsNullOrEmpty(directory))
					path = String.Format("{0}\\{1}",
										 directory.TrimEnd('/', '\\'),
										 path.Replace("~/", "").Replace("/", "\\").Trim('/', '\\'));
			}
			else
				path = context.Server.MapPath(path);
			return path;
		}

		#endregion Methods
	}
}