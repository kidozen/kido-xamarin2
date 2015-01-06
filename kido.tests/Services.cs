using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


using Kidozen;

namespace kido.tests
{
	[TestFixture ()]
	public class Services
	{
		KidoApplication kidozenApplication;

		[TestFixtureSetUp()]
		public void TestInit()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
		}

		[Test ()]
		public async void ShouldDownloadFile ()
		{
			await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			var ds = kidozenApplication.Service ("fileServerTest01");
			var results = await ds.InvokeFile("get", new {fullpath ="/Users/christian/img1.png"});
			Assert.IsNotNull (results);
		}
	}
}

