using NUnit.Framework;
using System;

using Kidozen;

namespace kido.tests
{

	[TestFixture ()]
	public class ActiveAuthentication
	{
		KidoApplication kidozenApplication;
		ObjectSet database;

		[TestFixtureSetUp()]
		public void TestInit()
		{
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; 
            this.kidozenApplication = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);

        }

		[Test ()]
        public async void IsAuthenticated_ShouldBeTrue()
		{
			var user = await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			Assert.IsTrue (this.kidozenApplication.IsAuthenticated);
		}

        [Test()]
        public async void Authenticate_ShouldCreateValidUser()
        {
            var user = await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            Assert.IsNotNull(user);
            Assert.IsNotNullOrEmpty(user.UserName);
            Assert.IsNotNullOrEmpty(user.RawToken);
            Assert.IsTrue(user.AllClaims.Count > 0);
        }

	}
}

