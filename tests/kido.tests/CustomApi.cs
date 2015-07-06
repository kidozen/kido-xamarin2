using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;


using Kidozen;

namespace kido.tests
{
    public class EndpointResponse
    {
        public string length { get; set; }
    }

    [TestFixture()]
    public class CustomApi
    {
        KidoApplication kidozenApplication;

        [TestFixtureSetUp()]
        public void TestInit()
        {
            this.kidozenApplication = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }


        [Test()]
        public async void ShouldExecuteAsType()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.CustomApi("testscript1");
            var results = await ds.Execute<EndpointResponse>();
            Assert.IsNotNull(results.length);
            Assert.AreEqual(4, results.length);
        }

        [Test()]
        public async void ShouldExecuteAsString()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.CustomApi("testscript1");
            var results = await ds.Execute(new { name = "kidoScript" });
            Assert.IsTrue(results.IndexOf("name") > -1);
        }

        //
    }

}

