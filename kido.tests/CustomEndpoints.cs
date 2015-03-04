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

    [TestFixture()]
    public class CustomEndpoints
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
            var ds = kidozenApplication.CustomEndpoint("customScript1");
            var results = await ds.Execute<EndpointResponse>();
            Assert.IsNotNull(results.length);
            Assert.AreEqual(4, results.length);
        }

        [Test()]
        public async void ShouldExecuteAsString()
        {
            await this.kidozenApplication.Authenticate(Settings.User, Settings.Pass, Settings.Provider);
            var ds = kidozenApplication.CustomEndpoint("customScript2");
            var results = await ds.Execute( new { name = "kidoScript"});
            Assert.IsTrue(results.IndexOf("name") > -1);
        }

        //
    }

}

