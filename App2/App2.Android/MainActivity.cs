using System;
using System.Net;
using System.Net.Sockets;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;

namespace XController.Droid
{
    [Activity(Label = "XController", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            Instance = this;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            
        }
    }


    [Service(Exported = true, Name = "com.pretdb.XController.DeviceListener", IsolatedProcess = true)]
    public class DeviceListener : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}

