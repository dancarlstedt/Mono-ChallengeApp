using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Challenge.Core;

using Java.IO;

namespace Challenge.UI
{
    [Activity(Label = "Image Capture", MainLauncher = true, Icon = "@drawable/icon")]
    public class HomeScreen : Activity
    {
        private File _file;

        private const int ImageCaptureRequestCode = 1;

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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var captureImageButton = FindViewById<ImageButton>(Resource.Id.image_button);
            captureImageButton.Click += (x, y) =>
                {

                    //var dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), Guid.NewGuid().ToString());

                    //if (!dir.Exists())
                    //{
                    //    dir.Mkdirs();
                    //}

                    //_file = new Java.IO.File(dir, String.Format("myPhoto{0}.jpg", Guid.NewGuid()));

                    //var uri = Android.Net.Uri.FromFile(_file);
                    //Toast toad = Toast.MakeText(this, uri.ToString(), ToastLength.Long);
                    //toad.Show();

                    //var imageIntent = new Intent(MediaStore.ActionImageCapture);

                    //imageIntent.PutExtra(MediaStore.ExtraOutput, uri);
                    //imageIntent.AddFlags(ActivityFlags.SingleTop);
                    //StartActivityForResult(imageIntent, ImageCaptureRequestCode);
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
            var availableActivities = this.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);

            if (availableActivities != null && availableActivities.Count > 0)
            {
                var dir = new Java.IO.File(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");

                if (!dir.Exists())
                {
                    dir.Mkdirs();
                }

                _file = new Java.IO.File(dir, String.Format("myPhoto{0}.jpg", Guid.NewGuid()));

                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
                StartActivityForResult(intent, 0);
            }
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
            return new ImageCaptureEntity
            {
                Id = CustomerIdEditText.Text,
                Date = DateTime.Parse(DateButton.Text),
                Image = null
            };
        }

        private void OnDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            var dateButton = FindViewById<Button>(Resource.Id.datePicker);
            dateButton.Text = e.Date.ToShortDateString();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            //base.OnActivityResult(requestCode, resultCode, data);
            //if (requestCode == ImageCaptureRequestCode && resultCode == Result.Ok)
            //{
            //    var dataUri = _file.ToURI()
            //                       .ToString();
            //    ImageViewer.SetImageURI(Android.Net.Uri.Parse(dataUri));
            //}

            base.OnActivityResult(requestCode, resultCode, data);
            var imageView = ImageViewer;

            // make it available in the gallery
            var mediaScanIntent =
                new Intent(Intent.ActionMediaScannerScanFile);
            var contentUri = Android.Net.Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            this.SendBroadcast(mediaScanIntent);

            // display in ImageView
            var bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, contentUri);
            imageView.SetImageBitmap(bitmap);
            bitmap.Dispose();
        }
    }
}

