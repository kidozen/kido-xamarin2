using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Kidozen;
using Kidozen.iOS;


namespace DataVisTouch
{
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
			return kido.Authenticate (Settings.User, Settings.Pass, Settings.Provider)
				.ContinueWith(t=> { 
					return !t.IsFaulted;}
				);
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
	}
}



