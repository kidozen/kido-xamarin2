using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncSDK
{
    //La entidad
    class Order {
        public string CustomerId {get;set;}
        public string Id { get; set; }
        public int Total { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Se maneja 1 datasync por tipo
            var ds = new DataSync<Order>("Orders");
            var order = new Order { CustomerId = "2", Id = "a", Total = 4 };

            //Internamente serializa y deserializa para generar los formatos que necesita el server
            ds.Create(order);

            // Devuelve un IEnumerable<Order> filtrado por Func<Order, bool>
            var filter = ds.Query(x => x.CustomerId == "1");
            
            ds.Delete(filter.FirstOrDefault());

            // Devuelve todos los docs en un IEnumerable<Order>
            var all = ds.Query();

            //Primera version oneway 
            ds.Synchronize();

            // Acceso a la base de datos , para que puedan hacer mas cosas que soporta couch
            // pero por ahora nosotros no .. ej Crear Views basadas en Map Reduce
            var couch = ds.Database;
           

        }
    }
}
