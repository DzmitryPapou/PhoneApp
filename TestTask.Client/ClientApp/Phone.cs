using System;
using System.ComponentModel;
using System.IO;

namespace ClientApp
{
    public class Phone : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }

        private string _Name;
        public string Name
        {
            get => _Name;
            set { _Name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _Base64Image;
        public string Base64Image
        {
            get => _Base64Image;
            set { _Base64Image = value; OnPropertyChanged(nameof(Base64Image)); }
        }

        private string _ImageName;
        public string ImageName
        {
            get => Id != 0 && string.IsNullOrWhiteSpace(_ImageName) ? $"{Name.Replace(" ", "")}.img" : _ImageName;
            set { _ImageName = value; OnPropertyChanged(nameof(ImageName)); }
        }

        private string _ImagePath;
        public string ImagePath
        {
            get => _ImagePath;
            set
            {
                _ImagePath = value;
                ImageName = Path.GetFileName(value);
                Base64Image = Convert.ToBase64String(File.ReadAllBytes(value));
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set { _IsSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }
    }
}
