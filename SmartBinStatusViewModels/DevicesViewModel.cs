using SmartBinStatusModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SmartBinStatus.ViewModels
{
    public class DevicesViewModel : ViewModelBase
    {
        private DeviceViewModel _selectedDevice;
        private ICommand        _addDevice, _removeDevice;
        private string          _serialFilter, _statusFilter;                                                
        
        private ObservableCollection<DeviceViewModel> _devices;
        private ObservableCollection<string>          _deviceStatuses;

        private SmartBinDeploymentEntities _entities;

        public DevicesViewModel()
        {
            SerialFilter = "";
            StatusFilter = "";
        }

        private bool CanAddDevice(object parameter)
        {
            return Entities != null;
        }

        private System.Collections.Generic.List<DeviceViewModel> GetVMCollection()
        {
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
                return Entities.Devices.ToList().Select(d => new DeviceViewModel(d)).ToList();
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
            Device d = new Device();
            d.Serial = new Serial().ToString();
            d.ReceivedDate = DateTime.Now;
            d.IsDeleted = false;
            d.DeviceStatuses = new System.Collections.Generic.List<DeviceStatus>();

            Entities.Devices.Add(d);
            Entities.SaveChanges();

            OnPropertyChanged("Devices");
        }

        public ObservableCollection<DeviceViewModel> Devices
        {
            get
            {
                _devices = new ObservableCollection<DeviceViewModel>(GetVMCollection());

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