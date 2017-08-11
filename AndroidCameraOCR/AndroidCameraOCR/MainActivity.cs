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
using Android.Support.V7.App;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Util;
using Android.Graphics;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using static Android.Gms.Vision.Detector;

namespace AndroidCameraOCR
{
    [Activity(Label = "AndroidCameraOCR", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback, IProcessor
    {
        private SurfaceView scanArea;
        private TextView txtViewScan;
        private CameraSource scanSource;
        private const int RequestCameraPermissionId = 1001;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCameraPermissionId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            scanSource.Start(scanArea.Holder);
                        }
                    }
                    break;
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            scanArea = FindViewById<SurfaceView>(Resource.Id.surfaceView);
            txtViewScan = FindViewById<TextView>(Resource.Id.txtView);

            TextRecognizer ocrApp = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!ocrApp.IsOperational)
            {
                Log.Error("Main Activity", "Detector depedencies are not yet available");
            }
            else
            {
                scanSource = new CameraSource.Builder(ApplicationContext, ocrApp)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();

                scanArea.Holder.AddCallback(this);
                ocrApp.SetProcessor(this);
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {

        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                //request Permission
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera
                }, RequestCameraPermissionId);
                return;
            }
            scanSource.Start(scanArea.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            scanSource.Stop();
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if (items.Size() != 0)
            {
                txtViewScan.Post(() => {
                    StringBuilder strBuilder = new StringBuilder();
                    for (int i = 0; i < items.Size(); ++i)
                    {
                        strBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                        strBuilder.Append("\n");
                    }
                    txtViewScan.Text = strBuilder.ToString();
                });
            }
        }

        public void Release()
        {

        }
    }
}