using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xam.Plugins.ImageCropper.Sample
{
    public class DrawView : ContentPage
    {
        public RootObject rootObject;
        public Action refresh;
        public Image Image;
        public Action RefreshAction;
        public bool DidDraw = false;

        public DrawView(RootObject rootObject, Action refresh, Image img)
        {
            this.rootObject = rootObject;
            this.refresh = refresh;
            this.Image = img;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (DidDraw)
                RefreshAction.Invoke();
        }
    }
}
