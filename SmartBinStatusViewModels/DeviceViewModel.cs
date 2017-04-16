using SmartBinStatusModels;
using System;
using System.Linq;
using System.Windows.Input;

namespace SmartBinStatus.ViewModels
{
    public class DeviceViewModel : ViewModelBase
    {
        #region Private Fields
        private bool      _isDeleted;
        private decimal?  _latitude, _longitude;
        private DateTime  _receivedDate;
        private DateTime? _deploymentDate;
        private string    _nearestIntersectionFirst, _nearestIntersectionSecond, _serial, _status;
        #endregion

        public enum DeviceStatusCodes { NotFull, Full }

        public DeviceViewModel()
        {
            Serial = new Serial().ToString();
            
        }

        public DeviceViewModel(Device device)
        {
            DeploymentDate            = device.DeploymentDate;
            IsDeleted                 = device.IsDeleted;
            Latitude                  = device.Latitude;
            Longitude                 = device.Longitude;
            NearestIntersectionFirst  = device.NearestIntersectionFirst;
            NearestIntersectionSecond = device.NearestIntersectionSecond;
            ReceivedDate              = device.ReceivedDate;
            Serial                    = device.Serial;
            Status                    = GetDeviceStatus(device);
        }

        private string GetDeviceStatus(Device device)
        {
            DeviceStatus status;

            if ((status = device.DeviceStatuses.ToList()
                                .OrderByDescending(s => s.Timestamp).Take(1).SingleOrDefault()) == null)
                return "Device status unavailable.";

            if (status.Status == (int)DeviceStatusCodes.Full)
                return "Bin is full";
            else if (status.Status == (int)DeviceStatusCodes.NotFull)
                return "No attention required";
            else
                return "Device status unreported";
        }

        #region Public Properties
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    OnPropertyChanged("IsDeleted");
                }
            }
        }

        public DateTime? DeploymentDate
        {
            get { return _deploymentDate; }
            set
            {
                if (_deploymentDate != value)
                {
                    _deploymentDate = value;
                    OnPropertyChanged("DeploymentDate");
                }
            }
        }

        public DateTime ReceivedDate
        {
            get { return _receivedDate; }
            set
            {
                if (_receivedDate != value)
                {
                    _receivedDate = value;
                    OnPropertyChanged("ReceivedDate");
                }
            }
        }

        public decimal? Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    OnPropertyChanged("Latitude");
                }
            }
        }

        public decimal? Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    OnPropertyChanged("Longitude");
                }
            }
        }

        public string NearestIntersection
        {
            get { return NearestIntersectionFirst + " + " + NearestIntersectionSecond; }
        }

        public string NearestIntersectionFirst
        {
            get { return _nearestIntersectionFirst; }
            set
            {
                if (_nearestIntersectionFirst != value)
                {
                    _nearestIntersectionFirst = value;
                    OnPropertyChanged("NearestIntersectionFirst");
                    OnPropertyChanged("NearestIntersection");
                }
            }
        }

        public string NearestIntersectionSecond
        {
            get { return _nearestIntersectionSecond; }
            set
            {
                if (_nearestIntersectionSecond != value)
                {
                    _nearestIntersectionSecond = value;
                    OnPropertyChanged("NearestIntersectionSecond");
                    OnPropertyChanged("NearestIntersection");
                }
            }
        }

        public string Serial
        {
            get { return _serial; }
            set
            {
                if (_serial != value)
                {
                    _serial = value;
                    OnPropertyChanged("Serial");
                }
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        #endregion
    }
}