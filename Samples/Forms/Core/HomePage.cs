using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace FormsSample
{
    public class HomePage : ContentPage
    {
        ZXingScannerPage scanPage;
        Button buttonScanDefaultOverlay;
        Button buttonScanCustomOverlay;
        Button buttonScanContinuously;
        Button buttonScanCustomPage;
        Button buttonGenerateBarcode;
        private Image image;
        public HomePage() : base()
        {
            buttonScanDefaultOverlay = new Button
            {
                Text = "Scan with Default Overlay",
                AutomationId = "scanWithDefaultOverlay",
            };
            buttonScanDefaultOverlay.Clicked += async delegate
            {
                scanPage = new ZXingScannerPage();
                scanPage.OnScanResult += (result) =>
                {
                    scanPage.IsScanning = false;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Navigation.PopAsync();
                        DisplayAlert("Scanned Barcode", result.Text, "OK");
                    });
                };

                await Navigation.PushAsync(scanPage);
            };


            buttonScanCustomOverlay = new Button
            {
                Text = "Scan with Custom Overlay",
                AutomationId = "scanWithCustomOverlay",
            };
            buttonScanCustomOverlay.Clicked += async delegate
            {
                // Create our custom overlay
                var customOverlay = new StackLayout
                {
                    Spacing = 0,
                    Padding = 10,
                    BackgroundColor = Color.Black,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Start
                };
                var fotoButton = new Button()
                {
                    Text = "Foto",
                    WidthRequest = 150,
                    HeightRequest = 40,
                    BorderRadius = 10,
                    TextColor = Color.White,
                    BackgroundColor = Color.Black,
                    BorderColor = Color.White
                };
                var QRCodeButton = new Button()
                {
                    WidthRequest = 150,
                    HeightRequest = 40,
                    Text = "QR Code",
                    BorderRadius = 10,
                    TextColor = Color.Black,
                    BackgroundColor = Color.White,
                    BorderColor = Color.White
                };
                var circleButton = new Button();
                var imageGrid = new Grid();
                fotoButton.Clicked += delegate
                {
                    fotoButton.TextColor = Color.Black;
                    fotoButton.BackgroundColor = Color.White;

                    QRCodeButton.TextColor = Color.White;
                    QRCodeButton.BackgroundColor = Color.Black;

                    imageGrid.IsVisible = true;
                    //scanPage.IsScanning = false;
                    //scanPage.IsAnalyzing = false;
                };

                QRCodeButton.Clicked += delegate
                {

                    QRCodeButton.TextColor = Color.Black;
                    QRCodeButton.BackgroundColor = Color.White;

                    fotoButton.TextColor = Color.White;
                    fotoButton.BackgroundColor = Color.Black;

                    imageGrid.IsVisible = false;
                    //scanPage.IsScanning = true;
                    //scanPage.IsAnalyzing = true;
                };

                var torch = new Button
                {
                    Text = "T",
                    WidthRequest = 40,
                    HeightRequest = 40,
                    BorderRadius = 10,
                    TextColor = Color.White,
                    BackgroundColor = Color.Black,
                };
                torch.Clicked += delegate
                {
                    scanPage.ToggleTorch();
                    if (scanPage.IsTorchOn)
                    {
                        torch.TextColor = Color.Black;
                        torch.BackgroundColor = Color.White;
                    }
                    else
                    {
                        torch.TextColor = Color.White;
                        torch.BackgroundColor = Color.Black;
                    }
                };

                customOverlay.Children.Add(fotoButton);
                customOverlay.Children.Add(QRCodeButton);
                customOverlay.Children.Add(torch);

                var rootGrid = new Grid();
                rootGrid.RowSpacing = 0;
                rootGrid.ColumnSpacing = 0;
                rootGrid.RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition(){Height = 60},
                    new RowDefinition(){Height = new GridLength(1, GridUnitType.Star)},
                    new RowDefinition(){Height = 80},
                };

                rootGrid.Children.Add(customOverlay, 0, 0);


                imageGrid.BackgroundColor = Color.Black;
                imageGrid.IsVisible = false;
                imageGrid.RowSpacing = 0;
                imageGrid.ColumnSpacing = 0;
                imageGrid.ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(){Width = 60},
                    new ColumnDefinition(){Width = new GridLength(1, GridUnitType.Star)},
                };
                imageGrid.RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition(){Height = GridLength.Auto},
                    new RowDefinition(){Height = GridLength.Auto}
                };

                image = new Image();
                image.Aspect = Aspect.AspectFill;
                image.HorizontalOptions = LayoutOptions.Center;
                image.VerticalOptions = LayoutOptions.Center;
                //image.Source = new UriImageSource() { Uri = new Uri("https://cloud.netlifyusercontent.com/assets/344dbf88-fdf9-42bb-adb4-46f01eedd629/242ce817-97a3-48fe-9acd-b1bf97930b01/09-posterization-opt.jpg") };
                image.WidthRequest = 200;
                image.HeightRequest = 200;

                circleButton.HorizontalOptions = LayoutOptions.Center;
                circleButton.VerticalOptions = LayoutOptions.Center;
                circleButton.WidthRequest = 60;
                circleButton.HeightRequest = 60;
                circleButton.BorderRadius = 30;
                circleButton.BorderColor = Color.Black;
                circleButton.BackgroundColor = Color.White;

                var fotoLabel = new Label();
                fotoLabel.Text = "FOTO";
                fotoLabel.TextColor = Color.FromRgb(255, 197, 45);
                fotoLabel.FontSize = 14;
                fotoLabel.HorizontalOptions = LayoutOptions.Center;
                fotoLabel.HorizontalTextAlignment = TextAlignment.Center;

                imageGrid.Children.Add(fotoLabel, 0, 0);
                Grid.SetColumnSpan(fotoLabel, 2);

                //imageGrid.Children.Add(image, 0, 1);
                imageGrid.Children.Add(circleButton, 0, 1);
                Grid.SetColumnSpan(circleButton, 2);

                circleButton.Command = new Command(() =>
                {
                    scanPage.GetPicture();
                });

                rootGrid.Children.Add(imageGrid, 0, 2);
                rootGrid.Children.Add(image, 0, 1);


                scanPage = new ZXingScannerPage(new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    AutoRotate = true,
                    CameraResolutionSelector = resolutions => resolutions.First(),
                }, customOverlay: rootGrid);
                scanPage.GotBytes += GotBytes;
                NavigationPage.SetHasNavigationBar(scanPage, false);
                scanPage.OnScanResult += (result) =>
                {
                    scanPage.IsScanning = false;


                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Navigation.PopAsync();
                        DisplayAlert("Scanned Barcode", result.Text, "OK");
                    });
                };
                await Navigation.PushAsync(scanPage);
            };


            buttonScanContinuously = new Button
            {
                Text = "Scan Continuously",
                AutomationId = "scanContinuously",
            };
            buttonScanContinuously.Clicked += async delegate
            {
                scanPage = new ZXingScannerPage(new ZXing.Mobile.MobileBarcodeScanningOptions
                {
                    DelayBetweenContinuousScans = 300
                });
                scanPage.OnScanResult += (result) =>
                    Device.BeginInvokeOnMainThread(() =>
                       DisplayAlert("Scanned Barcode", result.Text, "OK"));
                await Navigation.PushAsync(scanPage);
            };

            buttonScanCustomPage = new Button
            {
                Text = "Scan with Custom Page",
                AutomationId = "scanWithCustomPage",
            };
            buttonScanCustomPage.Clicked += async delegate
            {
                var customScanPage = new CustomScanPage();
                await Navigation.PushAsync(customScanPage);
            };


            buttonGenerateBarcode = new Button
            {
                Text = "Barcode Generator",
                AutomationId = "barcodeGenerator",
            };
            buttonGenerateBarcode.Clicked += async delegate
            {
                await Navigation.PushAsync(new BarcodePage());
            };

            var stack = new StackLayout();
            stack.Children.Add(buttonScanDefaultOverlay);
            stack.Children.Add(buttonScanCustomOverlay);
            stack.Children.Add(buttonScanContinuously);
            stack.Children.Add(buttonScanCustomPage);
            stack.Children.Add(buttonGenerateBarcode);

            Content = stack;
        }

        private void GotBytes(object sender, byte[] e)
        {
            if (e != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    image.Source = ImageSource.FromStream(() => new MemoryStream(e));
                });

            }
        }
    }
}
