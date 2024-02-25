using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using OpenRGB.NET;
using OpenRGB.NET.Models;

namespace DevinOpenRGB
{
    class Program
    {
        public static int CurrentColor = 0;
        public static OpenRGBClient client;
        public static int deviceCount;
        public static Dictionary<int, Device> devices = new Dictionary<int, Device>();

        static void Main(string[] args)
        {
            client = new OpenRGBClient(name: "My OpenRGB Client", autoconnect: true, timeout: 1000);

            deviceCount = client.GetControllerCount();
            Device[] allDevices = client.GetAllControllerData();

            for (int i = 0; i < allDevices.Length; i++)
            {
                if (allDevices[i].Name.Contains("ASUS"))
                {
                    client.SetMode(i, 0);
                    devices.Add(i, allDevices[i]);
                }
            }

            Timer t = new Timer(10);
            t.Elapsed += new ElapsedEventHandler(ChangeLED);
            t.Start();

            Console.ReadLine();
            t.Stop();
        }

        public static void ChangeLED(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<int, Device> device in devices)
            {
                var leds = Enumerable.Range(0, device.Value.Colors.Length)
                    .Select((device, index) => Color.FromHsv((((index * 5) + CurrentColor) % 121) + 180, 1, 1))
                    .ToArray();
                client.UpdateLeds(device.Key, leds);
            }
            if (CurrentColor >= 120)
            {
                CurrentColor = 0;
            }
            else
            {
                CurrentColor++;
            }
        }
    }
}
