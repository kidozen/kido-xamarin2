using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Object = Java.Lang.Object;

namespace Kidozen.Analytics.Android
{
    class ActivityTrackStatus
    {
        public int Status { get; set; } //1= paused, 2= stopped, 3 = started, 4 = resumed
        public string ClassName { get; set; }
        public DateTime Date { get; set; }
    }

    /*
     * The application was in background ONLY if any activity went through these states in this order:
     *      1 - Paused
     *      2 - Stopped
     *      3 - Started
     *      4 - Resumed
     * 
     * If OnActivityDestroyed callback was executed, the SDK does NOT fire the DidEnterBackground event.
     * For more information, please refer to :
     * http://developer.android.com/reference/android/app/Activity.html
     */
    class AnalyticsActivityLifecycleCallbacks : Object, Application.IActivityLifecycleCallbacks
    {
        private List<KeyValuePair<string, ActivityTrackStatus>> _normalFlowTracking =
            new List<KeyValuePair<string, ActivityTrackStatus>>();
        private readonly object _locker = new object();
        private static Action<double> _didEnterBackgroundAction;

        private void AddToDictionary(int status, Activity activity)
        {
            var classname = activity.GetType().FullName;
            lock (_locker)
            {
                _normalFlowTracking.Add(
                    new KeyValuePair<string, ActivityTrackStatus>(
                        classname,
                        new ActivityTrackStatus { Status = status, ClassName = classname, Date = DateTime.UtcNow })
                );
            }
        }

        public void OnActivityPaused(Activity activity)
        {
            //System.Console.WriteLine("1 - OnActivityPaused", activity.GetType().FullName);
            lock (_locker)
            {
                _normalFlowTracking = new List<KeyValuePair<string, ActivityTrackStatus>>();
            }
            AddToDictionary(1, activity);
        }
        public void OnActivityStopped(Activity activity)
        {
            //System.Console.WriteLine("2 - OnActivityStopped", activity.GetType().FullName);
            AddToDictionary(2, activity);
        }

        public void OnActivityStarted(Activity activity)
        {
            //System.Console.WriteLine("3 - OnActivityStarted", activity.GetType().FullName);
            AddToDictionary(3, activity);
        }

        public void OnActivityResumed(Activity activity)
        {
            //System.Console.WriteLine("4 - OnActivityResumed: " + activity.GetType().FullName + "; HashCode: " + activity.GetHashCode().ToString() );
            AddToDictionary(4, activity);

            var classname = activity.GetType().FullName;

            var ordered = _normalFlowTracking
                .Where(i => i.Key == classname)
                .OrderBy(itm => itm.Value.Status);


            if (ordered.ToList().Count == 4)
            {
                var flowConditions = EvaluateNormalFlowTracking(ordered).ToList();
                var wasInBackground = !flowConditions.Contains(false);

                //System.Console.WriteLine(wasInBackground ? "WAS IN BACKGROUND" : "NADA NADA", activity.GetType().FullName);
                if (_didEnterBackgroundAction!=null && wasInBackground)
                {

                    var startdate = ordered.FirstOrDefault(itm => itm.Value.Status == 1).Value.Date;
                    var enddate = ordered.FirstOrDefault(itm => itm.Value.Status == 4).Value.Date;

                    _didEnterBackgroundAction.Invoke(enddate.Subtract(startdate).TotalSeconds);
                }
            }
        }

        private static IEnumerable<bool> EvaluateNormalFlowTracking(IOrderedEnumerable<KeyValuePair<string, ActivityTrackStatus>> ordered)
        {
            var iter = ordered.GetEnumerator();
            iter.MoveNext();
            ;
            var previous = iter.Current;
            while (iter.MoveNext())
            {
                //Console.WriteLine( previous.Value.Date.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                var current = iter.Current;
                //Console.WriteLine(current.Value.Date.ToString("MM/dd/yyyy hh:mm:ss.fff tt")); 
                var wasInBackgroud = (previous.Value.Date < current.Value.Date);
                yield return wasInBackgroud;
                previous = iter.Current;
            }
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            //System.Console.WriteLine("OnActivityCreated", activity.GetType().FullName);
        }

        public void OnActivityDestroyed(Activity activity)
        {
            //System.Console.WriteLine("OnActivityDestroyed", activity.GetType().FullName);
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            //System.Console.WriteLine("OnActivitySaveInstanceState", activity.GetType().FullName);
        }

        public void BackgroundCallback(Action<double> didEnterBackground)
        {
            _didEnterBackgroundAction = didEnterBackground;
        }
    }

}
