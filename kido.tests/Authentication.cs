using NUnit.Framework;
using System;

using Kidozen;

namespace kido.tests
{

	[TestFixture ()]
	public class Authentication
	{
		KidoApplication kidozenApplication;
		Storage.Storage database;

		[TestFixtureSetUp()]
		public void TestInit()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}

		[Test ()]
		public async void TestCase ()
		{
			var user = await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			Assert.IsNotNull (user);
		}
	}
}

