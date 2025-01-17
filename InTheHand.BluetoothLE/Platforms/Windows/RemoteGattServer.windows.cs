﻿//-----------------------------------------------------------------------
// <copyright file="BluetoothRemoteGATTServer.windows.cs" company="In The Hand Ltd">
//   Copyright (c) 2018-20 In The Hand Ltd, All rights reserved.
//   This source code is licensed under the MIT License - see License.txt
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InTheHand.Bluetooth
{
    partial class RemoteGattServer
    {
        private void PlatformInit()
        {
            Device.NativeDevice.ConnectionStatusChanged += NativeDevice_ConnectionStatusChanged;
        }

        private void NativeDevice_ConnectionStatusChanged(Windows.Devices.Bluetooth.BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Disconnected)
                Device.OnGattServerDisconnected();
        }

        bool GetConnected()
        {
            return Device.NativeDevice.ConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected;
        }

        async Task PlatformConnect()
        {
            var status = await Device.NativeDevice.RequestAccessAsync();
            if(status == Windows.Devices.Enumeration.DeviceAccessStatus.Allowed)
            {
                var session = await Windows.Devices.Bluetooth.GenericAttributeProfile.GattSession.FromDeviceIdAsync(Device.NativeDevice.BluetoothDeviceId);
                if(session != null)
                    session.MaintainConnection = true;

                // need to request something to force a connection
                await Device.NativeDevice.GetGattServicesAsync(cacheMode: Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);
            }
        }

        void PlatformDisconnect()
        {
            // Windows has no explicit disconnect 🤪
        }

        async Task<GattService> PlatformGetPrimaryService(BluetoothUuid service)
        {
            var result = await Device.NativeDevice.GetGattServicesForUuidAsync(service, Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (result == null || result.Services.Count == 0)
                return null;

            return new GattService(Device, result.Services[0], true);
        }

        async Task<List<GattService>> PlatformGetPrimaryServices(BluetoothUuid? service)
        {
            var services = new List<GattService>();
            var nativeDevice = Device.NativeDevice;
            Windows.Devices.Bluetooth.BluetoothCacheMode cacheMode = nativeDevice.DeviceInformation.Pairing.IsPaired ? Windows.Devices.Bluetooth.BluetoothCacheMode.Cached : Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached;
            Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceServicesResult result;
            if (service == null)
            {
                result = await nativeDevice.GetGattServicesAsync(cacheMode);
            }
            else
            {
                result = await nativeDevice.GetGattServicesForUuidAsync(service.Value, cacheMode);
            }

            if (result != null && result.Services.Count > 0)
            {
                foreach(var serv in result.Services)
                {
                    services.Add(new GattService(Device, serv, true));
                }
            }
            
            return services;
        }

        Task<short> PlatformReadRssi()
        {
            return Task.FromResult((short)0);
        }

        void PlatformSetPreferredPhy(BluetoothPhy phy)
        {
        }
    }
}
