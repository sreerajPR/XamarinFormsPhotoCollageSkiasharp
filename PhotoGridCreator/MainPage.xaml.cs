using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGridCreator.Helpers;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace PhotoGridCreator
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private string gridImageSource;
        public string GridImageSource
        {
            get
            {
                return gridImageSource;
            }
            set
            {
                gridImageSource = value;
                OnPropertyChanged();
                OnPropertyChanged("PhotosSelected");
            }
        }

        public bool PhotosSelected => !string.IsNullOrWhiteSpace(GridImageSource);

        private List<string> ImageSourceList { get; set; }

        public MainPage()
        {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            ImageSourceList = new List<string>();
            BindingContext = this;
        }

        private async Task<bool> CheckAndRequestPermissionAsync<TPermission>(string permissionName)
where TPermission : Permissions.BasePermission, new()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<TPermission>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<TPermission>();
                    if (status != PermissionStatus.Granted)
                    {
                        if (status == PermissionStatus.Denied)
                        {
                            await DisplayAlert("Permission Required", "Please enable " + permissionName +
                            " permission to carry on with this operation.", "OK");
                        }
                    }
                }
                if (status == PermissionStatus.Granted || status == PermissionStatus.Unknown)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        async void PickPhoto_Clicked(System.Object sender, System.EventArgs e)
        {
            if(ImageSourceList.Count < 9)
            {
                await CrossMedia.Current.Initialize();
                if (await CheckAndRequestPermissionAsync<Permissions.StorageRead>("Storage")
                && await CheckAndRequestPermissionAsync<Permissions.Photos>("Photos"))
                {
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await DisplayAlert("No Gallery Access", ":( No access to gallery available.", "OK");
                        return;
                    }
                    var files = await CrossMedia.Current.PickPhotosAsync(new PickMediaOptions { PhotoSize = PhotoSize.Medium });
                    if (files == null)
                    {
                        return;
                    }
                    foreach(MediaFile file in files)
                    {
                        if (ImageSourceList.Count < 9)
                            ImageSourceList.Add(file.Path);
                    }

                    if(ImageSourceList.Count>0)
                    {
                        GridImageSource = ImageMergerHelper.Combine(ImageSourceList);
                    }
                }
            }
            else
            {
                await DisplayAlert("","Can not add more than 9 images","OK");
            }
        }

        void ResetPhotos_Clicked(System.Object sender, System.EventArgs e)
        {
            ImageSourceList.Clear();
            GridImageSource = string.Empty;
        }

        async void SharePhoto_Clicked(System.Object sender, System.EventArgs e)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share the collaged image",
                File = new ShareFile(GridImageSource)
            });
        }
    }
}