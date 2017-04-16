using System.Collections.Generic;
using System.Linq;

namespace SmartBinStatusModels
{
    public class Serial
    {
        private const int SERIAL_SIZE = 4;

        private char[] _value;

        public Serial()
        {
            Initialize(new SmartBinDeploymentEntities());
        }

        public Serial(string serial)
        {
            Initialize(serial);
        }

        private void Initialize(SmartBinDeploymentEntities entities)
        {
            List<Device> devices;

            if ((devices = entities.Devices.ToList()).Count == 0)
                Initialize("1");
            else
            {
                Initialize(devices.OrderByDescending(s => s.Serial).Take(1)
                                  .SingleOrDefault().Serial.ToString());
                CalculateSerial();
            }
        }

        private void Initialize(string serial)
        {
            if (serial.Length < 4)
            {
                _value = new char[SERIAL_SIZE];

                for (int i = 0; i < SERIAL_SIZE - serial.Length; ++i)
                    _value[i] = '0';

                for (int i = SERIAL_SIZE - serial.Length; i < SERIAL_SIZE; ++i)
                    _value[i] = serial[SERIAL_SIZE - serial.Length - i];
            }
            else if (serial.Length == SERIAL_SIZE)
                _value = serial.ToCharArray();
            else if (serial.Length > 4)
                _value = null;
        }

        private void CalculateSerial()
        {
            for (int i = SERIAL_SIZE - 1; i >= 0; --i)
            {
                if (i == SERIAL_SIZE - 1)
                    Value[i]++;

                if (Value[i] > '9' && Value[i] < 'A')
                    Value[i] = 'A';

                if (Value[i] > 'Z')
                {
                    if (i > 0)
                    {
                        Value[i] = '0';
                        Value[i - 1]++;
                    }
                    else
                        Value = null;
                }
            }
        }

        public static Serial operator ++(Serial s)
        {
            s.CalculateSerial();

            return s;
        }

        public override string ToString()
        {
            if (Value == null)
                return null;
            else
                return new string(Value);
        }

        public char[] Value
        {
            get { return _value; }
            private set
            {
                _value = value;
            }
        }
    }
}