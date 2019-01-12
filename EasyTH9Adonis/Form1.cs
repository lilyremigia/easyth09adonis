using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using IniParser;
using IniParser.Model;
using Open.Nat;

namespace EasyTH9Adonis
{
    public partial class Form1 : Form
    {
        private Mapping _currentMapping;
        private NatDevice _device;
        private IniData _iniData;
        private const string IniFile = "adonis_config";
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly FileIniDataParser _parser = new FileIniDataParser();
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region LoadIniFile
            // Exit and Error if Ini File not found.
            if (!File.Exists(IniFile))
            {
                MessageBox.Show(@"Couldn't find adonis_config.ini.\nAre you sure the program in the correct folder?",
                    @"Ini File Not Found Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            _iniData = _parser.ReadFile(IniFile);
            numeric_Port.Value = int.Parse(_iniData["SaveIP"]["ServerPort"]);
            textBox_ConnectIP.Text = _iniData["SaveIP"]["PeerIP"];
            if (_iniData["EasyAdonis"]["UseUPnP"] != null)
                checkBox_UseUPnP.Checked = bool.Parse(_iniData["EasyAdonis"]["UseUPnP"]);
            else _iniData["EasyAdonis"]["UseUPnP"] = checkBox_UseUPnP.Checked.ToString();
            textBox_username.Text = _iniData["PlayerName"]["Name"];
            numericUpDown_GameWindow_X.Value = int.Parse(_iniData["Window"]["X"]);
            numericUpDown_GameWindow_Y.Value = int.Parse(_iniData["Window"]["Y"]);
            numericUpDown_GameWindow_Width.Value = int.Parse(_iniData["Window"]["Width"]);
            numericUpDown_GameWindow_Height.Value = int.Parse(_iniData["Window"]["Height"]);
            checkBox_GameWindow_TitleBar.Checked = bool.Parse(_iniData["Window"]["TitleBar"]);
            checkBox_GameWindow_Enabled.Checked = bool.Parse(_iniData["Window"]["enabled"]);
            checkBox_GameWindow_AlwaysOnTop.Checked = bool.Parse(_iniData["Window"]["AlwaysOnTop"]);
            #endregion
        }

        private static int ConvertBoolToInt(bool b) => b ? 1 : 0;

        private void SaveIniFile()
        {
            _iniData["SaveIP"]["ServerPort"] = numeric_Port.Value.ToString(CultureInfo.InvariantCulture);
            _iniData["SaveIP"]["PeerIP"] = textBox_ConnectIP.Text;
            _iniData["EasyAdonis"]["UseUPnP"] = checkBox_UseUPnP.Checked.ToString();
            _iniData["PlayerName"]["Name"] = textBox_username.Text;
            _iniData["Window"]["X"] = numericUpDown_GameWindow_X.ToString();
            _iniData["Window"]["Y"] = numericUpDown_GameWindow_Y.ToString();
            _iniData["Window"]["Width"] = numericUpDown_GameWindow_Width.ToString();
            _iniData["Window"]["Height"] = numericUpDown_GameWindow_Height.ToString();
            _iniData["Window"]["TitleBar"] = ConvertBoolToInt(checkBox_GameWindow_TitleBar.Checked).ToString();
            _iniData["Window"]["AlwaysOnTop"] = ConvertBoolToInt(checkBox_GameWindow_AlwaysOnTop.Checked).ToString();
            _iniData["Window"]["enabled"] = ConvertBoolToInt(checkBox_GameWindow_Enabled.Checked).ToString();
            _parser.WriteFile(IniFile, _iniData);
        }
        
        #if DEBUG
        private byte KillRogueAdonis()
        #else 
        private void KillRogueAdonis()
        #endif
        {
            #if DEBUG
            byte wasRogueProcess = 0;
            #endif
            var p = Process.GetProcessesByName(domain_Adonis.SelectedItem.ToString()).FirstOrDefault();
            if (p != null)
            {
                #if DEBUG
                wasRogueProcess = 1;
                label_Status.Text = @"Killed rogue " + domain_Adonis.SelectedItem + @".exe process!";
                #endif
            }

            p?.Close();
            #if DEBUG
            return wasRogueProcess;
            #endif
        }

        #if DEBUG
        private byte KillRogueTouhou()
        #else
        private void KillRogueTouhou()
        #endif
        {
            #if DEBUG
            byte wasRogueProcess = 0;
            #endif
            Process p;
            if (domain_Adonis.SelectedIndex == 0)
            {
                p = Process.GetProcessesByName("th09.exe").FirstOrDefault();
                if (p != null)
                {
                    #if DEBUG
                    wasRogueProcess = 1;
                    label_Status.Text = @"Killed rogue th09.exe!";
                    #endif
                }

                p?.Close();
            }
            else
            {
                p = Process.GetProcessesByName("th09e.exe").FirstOrDefault();
                if (p != null)
                {
                    #if DEBUG
                    wasRogueProcess = 1;
                    label_Status.Text = @"Killed rogue th09e.exe!";
                    #endif
                }

                p?.Close();
            }
            #if DEBUG
            return wasRogueProcess;
            #endif
        }

        private void KillRogueProcesses()
        {
            #if DEBUG
            label_Status.Text = @"Checking for rogue processes...";
            byte wasRogueProcess = 0;
            wasRogueProcess += KillRogueAdonis();
            wasRogueProcess += KillRogueTouhou();
            label_Status.Text = wasRogueProcess>0 ? @"Finished killing rogue processes." : @"No rogue process has been found.";
            #else
            label_Status.Text = @"The girls are preparing...";
            KillRogueAdonis();
            KillRogueTouhou();
            #endif
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
            SaveIniFile();
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("s");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select server option
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            #if DEBUG
            label_Status.Text = @"Waiting for Client to connect...";
            #endif
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use recommended latency
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Use default side
            #if DEBUG
            label_Status.Text = @"Starting Touhou 9...";
            #endif
        }

        private async void numeric_Port_ValueChanged(object sender, EventArgs e)
        {
            if (!checkBox_UseUPnP.Checked) return;
            textBox_upnpIP.Text = @"Updating UPnP...";
            #if DEBUG
            label_Status.Text = @"Deleting existing UPnP...";
            #endif
            await _device.DeletePortMapAsync(_currentMapping);
            #if DEBUG
            label_Status.Text = @"Creating new UPnP...";
            #endif
            await _device.CreatePortMapAsync(_currentMapping = new Mapping(Protocol.Udp, Convert.ToInt32(numeric_Port.Value), Convert.ToInt32(numeric_Port.Value), "Touhou 7"));
            textBox_upnpIP.Text = _device.GetExternalIPAsync().Result.ToString();
            label_Status.Text = @"UPnP updated.";
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopUpiP();
            SaveIniFile();
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            SaveIniFile();
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("c");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            _inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            #if DEBUG
            label_Status.Text = @"Connecting to Server...";
            #endif
        }

        private void tabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 0) StopUpiP();
        }

        private void btn_watch_Click(object sender, EventArgs e)
        {
            SaveIniFile();
            StartAdonis();
            _inputSimulator.Keyboard.TextEntry("w");
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Select client option
            _inputSimulator.Keyboard.TextEntry(textBox_ConnectIP.Text);
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select IP
            _inputSimulator.Keyboard.TextEntry(Convert.ToInt32(numeric_Port.Value).ToString());
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); //Select port
            #if DEBUG
            label_Status.Text = @"Connecting to Server...";
            #endif
        }

        private void label_GitHub_Click(object sender, EventArgs e) => Process.Start("https://github.com/Tudi20/easyth09adonis");

        private async void checkBox_UseUPnP_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_UseUPnP.Checked)
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
    }

    internal static class NativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern int SetForegroundWindow(IntPtr point);
    }
}
