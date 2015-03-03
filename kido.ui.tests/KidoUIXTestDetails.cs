using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace kido.ui.tests
{
    public class KidoUIXTestDetails<T>
    {
        public string Name { get; set; }
        public T ExpectedValue { get; set; }
    }
}