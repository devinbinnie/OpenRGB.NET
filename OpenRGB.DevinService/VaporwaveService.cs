using OpenRGB.NET;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OpenRGB.DevinService
{
    public partial class VaporwaveService : ServiceBase
    {
        public static int CurrentColor = 0;
        public static int CurrentValue = 0;
        private OpenRGBClient client;
        public static int deviceCount;
        public static Dictionary<int, Device> devices = new Dictionary<int, Device>();
        public static Random random = new Random();
        private Timer sequenceTimer;
        private Timer connectTimer;
        private EventLog log;

        public VaporwaveService()
        {
            InitializeComponent();

            log = new EventLog();
            log.Source = "VaporwaveService";
            log.Log = "Application";

            sequenceTimer = new Timer(10);
            sequenceTimer.Elapsed += new ElapsedEventHandler(ChangeLED);

            connectTimer = new Timer(2000);
            connectTimer.Elapsed += new ElapsedEventHandler(TryConnect);
        }

        protected override void OnStart(string[] args)
        {
            log.WriteEntry("Service started! Waiting for OpenRGB connection...");
            connectTimer.Start();
        }

        protected override void OnStop()
        {
            sequenceTimer.Stop();
            sequenceTimer.Dispose();
        }

        private void SetupSequence()
        {
            try
            {
                deviceCount = client.GetControllerCount();
                Device[] allDevices = client.GetAllControllerData();

                for (int i = 0; i < allDevices.Length; i++)
                {
                    if (allDevices[i].Name.Contains("ASUS Aura Motherboard"))
                    {
                        devices.Add(i, allDevices[i]);
                        client.SetMode(i, 0);
                    }
                }

                if (devices.Count <= 0)
                {
                    throw new Exception("Found no devices");
                }

                log.WriteEntry("Running sequence.");
                sequenceTimer.Start();
            }
            catch (Exception ex)
            {
                log.WriteEntry(string.Format("Setup failed with error message: {0}, retrying the connection...", ex.Message));
                CleanupClientAndRetry();
            }
        }

        private void TryConnect(object sender, ElapsedEventArgs e)
        {
            if (client != null && client.Connected)
            {
                log.WriteEntry("Connected! Setting up controller sequence.");
                connectTimer.Stop();
                SetupSequence();
                return;
            }

            try
            {
                client = new OpenRGBClient(name: "VaporwaveService");
            }
            catch (Exception) 
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }
        }

        private void ChangeLED(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!client.Connected)
                {
                    throw new Exception();
                }

                foreach (var device in devices)
                {
                    var leds = Enumerable.Range(0, device.Value.Colors.Length)
                        .Select((_, index) => Color.FromHsv((((index * 5) + CurrentColor) % 91) + 210, 1, (((index * 2) + CurrentValue) % 101) / 100d))
                        .ToArray();
                    client.UpdateLeds(device.Key, leds);

                }
                if (CurrentColor >= 90)
                {
                    CurrentColor += random.Next(0, 3);
                }
                else
                {
                    CurrentColor += random.Next(0, 2);
                }

                if (CurrentValue >= 100)
                {
                    CurrentValue = 0;
                }
                else
                {
                    CurrentValue++;
                }
            }
            catch (Exception)
            {
                log.WriteEntry("Connection lost, attempting reconnection");
                sequenceTimer.Stop();
                CleanupClientAndRetry();
            }
        }

        private void CleanupClientAndRetry()
        {
            if (client != null)
            {
                client.Dispose();
            }
            devices.Clear();
            connectTimer.Start();
        }
    }
}
