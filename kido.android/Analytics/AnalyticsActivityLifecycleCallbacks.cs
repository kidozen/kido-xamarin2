using System;
using Android.App;
using Android.OS;
using Debug = System.Diagnostics.Debug;

namespace Kidozen.Analytics.Android
{
    class AnalyticsActivityLifecycleCallbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        private static int _activityCount = 0;

        public void OnActivityPaused(Activity activity)
        {
            // la activity pasa a back, decremento el contador
            Debug.WriteLine("1 - OnActivityPaused", activity.GetType().FullName);
        }
        public void OnActivityStopped(Activity activity)
        {
            Debug.WriteLine("2 - OnActivityStopped", activity.GetType().FullName);
        }

        public void OnActivityStarted(Activity activity)
        {
            Debug.WriteLine("3 - OnActivityStarted", activity.GetType().FullName);
        }

        public void OnActivityResumed(Activity activity)
        {
            //la activity vuelve a ser vista por el usuario
            Debug.WriteLine("4 - OnActivityResumed", activity.GetType().FullName);
        }


        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            //aca incremento el contador
            Debug.WriteLine("OnActivityCreated", activity.GetType().FullName);
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Debug.WriteLine("OnActivityDestroyed", activity.GetType().FullName);
            
        }


        
        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Debug.WriteLine("OnActivitySaveInstanceState", activity.GetType().FullName);
        }


        
    }
}