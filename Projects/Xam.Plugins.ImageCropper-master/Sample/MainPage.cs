using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace Xam.Plugins.ImageCropper.Sample
{
    public class MainPage : ContentPage
    {
        public Point p;
        ImageSource _imageSource;
        private IMedia _mediaPicker;
        Image image;
        const string subscriptionKey = "c1b2e4480f114717a9251e090dc8bdba";
        const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/aa662c28-7757-46ec-81dc-342c2a52ac58/image";

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
                await Navigation.PushModalAsync(new DrawView(rootObject, Refresh, image));
            }
            catch (Exception e)
            {
                //Console.WriteLine("\n" + e.Message);
                //return string.Empty;
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


