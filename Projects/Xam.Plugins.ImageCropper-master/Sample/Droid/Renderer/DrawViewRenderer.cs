using System;
using System.IO;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Com.Theartofdev.Edmodo.Cropper;
using Xam.Plugins.ImageCropper.Sample;
using Xam.Plugins.ImageCropper.Sample.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrawView), typeof(DrawViewRenderer))]
namespace Xam.Plugins.ImageCropper.Sample.Droid.Renderer
{
    public class DrawViewRenderer : PageRenderer
    {
        Xamarin.Forms.Point p;
        DrawView page;
        Canvas canvas;
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            page = Element as DrawView;
            if (page != null)
            {
                var imageCopy = page.Image;
                Bitmap bitmap = BitmapFactory.DecodeByteArray(App.CroppedImage, 0, App.CroppedImage.Length);
                canvas = new Canvas(bitmap);
                AnalyzePredictions(page.rootObject);
                this.canvas.Save();
                App.CroppedImage = this.canvas.ToArray<byte>();
                page.DidDraw = true;
                page.Navigation.PopModalAsync();
            }
        }

        private void AnalyzePredictions(RootObject rootObject)
        {
            var width = page.Image.Width;
            var height = page.Image.Height;
            if (rootObject != null && rootObject.predictions.Count > 0)
            {
                foreach (var item in rootObject.predictions)
                {
                    if (item.probability > .15)
                    {
                        var left = item.boundingBox.left * width;
                        var top = item.boundingBox.top * height;
                        var widthX = item.boundingBox.width * width;
                        var heightY = item.boundingBox.height * height;

                        //p = new Xamarin.Forms.Point(Convert.ToInt32(left), Convert.ToInt32(top));

                        var paint = new Paint();
                        paint.SetARGB(255, 200, 255, 0);
                        paint.SetStyle(Paint.Style.Stroke);
                        paint.StrokeWidth = 4;

                        //var _shape = new ShapeDrawable(new RectShape());
                        //_shape.Paint.Set(paint);
                        //_shape.SetBounds(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(widthX), Convert.ToInt32(heightY));

                        //var rectangle = new Rectangle(p, new Size(Convert.ToInt32(widthX), Convert.ToInt32(heightY)));
                        var r = new Rect(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(widthX), Convert.ToInt32(heightY));

                        this.canvas.DrawRect(r, paint);  
                    }
                }
            }
        }
    }
}