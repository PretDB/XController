using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace XController
{
    public class Target
    {
        public string string_TargetName { get; private set; }
        public Device device
        {
            get
            {
                return this._device;
            }

            set
            {
                switch (value)
                {
                    case Device.Car0:
                        this.string_TargetName = "Car 0";
                        break;
                    case Device.Car1:
                        this.string_TargetName = "Car 1";
                        break;
                    case Device.Marker:
                        this.string_TargetName = "Marker";
                        break;
                    default:
                        this.string_TargetName = "Unknown";
                        break;
                }
                this._device = value;
            }
        }

        private Device _device;
        public Target()
        {

        }
        public Target(Device dev)
        {
            this.device = dev;
        }
        public override string ToString()
        {
            return this.string_TargetName;
        }
    }


    public static class Data
    {
        public static ObservableCollection<Target> targets { get; private set; }

        static Data()
        {
            targets = new ObservableCollection<Target>()
            {
                new Target()
                {
                    device = Device.Car0
                },
                new Target()
                {
                    device = Device.Car1
                }
            };
        }
    }
}
