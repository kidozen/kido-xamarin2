using NUnit.Framework;
using System;

using Kidozen;

namespace kido.tests
{
	public class MyEntity {
		public string _id { get; set; }
		public Metadata _metadata { get; set;}
		public string Bar { get; set;}
	}

	[TestFixture ()]
	public class Storage
	{
		KidoApplication kidozenApplication;

		[TestFixtureSetUp()]
		public void TestInit()
		{
			this.kidozenApplication = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

        }

		[Test ()]
		public async void ShouldUpdate ()
		{
			await this.kidozenApplication.Authenticate (Settings.User, Settings.Pass, Settings.Provider);
			var os = kidozenApplication.ObjectSet ("tests");
			var entityMetadata = await os.Create<MyEntity> (new MyEntity {Bar ="foo"});
			Assert.IsNotNull (entityMetadata);
			var updated = new MyEntity { Bar = "foooo", _metadata = entityMetadata._metadata, _id = entityMetadata._id };
			var saved = await os.Save<MyEntity> (updated);
			Assert.IsNotNull (saved);
		}
	}
}

