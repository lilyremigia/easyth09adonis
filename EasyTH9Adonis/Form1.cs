using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Open.Nat;

namespace EasyTH9Adonis
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        private Mapping _currentMapping;
        private NatDevice device;
        private InputSimulator inputSimulator = new InputSimulator();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        

        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_useUPNP.Checked)
            {
                label_Status.Text = @"Starting UPnP...";
                var discoverer = new NatDiscoverer();
                device = await discoverer.DiscoverDeviceAsync();
                await device.CreatePortMapAsync(_currentMapping = new Mapping(Protocol.Udp, Convert.ToInt32(numeric_Port.Value), Convert.ToInt32(numeric_Port.Value), "Touhou 7"));
                 textBox_upnpIP.Text = (await device.GetExternalIPAsync()).ToString();
                label_Status.Text = @"UPnP started.";
            }
            else
            {
                label_Status.Text = @"Stopping UPnP...";
                await device.DeletePortMapAsync(_currentMapping);
                textBox_upnpIP.Text = @"UPnP Disabled";
                label_Status.Text = @"UpnP disabled.";
            }
            
        }

        private void KillRogueProcesses()
        {
            label_Status.Text = @"Checking for rogue processes...";
            var wasRogueProcess = false;
            var p = Process.GetProcessesByName(domain_Adonis.SelectedItem.ToString()).FirstOrDefault();
            if (p != null)
            {
                wasRogueProcess = true;
                label_Status.Text = @"Killed rogue " + domain_Adonis.SelectedItem + @".exe process!";
            }

            p?.Close();

            if (domain_Adonis.SelectedIndex == 0)
                {
                    p = Process.GetProcessesByName("th09.exe").FirstOrDefault();
                    if (p != null)
                    {
                        wasRogueProcess = true;
                        label_Status.Text = @"Killed rogue th09.exe!";
                    }

                    p?.Close();
                }
                else
                {
                    p = Process.GetProcessesByName("th09e.exe").FirstOrDefault();
                    if (p != null)
                    {
                        wasRogueProcess = true;
                        label_Status.Text = @"Killed rogue th09e.exe!";
                    }

                    p?.Close();
                }
            

            label_Status.Text = !wasRogueProcess ? @"Finished killing rogue processes." : @"No rogue process has been found.";
        }

        private void StartAdonis()
        {
            if (domain_Adonis.SelectedIndex == -1) domain_Adonis.SelectedIndex = 0;
            KillRogueProcesses();
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");
            var cmd = Process.Start(startInfo);
            while (cmd != null && !cmd.Responding) Thread.Sleep(1);
            Thread.Sleep(100);
            SetForegroundWindow(cmd.MainWindowHandle);
            inputSimulator.Keyboard.TextEntry(domain_Adonis.SelectedItem + ".exe");
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            label_Status.Text = @"Taking a break.";
            Thread.Sleep(1000);
            label_Status.Text = @"Applying inputs";
        }

        private async void StopUPiP()
        {
            label_Status.Text = @"Stopping UPnP...";
            if (_currentMapping != null) await device.DeletePortMapAsync(_currentMapping);
            textBox_upnpIP.Text = @"UPnP Disabled";
            label_Status.Text = @"UpnP disabled.";
        }

        private void btn_StartServer_Click(object sender, EventArgs e)
        {
            StartAdonis();
            inputSimulator.Keyboard.TextEntry("s");
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select server option
            inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Waiting for Client to connect...";
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use recommended latency
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use default side
            label_Status.Text = @"Starting Touhou 9...";
        }

        private async void numeric_Port_ValueChanged(object sender, EventArgs e)
        {
            if (!checkBox_useUPNP.Checked) return;
            textBox_upnpIP.Text = @"Updating UPnP...";
            label_Status.Text = @"Deleting existing UPnP...";
            await device.DeletePortMapAsync(_currentMapping);
            label_Status.Text = @"Creating new UPnP...";
            await device.CreatePortMapAsync(_currentMapping = new Mapping(Protocol.Udp, Convert.ToInt32(numeric_Port.Value), Convert.ToInt32(numeric_Port.Value), "Touhou 7"));
            textBox_upnpIP.Text = device.GetExternalIPAsync().Result.ToString();
            label_Status.Text = @"UPnP updated.";
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopUPiP();
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            StartAdonis();
            inputSimulator.Keyboard.TextEntry("c");
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Connecting to Server...";
        }

        private void tabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 0)
            {
                StopUPiP();
            }
        }

        private void btn_watch_Click(object sender, EventArgs e)
        {
            StartAdonis();
            inputSimulator.Keyboard.TextEntry("w");
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Connecting to Server...";
        }

        private void label_GitHub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Tudi20/easyth09adonis");
        }
    }
}
