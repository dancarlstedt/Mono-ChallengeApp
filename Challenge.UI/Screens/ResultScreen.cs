using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Provider;
using Android.Widget;
using Android.OS;

using Challenge.Core;

using JavaFile = Java.IO.File;
using AndroidUri = Android.Net.Uri;

namespace Challenge.UI.Screens
{
    [Activity(Label = "Image Submission Result", MainLauncher = false, Icon = "@drawable/icon")]
    public class ResultScreen : Activity
    {
        internal static string QuestionKey = "Question";

        internal static string AnswerKey = "Answer";

        private TextView Question
        {
            get
            {
                return FindViewById<TextView>(Resource.Id.question);
            }
        }

        private TextView Answer
        {
            get
            {
                return FindViewById<TextView>(Resource.Id.answer);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Result);

            if (Intent.HasExtra(QuestionKey))
            {
                Question.Text = Intent.GetStringExtra(QuestionKey);
            }

            if (Intent.HasExtra(AnswerKey))
            {
                Answer.Text = Intent.GetStringExtra(AnswerKey);
            }
        }
    }
}

