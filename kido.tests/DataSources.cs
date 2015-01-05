using NUnit.Framework;
using System;

using Kidozen;

namespace kido.tests
{
	[TestFixture ()]
	public class DataSources
	{
		KidoApplication kidozenApplication;

		[TestFixtureSetUp()]
		public void TestInit()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}

		[Test ()]
		public async void TestCase ()
		{
			await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			var ds = kidozenApplication.DataSource ("FileDownloadAsQuery");
			var results = await ds.QueryFile (new {fullpath =""});
			Assert.IsNotNull (results);
		}
	}
}

