using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


using Kidozen;

namespace kido.tests
{
    [TestFixture()]
    public class FileDataSources
    {
        KidoApplication kidozenApplication;

        [TestFixtureSetUp()]
        public void TestInit()
        {
            this.kidozenApplication = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        [Test()]
        public async void ShouldQueryFile()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.DataSource("fileGetDs");
            var results = await ds.QueryFile(new { fullpath = "/Users/christian/img1.png" });
            Assert.IsNotNull(results);
        }

        [Test()]
        public async void ShouldQuery()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.DataSource("sampleTable");
            var results = await ds.Query<IEnumerable<MyClass>>();
            Assert.IsNotNull(results);
        }
    }

    public class MyClass
    {
        public string id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }

    }
}

