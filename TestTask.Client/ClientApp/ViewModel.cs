using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClientApp.Properties;
using Microsoft.Win32;
using RestSharp;

namespace ClientApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        static readonly string apiSource = "api/values";

        private readonly RestClient restClient;

        private RelayCommand _RefreshCommand;
        private RelayCommand _SelectAllPhonesCommand;
        private RelayCommand _UnselectAllPhonesCommand;
        private RelayCommand _AddPhoneCommand;
        private RelayCommand _ClosePhoneCommand;
        private RelayCommand _ChoosePhoneImageCommand;
        private RelayCommand _SavePhoneCommand;
        private RelayCommand _DeleteSelectedPhonesCommand;
        private RelayCommand _EditSelectedPhoneCommand;

        public ICommand RefreshCommand => _RefreshCommand ?? (_RefreshCommand = new RelayCommand(param => Get()));
        public ICommand SelectAllPhonesCommand => _SelectAllPhonesCommand ??
                                                  (_SelectAllPhonesCommand = new RelayCommand(param => SelectAllPhones()));
        public ICommand UnselectAllPhonesCommand => _UnselectAllPhonesCommand ??
                                                    (_UnselectAllPhonesCommand =
                                                        new RelayCommand(param => UnselectAllPhones()));
        public ICommand AddPhoneCommand =>
            _AddPhoneCommand ?? (_AddPhoneCommand = new RelayCommand(param => AddPhone()));
        public ICommand ClosePhoneCommand => _ClosePhoneCommand ??
                                             (_ClosePhoneCommand = new RelayCommand(param => ClosePhone()));
        public ICommand ChoosePhoneImageCommand => _ChoosePhoneImageCommand ??
                                                   (_ChoosePhoneImageCommand =
                                                       new RelayCommand(param => ChoosePhoneImage()));
        public ICommand SavePhoneCommand => _SavePhoneCommand ?? (_SavePhoneCommand = new RelayCommand(param => SavePhone()));
        public ICommand DeleteSelectedPhonesCommand => _DeleteSelectedPhonesCommand ?? (_DeleteSelectedPhonesCommand = new RelayCommand(param => DeleteSelectedPhones()));
        public ICommand EditSelectedPhoneCommand => _EditSelectedPhoneCommand ??
                                                    (_EditSelectedPhoneCommand =
                                                        new RelayCommand(param => EditSelectedPhone()));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Phone> _Phones;
        public ObservableCollection<Phone> Phones
        {
            get => _Phones;
            set { _Phones = value; OnPropertyChanged(nameof(Phones)); }
        }

        private Phone _SelectedPhone;
        public Phone SelectedPhone
        {
            get => _SelectedPhone;
            set
            {
                _SelectedPhone = value;
                SelectedPhoneError = null;
                OnPropertyChanged(nameof(SelectedPhone));
            }
        }

        private string _SelectedPhoneError;
        public string SelectedPhoneError
        {
            get => _SelectedPhoneError;
            set { _SelectedPhoneError = value; OnPropertyChanged(nameof(SelectedPhoneError)); }
        }

        private int? _SelectedSortIndex;
        public int? SelectedSortIndex
        {
            get => _SelectedSortIndex;
            set
            {
                _SelectedSortIndex = value;

                switch (value)
                {
                    case 0:
                        Phones = new ObservableCollection<Phone>(Phones.OrderBy(x => x.Name));
                        break;

                    case 1:
                        Phones = new ObservableCollection<Phone>(Phones.OrderByDescending(x => x.Name));
                        break;
                }

                OnPropertyChanged(nameof(SelectedSortIndex));
            }
        }

        public ViewModel()
        {
            restClient = new RestClient(Settings.Default.ApiUrl);
        }


        public void Get()
        {
            var request = new RestRequest(apiSource, Method.GET);
            var response = restClient.Execute<List<Phone>>(request);

            if (response.IsSuccessful)
            {
                Phones = new ObservableCollection<Phone>(response.Data);
                SelectedSortIndex = null;
            }
            else
            {
                MessageBox.Show(response.ErrorMessage ?? $"Server Error:\r\n{response.StatusDescription}", "Data not received", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public IRestResponse<Phone> Post(Phone phone)
        {
            var request = new RestRequest(apiSource, Method.POST);
            request.AddJsonBody(phone);
            return restClient.Execute<Phone>(request);
        }

        public IRestResponse<Phone> Put(Phone phone)
        {
            var request = new RestRequest(apiSource, Method.PUT);
            request.AddJsonBody(phone);
            return restClient.Execute<Phone>(request);
        }

        public IRestResponse Delete(IEnumerable<int> id)
        {
            var request = new RestRequest(apiSource, Method.DELETE);
            request.AddJsonBody(id);
            return restClient.Execute(request);
        }

        public void SelectAllPhones() => Phones.ForEach(x => x.IsSelected = true);

        public void UnselectAllPhones() => Phones.ForEach(x => x.IsSelected = false);

        public void AddPhone() => SelectedPhone = new Phone();

        public void ClosePhone() => SelectedPhone = null;

        public void ChoosePhoneImage()
        {
            var imageDialog = new OpenFileDialog
            {
                Filter = "Images (JPG, PNG)|*.JPG;*.PNG",
                CheckFileExists = true,
                Multiselect = false
            };

            if (imageDialog.ShowDialog() == true)
                SelectedPhone.ImagePath = imageDialog.FileName;
        }

        public void SavePhone()
        {
            SelectedPhoneError = null;

            if (string.IsNullOrWhiteSpace(SelectedPhone.Name))
            {
                SelectedPhoneError = "Please fill Phone name.";
                return;
            }

            if (SelectedPhone.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(SelectedPhone.ImagePath))
                {
                    SelectedPhoneError = "Please select Phone image.";
                    return;
                }

                var response = Post(SelectedPhone);

                if (response.IsSuccessful)
                {
                    MessageBox.Show("The Phone was succesfully saved. Refresh please!");
                    ClosePhone();
                }
                else
                {
                    SelectedPhoneError = response.ErrorMessage ?? $"Server Error:\r\n{response.StatusDescription}";
                }

            }
            else
            {
                if (SelectedPhone.Name == Phones.First(x => x.Id == SelectedPhone.Id).Name && SelectedPhone.ImagePath == null)
                {
                    ClosePhone();
                    return;
                }

                var response = Put(SelectedPhone);

                if (response.IsSuccessful)
                {
                    var Phone = Phones.First(x => x.Id == SelectedPhone.Id);

                    if (Phone.Name == response.Data.Name)
                        Phone.Base64Image = response.Data.Base64Image;
                    else
                    {
                        if (SelectedSortIndex == null)
                        {
                            Phone.Name = response.Data.Name;
                            Phone.Base64Image = response.Data.Base64Image;
                        }
                        else
                        {
                            Phones.Remove(Phone);

                            var index = Phones.ToList().BinarySearch(response.Data);
                            if (index < 0) index = ~index;
                            Phones.Insert(index, response.Data);
                        }
                    }

                    MessageBox.Show("The Phone was succesfully saved. Refresh please!");
                    ClosePhone();
                }
                else
                {
                    SelectedPhoneError = response.ErrorMessage ?? $"Server Error:\r\n{response.StatusDescription}";
                }

            }
        }

        public void DeleteSelectedPhones()
        {
            var selectedPhones = Phones.Where(x => x.IsSelected);

            if (!selectedPhones.Any())
            {
                MessageBox.Show("There are no selected Phones.\r\nPlease select at least one Phone to delete.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var response = Delete(selectedPhones.Select(x => x.Id));

            if (response.IsSuccessful)
                Phones.RemoveAll(x => x.IsSelected);
            else
            {
                MessageBox.Show(response.ErrorMessage ?? $"Server Error:\r\n{response.StatusDescription}", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void EditSelectedPhone()
        {
            var selectedPhones = Phones.Where(x => x.IsSelected);

            if (selectedPhones.Count() != 1)
            {
                MessageBox.Show("Please select only one Phone.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedPhone = new Phone { Id = selectedPhones.First().Id, Name = selectedPhones.First().Name, Base64Image = selectedPhones.First().Base64Image };
        }

    }
}
