using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BeerMobileAnalyzer
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DrawingPage : ContentPage
	{
        RootObject rootObject;
        Image image;
        int skCanvasWidth;
        int skCanvasheight;

        public DrawingPage (RootObject rootObj, Image img)
		{
			InitializeComponent ();
            this.rootObject = rootObj;
            this.image = img;
		}

        private void SkCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var bitmap = SKBitmap.Decode(App.CroppedImage);
            // Init skcanvas
            SKImageInfo skImageInfo = e.Info;
            SKSurface skSurface = e.Surface;
            var im = SKImage.FromBitmap(bitmap);
            SKCanvas skCanvas = skSurface.Canvas;
            skCanvasWidth = skImageInfo.Width;
            skCanvasheight = skImageInfo.Height;
            var pictureFrame = SKRect.Create(0,0, skCanvasWidth, skCanvasheight);
            SKSize imageSize = new SKSize(im.Width, im.Height);
            var dest = pictureFrame.AspectFit(imageSize);

            // draw the image
            var paint = new SKPaint
            {
                FilterQuality = SKFilterQuality.High // high quality scaling
            };
            skCanvas.DrawImage(im, dest, paint);

            DrawAnalysis(skCanvas);
        }

        private void DrawAnalysis(SKCanvas skCanvas)
        {
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 5,
                IsAntialias = true,
            };
            if (rootObject != null && rootObject.predictions != null && rootObject.predictions.Count > 0)
            {
                foreach (var item in rootObject.predictions)
                {
                    if (item.probability > .15)
                    {
                        var left = item.boundingBox.left * skCanvasWidth;
                        var top = item.boundingBox.top * skCanvasheight;
                        var widthX = item.boundingBox.width * skCanvasWidth;
                        var heightY = item.boundingBox.height * skCanvasheight;

                        skCanvas.DrawRect(SKRect.Create((float)left, (float)top, (float)widthX, (float)heightY), skPaint);

                        // get an image subset to save
                        //var image = SKImage.FromBitmap(bitmap);
                        //var subset = image.Subset(SKRectI.Create(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(widthX), Convert.ToInt32(heightY)));

                        // encode the image
                        //var encodedData = subset.Encode(SKEncodedImageFormat.Png, 100);

                        // get a stream that can be saved to disk/memory/etc
                        //streamD= encodedData.AsStream();
                    }
                }

            }
            ////SKRect skRectangle = new SKRect();
            ////skRectangle.Size = new SKSize(100, 100);
            ////skRectangle.Location = new SKPoint(-100f / 2, -100f / 2);

            //var bitmap = SKBitmap.Decode(App.CroppedImage);
            //var point = new SKPoint(skCanvasWidth / 4, skCanvasheight / 4);
            ////var point = new SKPoint(0,0);
            //skCanvas.DrawBitmap(bitmap, point);

            //skCanvas.DrawRect(skRectangle, skPaint);
        }
    }
}