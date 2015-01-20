using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Kidozen.Examples.Portable;
namespace WindowsDs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ds = new MyDataSource();
            ds.Authenticate().ContinueWith(
                t =>
                {
                    var isauth = t.Result;
                    if (isauth)
                    {
                        var result = ds.QueryDataSoruce<DsParams>("getTellagoVacationsfff", new DsParams { qty = 2 }).Result;
                        System.Diagnostics.Debug.WriteLine(result.ToString());

                    }
                }
           );
        }
    }
}
