using System;
using UIKit;
using Foundation;
using Kidozen;
using System.Threading.Tasks;

namespace Kidozen.iOS
{

	public class KidozenDelegate : NSObject
	{
		public KidoApplication kzApplication { get; set; }

//		public bool strictSSL {get; set;}

		public String deviceToken {get; set;}

		/// <summary>
		///  This method should be called to initialize the Kidozen instance.
		/// You should use this as this:
		/// 
		/// 	kidoAppDelegate.initializeKidozen(launchOptions).ContinueWith(.....);
		/// 	kidoAppDelegate.Result(...);
		/// 
		/// </summary>
		/// <returns>A Task with the result of the initialization.</returns>
		/// <param name="launchOptions">Launch options dictionary that needs to have the following keys and values:
		/// 			* marketPlaceURL
		/// 			* applicationKey
		/// 			* applicationName
		/// 			* username
		/// 			* password
		/// 			* provider.
		/// </param>
		/// 
		public Task<bool> initializeKidozen(NSDictionary launchOptions) {
			
			this.registerForRemoteNotifications();

			return this.initializeAndAuthenticate (launchOptions).ContinueWith(theUser=> { 
					if (theUser.IsFaulted) {
						return false;
					} else {
						this.handleLaunchOptions(launchOptions);
						return true;
					}
				});

		}
		
		private void handleLaunchOptions(NSDictionary launchOptions) {
			NSDictionary notificationDictionary = launchOptions [UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
			this.handleNotificationEvent(notificationDictionary);
		}

		public void resetBadgeCount() {
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
		}

		private void handleNotificationEvent(NSDictionary notificationDictionary) {
			// check if notificationdictionary has a badge item.
			if (notificationDictionary != null) {
				// Application has been opened by tapping on the notification.
				// So, we reset the badge count.
				this.resetBadgeCount();
				NSString trackContextKey = (NSString) "trackContext";
				if (notificationDictionary.ContainsKey (trackContextKey)) {
					NSDictionary trackContext = (NSDictionary) notificationDictionary [trackContextKey];
					kzApplication.TrackNotification(trackContext);
				}

			}
		}

		public void DidRegisterUserNotificationSettings(UIUserNotificationSettings notificationSettings) {
			UIApplication.SharedApplication.RegisterForRemoteNotifications ();
		}



		public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			if (UIApplication.SharedApplication.ApplicationState == UIApplicationState.Background ||
			    UIApplication.SharedApplication.ApplicationState == UIApplicationState.Inactive) {
				this.handleNotificationEvent (userInfo);
			}
			
		}

		public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken) {
			this.deviceToken = deviceToken.ToString();
		}

		public void OnActivated() {
			this.resetBadgeCount ();
		}

		public void registerForRemoteNotifications() {
			UIUserNotificationSettings notificationSettings = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Badge | UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
			UIApplication.SharedApplication.RegisterUserNotificationSettings (notificationSettings);
		}

		private Task<User> initializeAndAuthenticate(NSDictionary d) {
			NSString marketPlaceURL = (NSString) d["marketPlaceURL"];
			NSString applicationName = (NSString) d["applicationName"];
			NSString applicationKey = (NSString) d["applicationkey"];
			NSString username = (NSString) d["username"];
			NSString password = (NSString) d["password"];
			NSString provider = (NSString) d["provider"];

			this.kzApplication = new KidoApplication(marketPlaceURL, applicationName, applicationKey);

			// TODO: Consider using passive authentication.
			return this.kzApplication.Authenticate (username, password, provider);
		}
			
	}
}

