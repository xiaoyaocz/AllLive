using AllLive.Avalonia.Helper;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace AllLive.Avalonia.Controls
{
    
    public partial class NetworkImage : UserControl
    {
        static Dictionary<string, Bitmap> ImagesCaches = new Dictionary<string, Bitmap>();
        public NetworkImage()
        {
            InitializeComponent();
        }

        static NetworkImage()
        {
            AffectsRender<NetworkImage>(SourceProperty);

        }
        public static readonly StyledProperty<string> SourceProperty =
          AvaloniaProperty.Register<NetworkImage, string>(nameof(Source), coerce: OnSourceChanged);
        private static string OnSourceChanged(IAvaloniaObject d, string e)
        {
            var img = d as NetworkImage;
            img.LoadImage(e);
            return e;
        }

        Image image;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            image = this.FindControl<Image>("image");
        }



        public string Source
        {
            get
            {
                return GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);

            }
        }

        private bool loading = false;

        public async void LoadImage(string source)
        {
            try
            {
                if (string.IsNullOrEmpty(source)|| loading) return;
                loading = true;
                image.Tag = "1";
                if (ImagesCaches.ContainsKey(source))
                {
                    image.Source = ImagesCaches[source];
                    return;
                }
                using (HttpClient http = new HttpClient())
                {
                    var imageStream = await http.GetByteArrayAsync(source);
                    using (MemoryStream memoryStream = new MemoryStream(imageStream))
                    {
                        var bitmap = new Bitmap(memoryStream);
                        ImagesCaches.Add(source, bitmap);
                        image.Source = bitmap;
                        //image.Stretch = Stretch.Fill;
                    }

                }
                loading = false;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Õº∆¨º”‘ÿ ß∞‹" + source);
                LogHelper.Log("Õº∆¨º”‘ÿ ß∞‹" + source, LogType.ERROR, ex);
                //throw;
            }
        }
    }
}
