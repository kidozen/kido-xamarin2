using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


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
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
		}


		[Test ()]
		public async void ShouldQueryAsType ()
		{
			await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.DataSource("query-for-tests");
            var results = await ds.Query<WeatherResponse>(new {city = "Buenos Aires,AR"});
			Assert.IsNotNull (results.weather);
		}

        [Test()]
        public async void ShouldQueryAsString()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.DataSource("query-for-tests");
            var results = await ds.Query(new {city = "Buenos Aires,AR"});
            Assert.IsTrue(results.IndexOf("description")>-1);
        }

        [Test()]
        public async void ShouldQueryAsString2()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);

            var ds = kidozenApplication.DataSource("sql-query-witherror");
            var results = await ds.Query();
            Assert.IsTrue(results.IndexOf("Gustavo") > -1);
        }
        //
    }

}

