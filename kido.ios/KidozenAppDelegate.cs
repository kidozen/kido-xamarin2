using System;
using UIKit;
using Foundation;
using Kidozen;
using System.Threading.Tasks;

namespace Kidozen.iOS
{

	public class KidozenAppDelegate : UIApplicationDelegate
	{
		public KidoApplication application { get; set; }
		public UIWindow windows { get; set; }

		public String marketPlaceURL { get; set; }
		public String applicationName { get; set; }
		public String applicationKey { get; set; }

		public String user { get; set; }
		public String password { get; set; }
		public String provider { get; set; }

		public bool notificationsEnabled {get; set;}
		public bool strictSSL {get; set;}

		public String deviceToken {get; set;}

		/// <summary>
		///  This method should be called to initialize the Kidozen instance.
		/// </summary>
		/// <returns>A Task with the result of the initialization.</returns>
		/// <param name="launchOptions">Launch options.</param>
		public Task<bool> initializeKidozen(NSDictionary launchOptions) {
			this.registerForRemoteNotifications();

			return Task.Factory.StartNew( () => {
					this.initializeKidozen ().ContinueWith(theUser=> { 
						if (theUser.IsFaulted) {
							return false;
						} else {
							this.handleLaunchOptions(launchOptions);
							return true;
						}
					});
				}) as Task<bool>;
		}
		
		private void handleLaunchOptions(NSDictionary launchOptions) {
			NSDictionary notificationDictionary = launchOptions [UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
			this.handleNotificationEvent(notificationDictionary);
		}

		private void handleNotificationEvent(NSDictionary notificationDictionary) {
			// check if notificationdictionary has a badge item.
//			if (notificationDictionary != nil)
//			{
//
//				// Application has been opened by tapping on the notification.
//				// So, we reset the badge count.
//				[self resetBadgeCount];
//
//				[self.kzApplication enableAnalytics];
//
//				KZDeviceInfo *info = [KZDeviceInfo sharedDeviceInfo];
//
//				NSMutableDictionary *attributes = [[NSMutableDictionary alloc] initWithDictionary:[info properties]];
//				if (notificationDictionary[KIDO_ID] != nil) {
//					attributes[KIDO_ID] = notificationDictionary[KIDO_ID];
//				}
//
//				//        [self.kzApplication openedFromNotification:notificationId];
//
		}
		public void DidRegisterUserNotificationSettings(UIUserNotificationSettings notificationSettings) {
			
		}



		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken) {
			
		}

		public void OnActivated() {
			
		}

		public void registerForRemoteNotifications() {
			UIUserNotificationSettings notificationSettings = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Badge | UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
			UIApplication.SharedApplication.RegisterUserNotificationSettings (notificationSettings);
		}

		private Task<User> initializeKidozen() {
			this.application = new Kidozen.KidoApplication(this.marketPlaceURL, this.applicationName, this.applicationKey);
			return this.application.Authenticate (this.user, this.password, this.provider);
		}
			
	}
}

