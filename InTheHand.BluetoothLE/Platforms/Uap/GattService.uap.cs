﻿//-----------------------------------------------------------------------
// <copyright file="GattService.windows.cs" company="In The Hand Ltd">
//   Copyright (c) 2018-20 In The Hand Ltd, All rights reserved.
//   This source code is licensed under the MIT License - see License.txt
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using WBluetooth = Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace InTheHand.Bluetooth
{
    partial class GattService
    {
        readonly WBluetooth.GattDeviceService _service;
        readonly bool _isPrimary;

        internal GattService(BluetoothDevice device, WBluetooth.GattDeviceService service, bool isPrimary) : this(device)
        {
            _service = service;
            _isPrimary = isPrimary;
        }

        public static implicit operator WBluetooth.GattDeviceService(GattService service)
        {
            return service._service;
        }

        async Task<GattCharacteristic> PlatformGetCharacteristic(BluetoothUuid characteristic)
        {
            var result = await _service.GetCharacteristicsForUuidAsync(characteristic, Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (result.Status == WBluetooth.GattCommunicationStatus.Success && result.Characteristics.Count > 0)
                return new GattCharacteristic(this, result.Characteristics[0]);

            return null;
        }

        async Task<IReadOnlyList<GattCharacteristic>> PlatformGetCharacteristics()
        {
            List<GattCharacteristic> characteristics = new List<GattCharacteristic>();

            var result = await _service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
            if(result.Status == WBluetooth.GattCommunicationStatus.Success)
            {
                foreach(var c in result.Characteristics)
                {
                    characteristics.Add(new GattCharacteristic(this, c));
                }
            }

            return characteristics.AsReadOnly();
        }

        private async Task<GattService> PlatformGetIncludedServiceAsync(BluetoothUuid service)
        {
            var servicesResult = await _service.GetIncludedServicesForUuidAsync(service);

            if(servicesResult.Status == WBluetooth.GattCommunicationStatus.Success)
            {
                return new GattService(Device, servicesResult.Services[0], false);
            }

            return null;
        }

        private async Task<IReadOnlyList<GattService>> PlatformGetIncludedServicesAsync()
        {
            List<GattService> services = new List<GattService>();

            var servicesResult = await _service.GetIncludedServicesAsync();

            if (servicesResult.Status == WBluetooth.GattCommunicationStatus.Success)
            {
                foreach(var includedService in servicesResult.Services)
                {
                    services.Add(new GattService(Device, includedService, false));
                }

                return services;
            }

            return null;
        }

        BluetoothUuid GetUuid()
        {
            return _service.Uuid;
        }

        bool GetIsPrimary()
        {
            return _isPrimary;
        }
    }
}
