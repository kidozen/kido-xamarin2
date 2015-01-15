using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Kidozen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kidozen.Examples.Portable
{
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
                System.Diagnostics.Debug.WriteLine(t.Result);
                return !t.IsFaulted; 
            });
        }
    }


}
