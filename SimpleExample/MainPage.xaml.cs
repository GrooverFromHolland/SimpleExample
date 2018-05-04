using System;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleExample
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainPage : Page
   {
      GattDeviceService service = null;
      GattCharacteristic charac = null;
      Guid MyService_GUID;
      Guid MYCharacteristic_GUID;
      readonly ulong BleAddress = 88565202216510;
      long deviceFoundMilis = 0, serviceFoundMilis = 0;
      long connectedMilis = 0, characteristicFoundMilis = 0;
      long WriteDescriptorMilis = 0;
      Stopwatch stopwatch;

      public MainPage()
      {
         this.InitializeComponent();
         stopwatch = new Stopwatch();
         MyService_GUID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
         MYCharacteristic_GUID = new Guid("{0000ffe1-0000-1000-8000-00805f9b34fb}");
         StartWatching();
      }

      private void StartWatching()
      {
         // Create Bluetooth Listener
         var watcher = new BluetoothLEAdvertisementWatcher
         {
            ScanningMode = BluetoothLEScanningMode.Passive
         };
         // Register callback for when we see an advertisements
         watcher.Received += OnAdvertisementReceivedAsync;
         stopwatch.Start();
         watcher.Start();
      }
      private async void OnAdvertisementReceivedAsync(BluetoothLEAdvertisementWatcher watcher,
                                                      BluetoothLEAdvertisementReceivedEventArgs eventArgs)
      {
         // Filter for specific Device
         if (eventArgs.BluetoothAddress == BleAddress)
         {
            watcher.Stop();            
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(BleAddress);
            if (device != null)
            {
               deviceFoundMilis = stopwatch.ElapsedMilliseconds;             
               Debug.WriteLine("Device found in " + deviceFoundMilis);
               var result = await device.GetGattServicesForUuidAsync(MyService_GUID);               
               if (result.Status == GattCommunicationStatus.Success)
               {
                  connectedMilis = stopwatch.ElapsedMilliseconds;
                  Debug.WriteLine("Connected in " + (connectedMilis - deviceFoundMilis));
                  var services = result.Services;
                  service = services[0];
                  if (service != null)
                  {
                     serviceFoundMilis = stopwatch.ElapsedMilliseconds;
                     Debug.WriteLine("Service found in " +
                        (serviceFoundMilis - connectedMilis ));
                     var charResult = await service.GetCharacteristicsForUuidAsync(MYCharacteristic_GUID);
                     if (charResult.Status == GattCommunicationStatus.Success)
                     {
                         charac = charResult.Characteristics[0];
                        if (charac != null)
                        {
                           characteristicFoundMilis = stopwatch.ElapsedMilliseconds;
                           Debug.WriteLine("Characteristic found in " +
                                          (characteristicFoundMilis - serviceFoundMilis));
                           try
                           {
                              var notifyResult = await charac.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                              if (notifyResult == GattCommunicationStatus.Success)
                              {
                                 // AddValueChangedHandler();
                                 WriteDescriptorMilis = stopwatch.ElapsedMilliseconds;
                                 Debug.WriteLine("Successfully registered; for notifications in " +
                                                (WriteDescriptorMilis - characteristicFoundMilis ));
                              }
                              else
                              {
                                 Debug.WriteLine($"Error registering for notifications: {result}");
                                 watcher.Start();
                              }
                           }
                           catch (UnauthorizedAccessException ex)
                           {                              
                              Debug.WriteLine(ex.Message);
                           }
                        }
                     }
                     else  Debug.WriteLine("No characteristics  found");                     
                  }
               }
               else Debug.WriteLine("No services found");               
            }
            else Debug.WriteLine("No device found");
         }
      }
   }
}


