using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.OS;

using Challenge.Core;

using Java.Nio;

using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Uri = Android.Net.Uri;

namespace Challenge.UI
{
    [Activity(Label = "Image Capture", MainLauncher = true, Icon = "@drawable/icon")]
    public class HomeScreen : Activity
    {
        private string _imagePath;

        private Bitmap _imageBitmap;

        private const int ImageCaptureRequestCode = 1;

        private const string ImagePathKey = "Image_URI";

        private Button DateButton
        {
            get
            {
                return FindViewById<Button>(Resource.Id.datePicker);
            }
        }

        private ImageView ImageViewer
        {
            get
            {
                return FindViewById<ImageView>(Resource.Id.image_viewer);
            }
        }

        private EditText CustomerIdEditText
        {
            get
            {
                return FindViewById<EditText>(Resource.Id.customer_id);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (bundle != null && bundle.ContainsKey(ImagePathKey))
            {
                _imagePath = bundle.GetString(ImagePathKey);
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var captureImageButton = FindViewById<ImageButton>(Resource.Id.image_button);
            captureImageButton.Click += (x, y) =>
                {
                    CaptureImageButton();
                };

            DateButton.Text = DateTime.Today.ToShortDateString();
            DateButton.Click += (x, y) =>
                {
                    var today = DateTime.Today.Date;
                    // Create a new instance of DatePickerDialog and return it
                    var dialog = new DatePickerDialog(this, OnDatePicked, today.Year, today.Month, today.Day);
                    dialog.Show();
                };

            var submitButton = FindViewById<Button>(Resource.Id.submit_button);
            submitButton.Click += (x, y) =>
            {
                var progress = ProgressDialog.Show(this, "Processing", "Please Wait...", false);
                Task.Factory.StartNew(
                    () =>
                    {
                        var answers = new CustomerComponent().Submit(GetCaptureEntity());
                        return answers;
                    })
                    .ContinueWith(result => OnCompletedAnsycStuff(result, progress));
                progress.Show();
            };
        }

        private void CaptureImageButton()
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            var availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);

            if (availableActivities != null && availableActivities.Count > 0)
            {
                var dir = new Java.IO.File(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");

                if (!dir.Exists())
                {
                    dir.Mkdirs();
                }

                var file = new Java.IO.File(dir, String.Format("myPhoto{0}.jpg", Guid.NewGuid()));
                var uri = Android.Net.Uri.FromFile(file);
                _imagePath = file.AbsolutePath;
                intent.PutExtra(MediaStore.ExtraOutput, uri);
                StartActivityForResult(intent, ImageCaptureRequestCode);


            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(ImagePathKey, _imagePath);
        }

        private void OnCompletedAnsycStuff(Task<string[]> task, ProgressDialog dialog)
        {
            RunOnUiThread(
                () =>
                {
                    dialog.Hide();
                    if (task.IsFaulted)
                    {
                        var errorDialog = new AlertDialog.Builder(this);
                        errorDialog.SetTitle("Error");
                        errorDialog.SetMessage(task.Exception.InnerExceptions.First().Message);
                        errorDialog.Create()
                                   .Show();
                    }
                    else if (task.IsCompleted)
                    {
                        var formattedAnswers = string.Format(
                            "Question: {0}{1}Answer: {2}", task.Result[0], System.Environment.NewLine, task.Result[1]);
                        var answerDialog = new AlertDialog.Builder(this);
                        answerDialog.SetTitle("Q & A");
                        answerDialog.SetMessage(formattedAnswers);
                        answerDialog.Create()
                                    .Show();
                    }
                });
        }

        private ImageCaptureEntity GetCaptureEntity()
        {
            byte[] imageBytes = null;
            if (_imageBitmap != null)
            {

                using (var stream = new MemoryStream())
                {
                    _imageBitmap.Compress(
                        Bitmap.CompressFormat.Png, 100, stream);
                    imageBytes = stream.ToArray();
                }
            }

            return new ImageCaptureEntity
            {
                Id = CustomerIdEditText.Text,
                Date = DateTime.Parse(DateButton.Text),
                Image = imageBytes
            };
        }

        private void OnDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            var dateButton = FindViewById<Button>(Resource.Id.datePicker);
            dateButton.Text = e.Date.ToShortDateString();
        }

        public void DecodeImage()
        {
            // Get the dimensions of the bitmap
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeFile(_imagePath, options);
            int photoW = options.OutWidth;
            int photoH = options.OutHeight;

            // Decode the image file into a Bitmap sized to fill the View
            options.InJustDecodeBounds = false;
            options.InSampleSize = 2;
            options.InPurgeable = true;

            _imageBitmap = BitmapFactory.DecodeFile(_imagePath, options);
            ImageViewer.SetImageBitmap(_imageBitmap);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == ImageCaptureRequestCode
                && resultCode == Result.Ok)
            {
                DecodeImage();
            }
        }
    }
}

