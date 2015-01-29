using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kidozen
{
    public interface IDeviceInformation
    {
        iOS.SessionAttributes GetAttributes();
    }
}
