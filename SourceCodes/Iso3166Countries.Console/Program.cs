using Iso3166Countries.Services;
using Iso3166Countries.Services.Utilities;

namespace Iso3166Countries.Console
{
	public class Program
	{
		private static void Main(string[] args)
		{
			var service = new ExtractorService(Settings.Instance);
			var entries = service.GetIso3166CountriesFromIso();
			var countries = service.Convert(entries);
			service.Export(countries);
		}
	}
}