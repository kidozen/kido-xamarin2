using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public interface IOfflineCache {
	string GetTargetDirectory (); 
}
