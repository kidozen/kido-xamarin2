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

        [Test()]
        public async void Authenticate_ShouldCreateValidUser2()
        {
            /*
            var app = new KidoApplication("https://bellhowelldev.kidocloud.com", 
                "sda2", "WLasDb3gRhTzzgpD4iQo82H1NIwQB7n7yxDk/d88PqE=");

            var user = app.Authenticate("bellhowelldev@kidozen.com", "3b7a3d4cb41c", "Kidozen").Result;
            */

            var app = new KidoApplication("https://bellhowell.kidocloud.com", "poc", "htZrzUAG3WaJnmj98uoLtpXrstqbfomL8/otR1HBSVE=");

            var user = app.Authenticate("bellhowell@kidozen.com", "83c0423dec94", "Kidozen").Result;

            Assert.IsNotNull(user);
        }
	}
}

