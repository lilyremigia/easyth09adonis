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
        private Mapping _currentMapping;
        private NatDevice _device;
        private readonly InputSimulator _inputSimulator = new InputSimulator();

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
                _device = await discoverer.DiscoverDeviceAsync();
                await _device.CreatePortMapAsync(_currentMapping = new Mapping(Protocol.Udp, Convert.ToInt32(numeric_Port.Value), Convert.ToInt32(numeric_Port.Value), "Touhou 7"));
                 textBox_upnpIP.Text = (await _device.GetExternalIPAsync()).ToString();
                label_Status.Text = @"UPnP started.";
            }
            else
            {
                label_Status.Text = @"Stopping UPnP...";
                await _device.DeletePortMapAsync(_currentMapping);
                textBox_upnpIP.Text = @"UPnP Disabled";
                label_Status.Text = @"UpnP disabled.";
            }
            
        }

        private byte KillRogueAdonis()
        {
            byte wasRogueProcess = 0;
            var p = Process.GetProcessesByName(domain_Adonis.SelectedItem.ToString()).FirstOrDefault();
            if (p != null)
            {
                wasRogueProcess = 1;
                label_Status.Text = @"Killed rogue " + domain_Adonis.SelectedItem + @".exe process!";
            }

            p?.Close();
            return wasRogueProcess;
        }

        private byte KillRogueTouhou()
        {
            byte wasRogueProcess = 0;
            Process p;
            if (domain_Adonis.SelectedIndex == 0)
            {
                p = Process.GetProcessesByName("th09.exe").FirstOrDefault();
                if (p != null)
                {
                    wasRogueProcess = 1;
                    label_Status.Text = @"Killed rogue th09.exe!";
                }

                p?.Close();
            }
            else
            {
                p = Process.GetProcessesByName("th09e.exe").FirstOrDefault();
                if (p != null)
                {
                    wasRogueProcess = 1;
                    label_Status.Text = @"Killed rogue th09e.exe!";
                }

                p?.Close();
            }

            return wasRogueProcess;
        }

        private void KillRogueProcesses()
        {
            label_Status.Text = @"Checking for rogue processes...";
            byte wasRogueProcess = 0;
            wasRogueProcess += KillRogueAdonis();
            wasRogueProcess += KillRogueTouhou();

            label_Status.Text = wasRogueProcess>0 ? @"Finished killing rogue processes." : @"No rogue process has been found.";
        }

        private void StartAdonis()
        {
            if (domain_Adonis.SelectedIndex == -1) domain_Adonis.SelectedIndex = 0;
            KillRogueProcesses();
            var startInfo = new ProcessStartInfo("cmd.exe");
            var cmd = Process.Start(startInfo);
            while (cmd != null && !cmd.Responding) Thread.Sleep(1);
            Thread.Sleep(100);
            if (cmd != null) NativeMethods.SetForegroundWindow(cmd.MainWindowHandle);
            _inputSimulator.Keyboard.TextEntry(domain_Adonis.SelectedItem + ".exe");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            label_Status.Text = @"Taking a break.";
            Thread.Sleep(1000);
            label_Status.Text = @"Applying inputs";
        }

        private async void StopUpiP()
        {
            label_Status.Text = @"Stopping UPnP...";
            if (_currentMapping != null) await _device.DeletePortMapAsync(_currentMapping);
            textBox_upnpIP.Text = @"UPnP Disabled";
            label_Status.Text = @"UpnP disabled.";
        }

        private void btn_StartServer_Click(object sender, EventArgs e)
        {
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("s");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select server option
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Waiting for Client to connect...";
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use recommended latency
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use default side
            label_Status.Text = @"Starting Touhou 9...";
        }

        private async void numeric_Port_ValueChanged(object sender, EventArgs e)
        {
            if (!checkBox_useUPNP.Checked) return;
            textBox_upnpIP.Text = @"Updating UPnP...";
            label_Status.Text = @"Deleting existing UPnP...";
            await _device.DeletePortMapAsync(_currentMapping);
            label_Status.Text = @"Creating new UPnP...";
            await _device.CreatePortMapAsync(_currentMapping = new Mapping(Protocol.Udp, Convert.ToInt32(numeric_Port.Value), Convert.ToInt32(numeric_Port.Value), "Touhou 7"));
            textBox_upnpIP.Text = _device.GetExternalIPAsync().Result.ToString();
            label_Status.Text = @"UPnP updated.";
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopUpiP();
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("c");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            _inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Connecting to Server...";
        }

        private void tabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 0)
            {
                StopUpiP();
            }
        }

        private void btn_watch_Click(object sender, EventArgs e)
        {
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("w");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            _inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            label_Status.Text = @"Connecting to Server...";
        }

        private void label_GitHub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Tudi20/easyth09adonis");
        }
    }

    internal class NativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern int SetForegroundWindow(IntPtr point);
    }
}
