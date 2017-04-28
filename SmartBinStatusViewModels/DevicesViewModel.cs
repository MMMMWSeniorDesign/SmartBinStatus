using SmartBinStatusModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace SmartBinStatus.ViewModels
{
    public class DevicesViewModel : ViewModelBase
    {
        private bool            _alerting, _showDeletedDevices, _showingLocations;
        private DeviceViewModel _selectedDevice;
        private ICommand        _addDevice, _hideLocations, _showLocations, _removeDevice;
        private string          _locations, _serialFilter, _statusFilter;                                          
        private Timer           _updateTimer;

        private ObservableCollection<DeviceViewModel> _devices;
        private ObservableCollection<string>          _deviceStatuses;

        private SmartBinDeploymentEntities _entities;

        public DevicesViewModel()
        {

            _updateTimer          = new Timer(10000);
            _updateTimer.Elapsed += _updateTimer_Elapsed;
            _updateTimer.Start();

            SerialFilter     = "";
            ShowingLocations = false;
            StatusFilter     = "";
        }

        private  void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
             Update();
        }

        private void Update()
        {
            OnPropertyChanged("Devices");

            Locations = "";

            var unreviewedFullBins = 
                 Entities.Devices.SelectMany(d => d.DeviceStatuses
                              .Where(s => s.Device.Reviewed == false && 
                                          s.Status == (short)DeviceViewModel.DeviceStatusCodes.Full))
                              .ToList();

            if (unreviewedFullBins.Count > 0)
            {
                Alerting = true;
                unreviewedFullBins.Select(d => d.Device.Latitude + ", " + d.Device.Longitude).ToList()
                                  .ForEach(d => Locations += d);
            }
            else
                Alerting = false;
        }

        private bool CanAddDevice(object parameter)
        {
            return Entities != null;
        }

        private bool CanDeleteDevice(object parameter)
        {
            return SelectedDevice != null;
        }

        private bool CanHideLocations(object parameter)
        {
            return ShowingLocations;
        }

        private bool CanShowLocations(object parameter)
        {
            return true;
        }

        private List<DeviceViewModel> GetVMCollection()
        {
            _entities = null;

            if (!string.IsNullOrWhiteSpace(SerialFilter) && string.IsNullOrWhiteSpace(StatusFilter))
                return Entities.Devices.ToList()
                               .Where(d => d.Serial.Contains(SerialFilter))
                               .Select(d => new DeviceViewModel(d))
                               .ToList();
            else if (!string.IsNullOrWhiteSpace(StatusFilter) && string.IsNullOrWhiteSpace(SerialFilter))
                return Entities.Devices.ToList()
                               .Where(d => d.DeviceStatuses.Count() > 0 && 
                                           ((DeviceViewModel.DeviceStatusCodes)d.DeviceStatuses
                                                            .OrderByDescending(s => s.Timestamp).Take(1)
                                                            .Single().Status).ToString() == StatusFilter)
                               .Select(d => new DeviceViewModel(d)).ToList();
            else if (!string.IsNullOrWhiteSpace(StatusFilter) && !string.IsNullOrWhiteSpace(SerialFilter))
                return Entities.Devices.ToList()
                               .Where(d => d.Serial.Contains(SerialFilter) && d.DeviceStatuses.Count() > 0 &&
                                           ((DeviceViewModel.DeviceStatusCodes)d.DeviceStatuses
                                                            .OrderByDescending(s => s.Timestamp).Take(1)
                                                            .Single().Status).ToString() == StatusFilter)
                               .Select(d => new DeviceViewModel(d)).ToList();
            else
                return Entities.Devices.AsParallel().ToList().Select(d => new DeviceViewModel(d)).ToList();
        }

        private SmartBinDeploymentEntities Entities
        {
            get
            {
                if (_entities == null)
                    _entities = new SmartBinDeploymentEntities();

                return _entities;
            }
        }

        private void AddDevice(object parameter)
        {
            Device d = new Device()
            {
                DeviceStatuses = new List<DeviceStatus>(),
                IsDeleted      = false,
                Serial         = new Serial().ToString(),
                ReceivedDate   = DateTime.UtcNow
            };

            Entities.Devices.Add(d);
            Entities.SaveChanges();

            OnPropertyChanged("Devices");
        }

        private void DeleteDevice(object parameter)
        {
            SelectedDevice.IsDeleted = true;
            Entities.SaveChanges();

            OnPropertyChanged("Devices");
        }

        private void HideLocations(object parameter)
        {
            Alerting         = false;
            ShowingLocations = false;

            Devices.SelectMany
                (d => d.Device.DeviceStatuses
                       .Where(s => s.Device.Reviewed == false && 
                                   s.Status == (short)DeviceViewModel.DeviceStatusCodes.Full))
                       .ToList().ForEach(d => d.Device.Reviewed = true);

            Entities.SaveChanges();
        }

        private void ShowLocations(object parameter)
        {
            ShowingLocations = true;
        }

        public bool Alerting
        {
            get { return _alerting; }
            set
            {
                if (_alerting != value)
                {
                    _alerting = value;
                    OnPropertyChanged("Alerting");
                    CanShowLocations(null);
                }
            }
        }

        public bool NotShowingLocations
        {
            get { return !ShowingLocations; }
        }

        public bool ShowDeletedDevices
        {
            get { return _showDeletedDevices; }
            set
            {
                if (_showDeletedDevices != value)
                {
                    _showDeletedDevices = value;
                    OnPropertyChanged("ShowDeletedDevices");
                    OnPropertyChanged("Devices");
                }
            }
        }

        public bool ShowDeploymentDate
        {
            get { return SelectedDevice != null && SelectedDevice.DeploymentDate != null; }
        }

        public bool ShowingLocations
        {
            get { return _showingLocations; }
            set
            {
                if (_showingLocations != value)
                {
                    _showingLocations = value;
                    OnPropertyChanged("ShowingLocations");
                    OnPropertyChanged("NotShowingLocations");
                }
            }
        }

        public bool ShowIntersection
        {
            get { return SelectedDevice != null && ShowDeploymentDate && 
                         SelectedDevice.NearestIntersection != " + "; }
        }

        public ObservableCollection<DeviceViewModel> Devices
        {
            get
            {
                if (ShowDeletedDevices)
                    _devices = new ObservableCollection<DeviceViewModel>(GetVMCollection());
                else
                    _devices = new ObservableCollection<DeviceViewModel>
                                      (GetVMCollection().Where(d => !d.IsDeleted));

                return _devices;
            }
        }

        public ObservableCollection<string> DeviceStatuses
        {
            get
            {
                if (_deviceStatuses == null)
                {
                    _deviceStatuses = new ObservableCollection<string>();
                    _deviceStatuses.Add("");
                    _deviceStatuses.Add("Full");
                    _deviceStatuses.Add("NotFull");
                }

                return _deviceStatuses;
            }
        }

        public DeviceViewModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged("SelectedDevice");
                    RemoveDeviceCommand.CanExecute(null);
                }
            }
        }

        public ICommand AddDeviceCommand
        {
            get
            {
                if (_addDevice == null)
                    _addDevice = new DelegateCommand(AddDevice, CanAddDevice);

                return _addDevice;
            }
            
        }

        public ICommand RemoveDeviceCommand
        {
            get
            {
                if (_removeDevice == null)
                    _removeDevice = new DelegateCommand(DeleteDevice, CanDeleteDevice);

                return _removeDevice;
            }
            
        }

        public ICommand HideLocationsCommand
        {
            get
            {
                if (_hideLocations == null)
                    _hideLocations = new DelegateCommand(HideLocations, CanHideLocations);

                return _hideLocations;
            }
        }

        public ICommand ShowLocationsCommand
        {
            get
            {
                if (_showLocations == null)
                    _showLocations = new DelegateCommand(ShowLocations, CanShowLocations);

                return _showLocations;
            }
        }

        public string SerialFilter
        {
            get { return _serialFilter; }
            set
            {
                if (_serialFilter != value)
                {
                    _serialFilter = value;
                    OnPropertyChanged("SerialFilter");
                    OnPropertyChanged("Devices");
                }
            }
        }

        public string Locations
        {
            get { return _locations; }
            set
            {
                if (_locations != value)
                {
                    _locations = value;
                    OnPropertyChanged("Locations");
                }
            }
        }

        public string StatusFilter
        {
            get { return _statusFilter; }
            set
            {
                if (_statusFilter != value)
                {
                    _statusFilter = value;
                    OnPropertyChanged("StatusFilter");
                    OnPropertyChanged("Devices");
                }
            }
        }
    }
}