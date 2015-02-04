using System.Threading.Tasks;
using Android.App;
using Kidozen;
using Kidozen.Android;
using Kidozen.Examples;
using System.Threading;
namespace AnalyticsDroid
{
	public class Model
	{
	    readonly KidoApplication _kido;

		public Model ()
		{
			_kido = new KidoApplication (Settings.Marketplace, Settings.Application, Settings.Key);

		}
		/// <summary>
		/// Authenticates against Kidozen. 
		/// returns true is success
		/// </summary>
		public Task<bool> Authenticate() {
			return _kido.Authenticate (Settings.User, Settings.Pass, Settings.Provider)
				.ContinueWith(t=> !t.IsFaulted);
		}


        public void EnableAnalytics(Application application)
        {
            _kido.EnableAnalytics(application);
        }


        internal void TagCustom<T>(T p)
        {
            _kido.TagCustom("custom",p);
        }

        internal void TagButton(string p)
        {
            _kido.TagClick(p);
        }

        internal void TagView(string p)
        {
            _kido.TagView(p);
        }
    }
}



