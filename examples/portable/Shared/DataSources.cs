using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kidozen.Examples.Portable
{
    class DsParams
    {
        public int qty { get; set; }
    }

    public class MyDataSource
    {
        KidoApplication kido;

        public MyDataSource()
        {
            kido = new KidoApplication(Settings.Marketplace, Settings.Application, Settings.Key);
        }
        /// <summary>
        /// Authenticates against Kidozen. 
        /// returns true is success
        /// </summary>
        public Task<bool> Authenticate()
        {
            return kido.Authenticate(Settings.User, Settings.Pass, Settings.Provider)
                .ContinueWith(t =>
                {
                    return !t.IsFaulted;
                }
                );
        }


        public Task<bool> QueryDataSoruce<T>(string name, T paramters)
        {
            var qds = kido.DataSource(name);
            return qds.Query<JObject>(paramters).ContinueWith(t => {
                Debug.WriteLine(t.Result);
                return !t.IsFaulted; 
            });
        }
    }


}
