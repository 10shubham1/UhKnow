namespace UhKnow
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text.Json;

    public partial class GamesSelector : Form
    {
        public Button addButton;
        public Button editJsonButton;
        public List<LauncherConfig> launcherConfig;
        public FlowLayoutPanel flowLayoutPanel;
        public string filePathToStore = "launcherConfig.json";
        public string iconDirectory = "Icons/";
        public string EditorPath = "S://Microsoft VS Code//Code.exe";

        public GamesSelector()
        {
            launcherConfig = GetLaunchConfig();
            Size = new Size(1350, 800);
            Text = "Choose What To Run";
            Icon = new Icon(Path.Combine(iconDirectory, "GameController.ico"));
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;


            addButton = new Button
            {
                Size = new Size(100, 40),
                Location = new Point(Width - 160, Height - 125),
                Text = "Add",
                ForeColor = Color.White,
                BackColor = Color.DarkCyan
            };
            Controls.Add(addButton);
            addButton.Click += new EventHandler(AddButtonClick);
            Resize += GamesSelectorResize;

            editJsonButton = new Button
            {
                Size = new Size(100, 40),
                Location = new Point(Width - 280, Height - 125),
                Text = "Edit JSON",
                ForeColor = Color.White,
                BackColor = Color.Gray
            };
            Controls.Add(editJsonButton);
            editJsonButton.Click += new EventHandler(EditJsonButtonClick);

            flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                WrapContents = true,
                AutoSize = true,
                Padding = new Padding(10)
            };

            Controls.Add(flowLayoutPanel);

            RefreshPanel();

            FileSystemWatcher watcher = new (Environment.CurrentDirectory, filePathToStore);
            watcher.Changed += (sender, e) =>
            {
                RefreshPanelonFileChange();
            };
            watcher.EnableRaisingEvents = true;
        }

        private List<LauncherConfig> GetLaunchConfig()
        {
            try
            {
                return JsonSerializer.Deserialize<List<LauncherConfig>>(File.ReadAllText(filePathToStore)) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private void GamesSelectorResize(object? sender, EventArgs e)
        {
            addButton.Location = new Point(Width - 160, Height - 125);
            editJsonButton.Location = new Point(Width - 280, Height - 125);
        }

        private void AddButtonClick(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new ()
            {
                Filter = "Executable files (*.exe)|*.exe",
                Title = "Select Game Launcher",
                Multiselect = false
            };

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog1.FileName;
                string gameName = Path.GetFileNameWithoutExtension(selectedFilePath);
                Icon? fileIcon = Icon.ExtractAssociatedIcon(selectedFilePath);

                bool? containsFileName = launcherConfig?.Any(config =>
                    gameName.Equals(config.GameName, StringComparison.OrdinalIgnoreCase));

                if (containsFileName == null || (bool)containsFileName)
                {
                    return;
                }

                string? iconFileName = SaveIconToFile(fileIcon, gameName);
                launcherConfig?.Add(
                    new LauncherConfig()
                    {
                        LauncherPath = selectedFilePath,
                        GameName = gameName,
                        IconPath = iconFileName
                    }
                );

                string jsonConfig = JsonSerializer.Serialize(launcherConfig, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

                File.WriteAllText(filePathToStore, jsonConfig);
                RefreshPanel();
            }
        }

        private void EditJsonButtonClick(object? sender, EventArgs e)
        {
            if (File.Exists(filePathToStore))
            {
                try
                {
                    Process.Start(EditorPath, filePathToStore);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"File not found: {filePathToStore}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string? SaveIconToFile(Icon? icon, string exeFilePath)
        {
            try
            {
                string iconFileName = Path.Combine(iconDirectory, Path.ChangeExtension(exeFilePath, "ico"));

                using (FileStream fs = new (iconFileName, FileMode.Create))
                {
                    icon?.Save(fs);
                }

                return iconFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving icon to file: {ex.Message}");
                return null;
            }
        }

        private FlowLayoutPanel AddPanel(LauncherConfig config)
        {
            FlowLayoutPanel gameTilePanel = new ()
            {
                Dock = DockStyle.Left,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(50) 
            };
            
            PictureBox gameIconPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(100, 100),
                Image = config.IconPath == null ? 
                    Image.FromFile(Path.Combine(iconDirectory, "GameController.ico"))
                    : Image.FromFile(config.IconPath),
                Cursor = Cursors.Hand
            };

            gameIconPictureBox.Click += (sender, e) => RunGame(config);

            Label gameNameLabel = new Label
            {
                Text = config.GameName,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                ForeColor = Color.White
            };

            gameTilePanel.Controls.Add(gameIconPictureBox);
            gameTilePanel.Controls.Add(gameNameLabel);

            return gameTilePanel;
        }

        private void RefreshPanel()
        {
            flowLayoutPanel.Controls.Clear();
            foreach (LauncherConfig config in launcherConfig)
            {
                FlowLayoutPanel gameTilePanel = AddPanel(config);
                flowLayoutPanel.Controls.Add(gameTilePanel);
            }
        }

        private void RefreshPanelonFileChange()
        {
            Thread.Sleep(20);
            launcherConfig = GetLaunchConfig()
;
            flowLayoutPanel.Invoke(new Action(RefreshPanel));
        }

        private void RunGame(LauncherConfig config)
        {
            using (ConfirmationForm confirmationForm = new ($"Do you want to open {config.GameName}?"))
            {
                confirmationForm.ShowDialog(this);
                string LauncherPath = Path.GetDirectoryName(config.LauncherPath) ?? string.Empty;
                Directory.SetCurrentDirectory(LauncherPath);

                if (confirmationForm.UserConfirmed)
                {
                    try
                    {
                        Process.Start(config.LauncherPath);
                        Close();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.ErrorCode == -2147467259)
                        {
                            ProcessStartInfo startInfo = new ()
                            {
                                FileName = config.LauncherPath,
                                UseShellExecute = true,
                                Verb = "runas"
                            };
                            Process.Start(startInfo);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show($"Error opening EXE path: {ex.ErrorCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}