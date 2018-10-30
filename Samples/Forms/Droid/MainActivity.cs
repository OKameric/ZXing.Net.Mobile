using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;

namespace FormsSample.Droid
{
    [Activity (Label = "ZXing Forms", Icon = "@mipmap/ic_launcher", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        App formsApp;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            //CrossCurrentActivity.Current.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init (this, bundle);

            global::ZXing.Net.Mobile.Forms.Android.Platform.Init ();

            formsApp = new App ();
            LoadApplication (formsApp);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult (requestCode, permissions, grantResults);           
        }

        [Java.Interop.Export ("UITestBackdoorScan")]
        public Java.Lang.String UITestBackdoorScan (string param)
        {
            formsApp.UITestBackdoorScan (param);
            return new Java.Lang.String ();
        }
    }
}

