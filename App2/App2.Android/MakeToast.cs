using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XController.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(MakeToast))]
namespace XController.Droid
{
    public class MakeToast : IInfo
    {
        public void Toast(string text, bool isLong)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, text, isLong ? ToastLength.Long : ToastLength.Short).Show();
        }
    }
}