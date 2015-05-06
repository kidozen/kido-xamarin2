using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


using Kidozen;

namespace kido.tests
{
    public class Weather
    {
        public string main { get; set; }
        public string description { get; set; }
    }

    public class WeatherResponse
    {
        public List<Weather> weather { get; set; }
    }

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
        [Test()]
        public async void ShouldInvokeAsType()
        {
            var Marketplace = "iffdev.kidocloud.com";
            var Application = "approvals";
            var Key = "MqmtCNQQWKymbRcJtfFRWWyU+rcOjR+r8/AMsh+agBs=";

            this.kidozenApplication = new KidoApplication(Marketplace, Application, Key);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            await this.kidozenApplication.Authenticate("iff@kidozen.com", "sup3r+", Settings.Provider);

            var results = kidozenApplication.DataSource("updateapprovalrequest").Invoke(new { RefId = "A" }).Result;

            Assert.IsTrue(results.IndexOf("Gustavo") > -1);
        }
    }

}

