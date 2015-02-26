using NUnit.Framework;
using System;

using Kidozen;

namespace kido.tests
{

	[TestFixture ()]
	public class Authentication
	{
		KidoApplication kidozenApplication;
		ObjectSet database;

		[TestFixtureSetUp()]
		public void TestInit()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

		[Test ()]
		public async void ShouldAuthenticateActive ()
		{
			var user = await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			Assert.IsNotNull (user);
		}
	}
}

