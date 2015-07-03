using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Kidozen;
using Kidozen.Examples;
using Kidozen.iOS;
using Kidozen.iOS.Analytics;


namespace Examples
{

    public class ScenariosList
    {
        public string StatusCode { get; set; }
        public List<Scenario> Scenarios { get; set; }
    }

    public class Scenario
    {
        public int SharePointId { get; set; }
        public string Title { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryColor { get; set; }
        public string ScenarioDescription { get; set; }
        public string Response { get; set; }
        public string VideoURL { get; set; }
        public DateTime Modified { get; set; }
        public string Status { get; set; }
        public bool IsBookmarked { get; set; }
    }

	public class Model
	{
		KidoApplication kido;

		public Model ()
		{
			kido = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);

		}
		/// <summary>
		/// Authenticates against Kidozen. 
		/// returns true is success
		/// </summary>
		public Task<bool> Authenticate() {
			return kido.Authenticate ()
				.ContinueWith(t=> {
                    var scenarios = kido.DataSource("GetScenarios").Query<ScenariosList>(new { lastUpdated = "2015-04-07T03:11:18" }).Result;

					return !t.IsFaulted;
                }
			);
		}


        public void EnableAnalytics()
        {
            kido.EnableAnalytics();
        }

		/// <summary>
		/// The current version of ShowDataVisualization is synchronous, so this method wraps the call in a task
		/// </summary>
		/// <returns>The data visualization.</returns>
		/// <param name="name">Name of the data visualization to invoke.</param>
		public Task<bool> DisplayDataVisualization (string name) {
			return Task.Factory.StartNew( () => {
				try {
					kido.ShowDataVisualization(name);
					return true;
				}  catch (Exception ex) {
					Debug.WriteLine(ex.Message);
					return false;					
				}
			} );
		}


        internal void TagCustom<T>(T p)
        {
            kido.TagCustom("custom",p);
        }

        internal void TagButton(string p)
        {
            kido.TagClick(p);
        }

        internal void TagView(string p)
        {
            kido.TagView(p);
        }
    }
}



