using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace XController
{
    public class Target
    {
        public string string_TargetName { get; private set; }
        public enum_Device device
        {
            get
            {
                return this._device;
            }

            set
            {
                switch (value)
                {
                    case enum_Device.Car0:
                        this.string_TargetName = "Car 0";
                        break;
                    case enum_Device.Car1:
                        this.string_TargetName = "Car 1";
                        break;
                    case enum_Device.Marker:
                        this.string_TargetName = "Marker";
                        break;
                    default:
                        this.string_TargetName = "Unknown";
                        break;
                }
                this._device = value;
            }
        }

        private enum_Device _device;
        public Target()
        {

        }
        public Target(enum_Device dev)
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
                    device = enum_Device.Car0
                },
                new Target()
                {
                    device = enum_Device.Car1
                }
            };
        }
    }
}
