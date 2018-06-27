using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace BeerMobileAnalyzer
{
	public partial class MainPage : ContentPage
	{
        public Point p;
        ImageSource _imageSource;
        private IMedia _mediaPicker;
        Image image;
        const string subscriptionKey = "c1b2e4480f114717a9251e090dc8bdba";
        const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/aa662c28-7757-46ec-81dc-342c2a52ac58/image";
        public RootObject buttonToolTip = new RootObject();
        public MainPage()
        {
            Title = "Crop View";
            NavigationPage.SetHasNavigationBar(this, true);
            ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Open",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async (x) =>
                {
                    var action = await DisplayActionSheet(null, "Cancel", null, "Photo Library", "Take Photo");
                    if (action == "Photo Library")
                        await SelectPicture();
                    else if (action == "Take Photo")
                        await TakePicture();
                    else if (action == "Cancel")
                        return;
                }),
            });

            ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Analyze",
                Order = ToolbarItemOrder.Secondary,
                Command = new Command(async (x) =>
                {
                    await AnalyzePicture();
                }),
            });

            image = new Image
            {
                Aspect = Aspect.AspectFit,
            };

            Label label = new Label
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Text = "Select an image to crop",
                TextColor = Color.Black
            };
            BackgroundColor = Color.White;
            Content = label;
        }

        private async Task AnalyzePicture()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Prediction-Key", "5f5b1a4d745a4a5fa9e220fc85e0ac11");
                string uri = uriBase;

                HttpResponseMessage response;
                byte[] byteData = App.CroppedImage;

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                var rootObject = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(contentString);
                //DrawView(rootObject, image);
                await Navigation.PushModalAsync(new DrawingPage(rootObject, image));
            }
            catch (Exception e)
            {
                var x = e.Message;
                //Console.WriteLine("\n" + e.Message);
                //return string.Empty;
            }
        }

        private void DrawView(RootObject rootObject, Image img)
        {
            Stream streamD = null;
            var width = img.Width;
            var height = img.Height;
            if (rootObject != null && rootObject.predictions.Count > 0)
            {
              //  beerRecognizedList = new List<RootObject>();
                //processedImage.Image = pcbOriginal.Image;
                //processedImage.SizeMode = PictureBoxSizeMode.StretchImage;
                int i = 0;
                foreach (var item in rootObject.predictions)
                {
                    if (item.probability > .50)
                    {
                        var bitmap = SKBitmap.Decode(App.CroppedImage);

                        // create a canvas for drawing
                        var canvas = new SKCanvas(bitmap);

                        // draw a rectangle with a red border
                        var paint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.Red,
                            StrokeWidth = 5
                        };

                        canvas.DrawRect(SKRect.Create(10, 10, 10, 10), paint);

                        // get an image subset to save
                        var im = SKImage.FromBitmap(bitmap);
                        var subset = im.Subset(SKRectI.Create(20, 20, 90, 90));

                        // encode the image
                        var encodedData = im.Encode(SKEncodedImageFormat.Png, 75);

                        // get a stream that can be saved to disk/memory/etc
                        var stream = encodedData.AsStream();

                        image.Source = ImageSource.FromStream(() => streamD);

                        Content = image;
                    }
                }

                    

                //foreach (var item in rootObject.predictions)
                //{
                //    if (item.probability > .50)
                //    {
                //        var left = item.boundingBox.left * width;
                //        var top = item.boundingBox.top * height;
                //        var widthX = item.boundingBox.width * width;
                //        var heightY = item.boundingBox.height * height;

                //        canvas.DrawRect(SKRect.Create((float)left, (float)top, (float)widthX, (float)heightY), paint);

                //        // get an image subset to save
                //        //var image = SKImage.FromBitmap(bitmap);
                //        //var subset = image.Subset(SKRectI.Create(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(widthX), Convert.ToInt32(heightY)));

                //        // encode the image
                //        //var encodedData = subset.Encode(SKEncodedImageFormat.Png, 100);

                //        // get a stream that can be saved to disk/memory/etc
                //        //streamD= encodedData.AsStream();
                //    }
                //}
                //image.Source = ImageSource.FromStream(() => streamD);

                //Content = image;
            }
        }

        private void Refresh()
        {
            try
            {
                if (App.CroppedImage != null)
                {
                    Stream stream = new MemoryStream(App.CroppedImage);
                    image.Source = ImageSource.FromStream(() => stream);

                    Content = image;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #region Photos

        private async void Setup()
        {
            if (_mediaPicker != null)
            {
                return;
            }

            ////RM: hack for working on windows phone? 
            await CrossMedia.Current.Initialize();
            _mediaPicker = CrossMedia.Current;
        }

        private async Task SelectPicture()
        {
            Setup();

            _imageSource = null;

            try
            {

                var mediaFile = await this._mediaPicker.PickPhotoAsync();

                _imageSource = ImageSource.FromStream(mediaFile.GetStream);

                var memoryStream = new MemoryStream();
                await mediaFile.GetStream().CopyToAsync(memoryStream);
                byte[] imageAsByte = memoryStream.ToArray();

                await Navigation.PushModalAsync(new CropView(imageAsByte, Refresh));

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task TakePicture()
        {
            Setup();

            _imageSource = null;

            try
            {
                var mediaFile = await this._mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    DefaultCamera = CameraDevice.Front
                });

                _imageSource = ImageSource.FromStream(mediaFile.GetStream);

                var memoryStream = new MemoryStream();
                await mediaFile.GetStream().CopyToAsync(memoryStream);
                byte[] imageAsByte = memoryStream.ToArray();

                await Navigation.PushModalAsync(new CropView(imageAsByte, Refresh));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
