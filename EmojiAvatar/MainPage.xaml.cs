using EmojiAvatar.DataModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmojiAvatar {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 500));
            var tb = ApplicationView.GetForCurrentView().TitleBar;
            tb.ButtonBackgroundColor = Colors.Transparent;
            tb.ButtonInactiveBackgroundColor = Colors.Transparent;

            CoreApplication.GetCurrentView().CoreWindow.ResizeCompleted += (a, b) => {
                if (a.Bounds.Height < 532)
                    ApplicationView.GetForCurrentView().TryResizeView(new Size(a.Bounds.Width, 532));

                // try to fix split view bug after resize.
                if (splitView.DisplayMode == SplitViewDisplayMode.Inline) splitView.IsPaneOpen = true;
            };

            if (GetOSBuild() >= 22000) {
                BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
                Background = null;
            }
        }

        AvatarCreatorViewModel ViewModel => DataContext as AvatarCreatorViewModel;

        // In Laney, remove this and use app's function!
        public static ulong GetOSBuild() {
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            return (v & 0x00000000FFFF0000L) >> 16;
        }

        private void OnRootSizeChanged(object sender, SizeChangedEventArgs e) {
            double width = e.NewSize.Width;
            if (width > 720) {
                RootGrid.Margin = new Thickness(0, 32, 0, 32);
                RootGrid.BorderThickness = new Thickness(1);
                RootGrid.CornerRadius = new CornerRadius(8);
                panePivot.Margin = new Thickness(0);
            } else {
                RootGrid.Margin = new Thickness(0);
                RootGrid.BorderThickness = new Thickness(0);
                RootGrid.CornerRadius = new CornerRadius(0);
                panePivot.Margin = new Thickness(0, 32, 0, 0);
            }

            if (width >= 602) {
                splitView.PaneBackground = new SolidColorBrush(Colors.Transparent);
                splitView.DisplayMode = SplitViewDisplayMode.Inline;
                splitView.IsPaneOpen = true;
                splitView.OpenPaneLength = RootGrid.ActualWidth - 320;
                Grid.SetColumnSpan(randomBtn, 2);
                chooseBtn.Visibility = Visibility.Collapsed;
                paneCloseBtn.Visibility = Visibility.Collapsed;
                paneSeparator.Visibility = Visibility.Visible;
            } else {
                splitView.PaneBackground = Resources["paneBackground"] as Microsoft.UI.Xaml.Media.AcrylicBrush;
                splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                splitView.IsPaneOpen = false;
                splitView.OpenPaneLength = 320;
                Grid.SetColumnSpan(randomBtn, 1);
                chooseBtn.Visibility = Visibility.Visible;
                paneCloseBtn.Visibility = Visibility.Visible;
                paneSeparator.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenRightPanel(object sender, RoutedEventArgs e) {
            splitView.IsPaneOpen = true;
        }

        private void CloseRightPanel(object sender, RoutedEventArgs e) {
            splitView.IsPaneOpen = false;
        }

        private void ListViewItemClicked(object sender, ItemClickEventArgs e) {
            if (splitView.DisplayMode == SplitViewDisplayMode.Overlay) splitView.IsPaneOpen = false;
        }

        //

        private void Setup(object sender, RoutedEventArgs e) {
            DataContext = new AvatarCreatorViewModel();
            ViewModel.Setup();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(AvatarCreatorViewModel.GradientDirection):
                    CheckGradientDirection();
                    break;
            }
        }

        private void CheckGradientDirection() {
            switch (ViewModel.GradientDirection) {
                case GradientDirection.TopLeftToBottomRight:
                    directionLTRBtn.IsChecked = true;
                    directionTTBBtn.IsChecked = false;
                    directionRTLBtn.IsChecked = false;
                    gradientBrush.StartPoint = new Point(0, 0);
                    gradientBrush.EndPoint = new Point(1, 1);
                    break;
                case GradientDirection.TopToBottom:
                    directionLTRBtn.IsChecked = false;
                    directionTTBBtn.IsChecked = true;
                    directionRTLBtn.IsChecked = false;
                    gradientBrush.StartPoint = new Point(0, 0);
                    gradientBrush.EndPoint = new Point(0, 1);
                    break;
                case GradientDirection.TopRightToBottomLeft:
                    directionLTRBtn.IsChecked = false;
                    directionTTBBtn.IsChecked = false;
                    directionRTLBtn.IsChecked = true;
                    gradientBrush.StartPoint = new Point(1, 0);
                    gradientBrush.EndPoint = new Point(0, 1);
                    break;
            }
        }

        //

        private void ChangeGradientDirectionToLTR(object sender, RoutedEventArgs e) {
            ViewModel.GradientDirection = GradientDirection.TopLeftToBottomRight;
        }

        private void ChangeGradientDirectionToTTB(object sender, RoutedEventArgs e) {
            ViewModel.GradientDirection = GradientDirection.TopToBottom;
        }

        private void ChangeGradientDirectionToRTL(object sender, RoutedEventArgs e) {
            ViewModel.GradientDirection = GradientDirection.TopRightToBottomLeft;
        }

        private void SetGradient(object sender, RoutedEventArgs e) {
            GradientPreset preset = (sender as HyperlinkButton).Tag as GradientPreset;
            ViewModel.ApplyGradientPreset(preset);
        }

        #region AvatarCreatorItem rendering

        private void DrawEmojiInGVI(FrameworkElement sender, DataContextChangedEventArgs args) {
            Border container = sender as Border;
            if (args.NewValue != null && args.NewValue is Emoji emoji) {
                container.Child = emoji.Render(RenderMode.InGridViewItem);
            }
        }

        private void DrawEmojiInCanvas(object sender, SelectionChangedEventArgs e) {
            GridView gridView = sender as GridView;
            if (gridView.SelectedItem != null && gridView.SelectedItem is Emoji emoji) {
                workCanvas.Children.RemoveAt(workCanvas.Children.Count - 1);
                var element = emoji.Render(RenderMode.InCanvas);
                workCanvas.Children.Add(element);
            }
        }

        #endregion

        private void GenerateRandomAvatar(object sender, RoutedEventArgs e) {
            ViewModel.GenerateRandomAvatar();
        }

        private async void SaveAvatar(object sender, RoutedEventArgs e) {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(workCanvas);

            // Get pixels from RTB
            IBuffer pixelBuffer = await rtb.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // Support custom DPI
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, // RGB with alpha
                                 BitmapAlphaMode.Premultiplied,
                                 (uint)rtb.PixelWidth,
                                 (uint)rtb.PixelHeight,
                                 displayInformation.RawDpiX,
                                 displayInformation.RawDpiY,
                                 pixels);

            await encoder.FlushAsync(); // Write data to the stream
            stream.Seek(0); // Set cursor to the beginning

            FileSavePicker fsp = new FileSavePicker() {
                DefaultFileExtension = ".png",
                SuggestedFileName = $"avatar_{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            fsp.FileTypeChoices.Add("PNG", new List<string> { ".png" });
            StorageFile file = await fsp.PickSaveFileAsync();
            if (file != null) {
                using (var fstream = await file.OpenAsync(FileAccessMode.ReadWrite)) {
                    await RandomAccessStream.CopyAndCloseAsync(stream.GetInputStreamAt(0), fstream.GetOutputStreamAt(0));
                    await new ContentDialog { 
                        Title = "Successfully saved!",
                        Content = file.Path,
                        PrimaryButtonText = "OK"
                    }.ShowAsync();
                }
            }
        }
    }
}