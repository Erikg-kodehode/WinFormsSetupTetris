using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace AprilFoolsJoke
{
    public class MainForm : Form
    {
        private Panel panelMainMenu, panelLicense, panelFolderSelection, panelInstallation;

        private Button standardInstallButton, customInstallButton;
        private TextBox licenseTextBox, folderTextBox;
        private CheckBox agreeCheckBox;
        private Button licenseNextButton, licenseBackButton;
        private Button folderBrowseButton, folderNextButton, folderBackButton;

        private Label installationMessageLabel, scanLabel, dotsLabel;
        private ProgressBar progressBar;
        private NotifyIcon toastNotifier;

        private Timer progressTimer, dotsAnimator;
        private static readonly Random random = new Random();

        private int progressValue = 0, dotsStage = 0;
        private string phase = "install", installPath = "";

        private Queue<string> installFiles, scanFiles, repairFiles;

        private readonly string[] allFakeFiles = new string[]
        {
            "kernel_conf.dll", "RegistryCat.exe", "pixel_drain_23.sys", "sys32_memeinjector.dll",
            "bios_patchy.sys", "trust_me_bro.tmp", "drivers\\banana.inf", "x86_fakepatch.sys",
            "notavirus.exe", "bootleg_crasher.dll", "doom.exe", "404handler.sys", "system32\\hovercat.sys",
            "quantum_boot.cfg", "update_backup.bak", "C:\\Temp\\sus_patch.tmp", "keyboardghost.dll",
            "RAM_refresher.exe", "nvidea.exe", "notepad_hook.dll", "firmware_patch.broken",
            "memcheck_fail.log", "services_fudge.cfg", "glitch_magic.sys", "invisible_config.json",
            "update_666.sys", "subroutine_zombie.dll", "error_please_reboot.now", "0xBADFOOD.bug",
            "gremlin.lock", "C:\\Drivers\\fake_patch.sys", "temp\\suspicious_file.exe", "config.meme",
            "wacky_scan.dat", "reboot_loop.sys", "spaghetti_stack.trace", "usb_fryer.inf",
            "update_mismatch.log", "registry_chaos.bat", "binary_dust.tmp", "Z:\\floppyboot.img",
            "kernel_sauce.sys", "hyperloop.bnk", "malware_simulator.vbs", "cursed_pixel.png",
            "RAMdump.meme", "Tetris-Installer-Fix.zip", "D:\\GameSave\\missing.save", "patched_patcher.pch",
            "buggy_driver.dll", "cpu_overheat.sim", "busted_plugin.js", "nonsense_update.exe",
            "network_glue.bin", "error_42.cat", "shutdown_helper.bak"
        };

        public MainForm()
        {
            this.Text = "Tetris Setup Wizard";
            this.Width = 440;
            this.Height = 320;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Icon = SystemIcons.Application;

            installFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));
            scanFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));
            repairFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));

            toastNotifier = new NotifyIcon { Icon = SystemIcons.Information, Visible = false };
            progressTimer = new Timer { Interval = 250 };
            dotsAnimator = new Timer { Interval = 500 };
            progressTimer.Tick += ProgressTimer_Tick;
            dotsAnimator.Tick += DotsAnimator_Tick;

            SetupMainMenu();
            SetupLicensePanel();
            SetupFolderPanel();
            SetupInstallationPanel();

            Controls.AddRange(new Control[] { panelMainMenu, panelLicense, panelFolderSelection, panelInstallation });
            ShowPanel(panelMainMenu);
        }

        private void SetupMainMenu()
        {
            panelMainMenu = new Panel { Dock = DockStyle.Fill };

            Label label = new Label
            {
                Text = "Welcome to the Tetris Setup Wizard.",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            standardInstallButton = new Button { Text = "Standard Install", Width = 140, Height = 35, Top = 80, Left = 150 };
            customInstallButton = new Button { Text = "Custom Install...", Width = 140, Height = 35, Top = 130, Left = 150 };

            standardInstallButton.Click += (s, e) => { installPath = @"C:\\Program Files\\Tetris"; licenseNextButton.Tag = "standard"; ShowPanel(panelLicense); };
            customInstallButton.Click += (s, e) => { licenseNextButton.Tag = "custom"; ShowPanel(panelLicense); };

            panelMainMenu.Controls.AddRange(new Control[] { label, standardInstallButton, customInstallButton });
        }

        private void SetupLicensePanel()
        {
            panelLicense = new Panel { Dock = DockStyle.Fill };

            Label title = new Label
            {
                Text = "License Agreement",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            licenseTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 380,
                Height = 120,
                Left = 30,
                Top = 50,
                Text = GetFakeLicenseText()
            };

            agreeCheckBox = new CheckBox
            {
                Text = "I accept the terms of this questionable agreement",
                Width = 350,
                Left = 30,
                Top = 180
            };
            agreeCheckBox.CheckedChanged += (s, e) => licenseNextButton.Enabled = agreeCheckBox.Checked;

            licenseBackButton = new Button { Text = "Back", Width = 80, Left = 100, Top = 210 };
            licenseNextButton = new Button { Text = "Next", Width = 80, Left = 240, Top = 210, Enabled = false };

            licenseBackButton.Click += (s, e) => ShowPanel(panelMainMenu);
            licenseNextButton.Click += LicenseNextButton_Click;

            panelLicense.Controls.AddRange(new Control[] { title, licenseTextBox, agreeCheckBox, licenseBackButton, licenseNextButton });
        }

        private void SetupFolderPanel()
        {
            panelFolderSelection = new Panel { Dock = DockStyle.Fill };

            Label label = new Label { Text = "Choose installation folder:", Left = 20, Top = 20, AutoSize = true };
            folderTextBox = new TextBox { Left = 20, Top = 50, Width = 250 };
            folderBrowseButton = new Button { Text = "Browse...", Left = 280, Top = 48 };

            folderBrowseButton.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                    folderTextBox.Text = fbd.SelectedPath;
            };

            folderNextButton = new Button { Text = "Next", Left = 240, Top = 90, Width = 80 };
            folderBackButton = new Button { Text = "Back", Left = 140, Top = 90, Width = 80 };

            folderBackButton.Click += (s, e) => ShowPanel(panelLicense);
            folderNextButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(folderTextBox.Text))
                {
                    installPath = folderTextBox.Text;
                    StartInstallation();
                }
                else MessageBox.Show("Please select a folder.", "Error");
            };

            panelFolderSelection.Controls.AddRange(new Control[] { label, folderTextBox, folderBrowseButton, folderNextButton, folderBackButton });
        }

        private void SetupInstallationPanel()
        {
            panelInstallation = new Panel { Dock = DockStyle.Fill };

            installationMessageLabel = new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            progressBar = new ProgressBar { Width = 300, Height = 25, Top = 70, Left = 70 };
            scanLabel = new Label { Width = 400, Top = 100, Left = 20 };
            dotsLabel = new Label { Width = 300, Top = 130, Left = 70, TextAlign = ContentAlignment.MiddleCenter };

            panelInstallation.Controls.AddRange(new Control[] { installationMessageLabel, progressBar, scanLabel, dotsLabel });
        }

        private void ShowPanel(Panel panel)
        {
            panelMainMenu.Visible = panelLicense.Visible = panelFolderSelection.Visible = panelInstallation.Visible = false;
            panel.Visible = true;
        }

        private string GetFakeLicenseText()
        {
            return @"TETRIS INSTALLER LICENSE AGREEMENT

This software is provided 'as-is', without any warranty of any kind,
except that it may prank you and/or confuse your coworkers.

By accepting, you agree that:
- You trust us, even though you shouldn't.
- You consent to the fake installation of potentially non-existent files.
- You will not hold us responsible for the existential dread that follows.";
        }
        private void LicenseNextButton_Click(object sender, EventArgs e)
        {
            string mode = licenseNextButton.Tag as string;
            if (mode == "custom") ShowPanel(panelFolderSelection);
            else StartInstallation();
        }

        private void StartInstallation()
        {
            ShowPanel(panelInstallation);
            installationMessageLabel.Text = $"Installing Tetris to:\n{installPath}";
            progressValue = 0;
            progressBar.Value = 0;
            phase = "install";
            progressTimer.Start();
            dotsAnimator.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            progressValue += phase == "scan" ? random.Next(5, 10) : random.Next(1, 3);
            progressBar.Value = Math.Min(progressValue, 100);

            if (installFiles.Count < 5 && phase == "install")
                installFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));
            if (scanFiles.Count < 5 && phase == "scan")
                scanFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));
            if (repairFiles.Count < 5 && phase == "repair")
                repairFiles = new Queue<string>(allFakeFiles.OrderBy(x => random.Next()));

            string file = phase switch
            {
                "install" => installFiles.Dequeue(),
                "scan" => scanFiles.Dequeue(),
                "repair" => repairFiles.Dequeue(),
                _ => ""
            };

            scanLabel.Text = $"{(phase == "install" ? "Copying" : phase == "scan" ? "Scanning" : "Repairing")}: {file}";

            if (progressValue >= 100)
            {
                progressTimer.Stop();
                dotsAnimator.Stop();

                if (phase == "install") ShowGlitchAndStartScan();
                else if (phase == "scan") PromptToRepair();
                else ShowPrank();
            }
        }

        private void DotsAnimator_Tick(object sender, EventArgs e)
        {
            dotsStage = (dotsStage + 1) % 4;
            string action = phase switch
            {
                "install" => "Installing",
                "scan" => "Scanning",
                "repair" => "Repairing",
                _ => ""
            };
            dotsLabel.Text = action + new string('.', dotsStage);
        }

        private async void ShowGlitchAndStartScan()
        {
            installationMessageLabel.Text = "Finalizing installation...";
            await Task.Delay(1500);

            toastNotifier.ShowBalloonTip(3000, "Tetris Setup Complete", "Tetris has been installed successfully.", ToolTipIcon.Info);

            for (int i = 0; i < 3; i++)
            {
                this.BackColor = Color.Black;
                await Task.Delay(150);
                this.BackColor = Color.White;
                await Task.Delay(150);
            }

            this.BackColor = SystemColors.Control;

            installationMessageLabel.ForeColor = Color.DarkRed;
            installationMessageLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            installationMessageLabel.Text = "⚠️ System Instability Detected!";
            await Task.Delay(3000);

            installationMessageLabel.ForeColor = Color.Black;
            installationMessageLabel.Font = new Font("Segoe UI", 9);
            installationMessageLabel.Text = "Initiating automatic scan...";

            phase = "scan";
            progressValue = 0;
            progressBar.Value = 0;
            progressTimer.Start();
            dotsAnimator.Start();
        }

        private async void PromptToRepair()
        {
            var result = MessageBox.Show("A potential system error was found.\nDo you want to attempt an automatic repair?", "System Issue Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                phase = "repair";
                progressValue = 0;
                progressBar.Value = 0;
                installationMessageLabel.Text = "Repairing system files...\nDo not turn off your computer.";
                progressTimer.Start();
                dotsAnimator.Start();
            }
            else
            {
                await ShowCountdownMessage("System will reboot in", "System Update", 5);
                ShowPrank();
            }
        }

        private async void ShowPrank()
        {
            await ShowCountdownMessage("System will reboot in", "System Update", 5);
            await ShowShutdownCountdown("Shutting down", "Windows", 5);

            MessageBox.Show(
                "🎉 April Fools! 🎉\n\nThe only bug here...\nIs YOU 🪲\n\nHope you enjoyed the ride!\n🥳💻",
                "System Purge Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk
            );

            this.Close();
        }

        private async Task ShowCountdownMessage(string baseMessage, string title, int seconds)
        {
            Form countdownForm = new Form
            {
                Width = 350,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false,
                TopMost = true
            };

            Label label = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            countdownForm.Controls.Add(label);
            countdownForm.Show();

            for (int i = seconds; i >= 1; i--)
            {
                label.Text = $"{baseMessage} {i} seconds...";
                await Task.Delay(1000);
            }

            countdownForm.Close();
        }

        private async Task ShowShutdownCountdown(string baseMessage, string title, int seconds)
        {
            Form shutdownForm = new Form
            {
                Width = 400,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false,
                BackColor = Color.Black,
                ForeColor = Color.White,
                TopMost = true
            };

            Label label = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            shutdownForm.Controls.Add(label);
            shutdownForm.Show();

            for (int i = seconds; i >= 1; i--)
            {
                label.Text = $"{baseMessage} in {i}...";
                await Task.Delay(1000);
            }

            shutdownForm.Close();
        }
    }
}

