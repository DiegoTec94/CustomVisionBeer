using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace BeerAnalyzer
{
    public partial class Form1 : Form
    {
        public ToolTip buttonToolTip = new ToolTip();
        List<TooltipT> beerRecognizedList = new List<TooltipT>();
        public Point p;
        public int width;
        public int height;
        public string filePath;
        const string subscriptionKey = "c1b2e4480f114717a9251e090dc8bdba";
        const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/aa662c28-7757-46ec-81dc-342c2a52ac58/image";
        public Form1()
        {
            InitializeComponent();
            //this.WindowState = FormWindowState.Maximized;
            //this.MinimumSize = this.Size;
            //this.MaximumSize = this.Size;
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog Abrir = new OpenFileDialog();
                if (Abrir.ShowDialog() == DialogResult.OK)
                {
                    filePath = Abrir.FileName;
                    pcbOriginal.Image = Image.FromFile(Abrir.FileName);
                    pcbOriginal.SizeMode = PictureBoxSizeMode.StretchImage;
                    width  = pcbOriginal.Size.Width;
                    height = pcbOriginal.Size.Height;
                }
                else
                {
                    pcbOriginal.Image = null;
                }
            }
            catch (Exception var)
            {
                MessageBox.Show("No eligio ninguna imagen");
            }
        }

        private async void analyzeBtn_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(filePath))
            {
                buttonToolTip = new ToolTip();
                var response = await MakeAnalysisRequest(filePath);
               RootObject rootObject = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(response);
                AnalyzePredictions(rootObject);
            }
        }

        private void AnalyzePredictions(RootObject rootObject)
        {
            if(rootObject != null && rootObject.predictions.Count > 0)
            {
                beerRecognizedList = new List<TooltipT>();
                //processedImage.Image = pcbOriginal.Image;
                //processedImage.SizeMode = PictureBoxSizeMode.StretchImage;
                int i = 0;
                foreach(var item in rootObject.predictions)
                {
                    if(item.probability > .15)
                    {
                        TooltipT t = new TooltipT();
                        var left = item.boundingBox.left * width;
                        var top = item.boundingBox.top * height;
                        var widthX = item.boundingBox.width * width;
                        var heightY = item.boundingBox.height * height;

                        p = new Point(Convert.ToInt32(left), Convert.ToInt32(top));

                        t.Point = p;
                        t.height = height;
                        t.width = width;
                        t.tagName = item.tagName;
                        t.percentage = (item.probability * 100).ToString();
                        Graphics g = pcbOriginal.CreateGraphics();
                        var rectangle = new Rectangle(p, new Size(Convert.ToInt32(widthX), Convert.ToInt32(heightY)));
                        g.DrawRectangle(Pens.Red, rectangle);
                        t.rectangle = rectangle;
                        beerRecognizedList.Add(t);
                    }
                }
            }
        }

        private async Task<string> MakeAnalysisRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Prediction-Key", "5f5b1a4d745a4a5fa9e220fc85e0ac11");
                string uri = uriBase;

                HttpResponseMessage response;
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                return contentString;
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return string.Empty;
            }
        }
        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        private void pcbOriginal_Click(object sender, EventArgs e)
        {
            if (beerRecognizedList != null && beerRecognizedList.Count > 0)
            {
                var pointClick = (System.Windows.Forms.MouseEventArgs)e;
                var beerRecognized = beerRecognizedList.Where(x => x.rectangle.Contains(pointClick.Location)).FirstOrDefault();
                if (beerRecognized != null)
                {
                    buttonToolTip = new ToolTip();

                    buttonToolTip.ToolTipTitle = beerRecognized.tagName;

                    buttonToolTip.UseFading = true;

                    buttonToolTip.UseAnimation = true;

                    buttonToolTip.IsBalloon = true;



                    buttonToolTip.ShowAlways = true;



                    buttonToolTip.AutoPopDelay = 1000;

                    buttonToolTip.InitialDelay = 500;

                    buttonToolTip.ReshowDelay = 500;



                    buttonToolTip.SetToolTip(pcbOriginal, beerRecognized.percentage + "%");
                }
                //foreach (var beer in beerRecognizedList)
                //{
                //    Rectangle rect = beer.rectangle;
                //    if (rect.Contains(xCoordinate, yCoordinate))
                //    {
                //        ToolTip toolTip1 = new ToolTip();

                //        // Set up the delays for the ToolTip.
                //        toolTip1.AutoPopDelay = 500;
                //        toolTip1.InitialDelay = 500;
                //        toolTip1.ReshowDelay = 500;
                //        // Force the ToolTip text to be displayed whether or not the form is active.
                //        toolTip1.ShowAlways = true;

                //        // Set up the ToolTip text for the Button and Checkbox.
                //        toolTip1.SetToolTip(this.pcbOriginal, beer.tagName);
                //        break;
                //    }
                //    //if (!rect.Intersects(e.ClipRectangle))
                //    //    continue;

                //    //var boundLeft = Convert.ToInt32(beer.Point.X - (beer.width / 2));
                //    //var boundRight = Convert.ToInt32(beer.Point.X + (beer.width / 2));
                //    //var boundTop = Convert.ToInt32(beer.Point.Y - (beer.height / 2));
                //    //var boundBottom = Convert.ToInt32(beer.Point.Y + (beer.height / 2));
                //    //if((xCoordinate >= boundLeft && xCoordinate <= boundRight) && ((yCoordinate >= boundTop && yCoordinate <= boundBottom)))
                //    //{
                //    //    ToolTip toolTip1 = new ToolTip();

                //    //    // Set up the delays for the ToolTip.
                //    //    toolTip1.AutoPopDelay = 500;
                //    //    toolTip1.InitialDelay = 500;
                //    //    toolTip1.ReshowDelay = 500;
                //    //    // Force the ToolTip text to be displayed whether or not the form is active.
                //    //    toolTip1.ShowAlways = true;

                //    //    // Set up the ToolTip text for the Button and Checkbox.
                //    //    toolTip1.SetToolTip(this.pcbOriginal, beer.tagName);
                //    //}   
                //}

            }
        }
    }

    public class BoundingBox
    {
        public double left { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class Prediction
    {
        public double probability { get; set; }
        public string tagId { get; set; }
        public string tagName { get; set; }
        public BoundingBox boundingBox { get; set; }
    }

    public class RootObject
    {
        public string id { get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public DateTime created { get; set; }
        public List<Prediction> predictions { get; set; }
    }

    public class TooltipT
    {
        public Point Point { get; set; }
        public string tagName { get; set; }
        public int width  { get; set; }
        public int height { get; set; }
        public Rectangle rectangle { get; set; }
        public string percentage { get; set; }
    }
}
