namespace UhKnow
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    public partial class GamesSelector : Form
    {
        public JsonObject appConfig;
        public Button addButton;
        public Button editJsonButton;
        public List<LauncherConfig> launcherConfig;
        public FlowLayoutPanel flowLayoutPanel;
        public string filePathToStore;
        public string iconDirectory;
        public int ScreenWidth;
        public int ScreenHeight;

        public GamesSelector()
        {
            appConfig = GetAppConfig();
            filePathToStore = GetConfigValue("LauncherConfigs");
            launcherConfig = GetLaunchConfig();
            iconDirectory = GetConfigValue("IconsDirectory");
            Rectangle ScreenBounds = Screen.PrimaryScreen == null ?
                new Rectangle(0, 0, 1920, 1080) : Screen.PrimaryScreen.Bounds;
            ScreenWidth = ScreenBounds.Width;
            ScreenHeight = ScreenBounds.Height;
            Size = new Size(ScreenWidth * 2 / 3, ScreenHeight * 2 / 3);
            Text = GetConfigValue("Title");
            Padding = new Padding(0, 0, 0, 50); 
            try
            {
                Icon = new Icon(Path.Combine(iconDirectory, GetConfigValue("DefaultIcon")));
            }
            catch
            {
                Icon = null;
            }
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;


            addButton = new Button
            {
                Size = new Size(ScreenWidth / 25, ScreenHeight / 35),
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
                Size = new Size(ScreenWidth / 25, ScreenHeight / 35),
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

        private static JsonObject GetAppConfig()
        {
          try
            {
                return JsonSerializer.Deserialize<JsonObject>(File.ReadAllText("Appsettings.json")) ?? [];
            }
            catch
            {
                return [];
            }  
        }

        private string GetConfigValue(string key)
        {
            return appConfig[key]?.ToString() ?? "";
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
                    Process.Start(GetConfigValue("EditorPath"), filePathToStore);
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
                Padding = new Padding(0, ScreenHeight / 25, 0, 0) 
            };
            Image image;
            try
            {
                image = Image.FromFile(config.IconPath!);
            }
            catch
            {
                image = Image.FromFile(Path.Combine(iconDirectory, GetConfigValue("DefaultIcon")));
            }
            PictureBox gameIconPictureBox = new ()
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(ScreenWidth / 10, ScreenHeight / 15),
                Image = image,
                Cursor = Cursors.Hand
            };

            gameIconPictureBox.Click += (sender, e) => RunGame(config);

            Label gameNameLabel = new ()
            {
                Text = config.GameName,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                ForeColor = Color.White,
                Margin = new Padding(0, 10, 0, 20)
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

        private async void RefreshPanelonFileChange()
        {
            await Task.Delay(20);
            launcherConfig = GetLaunchConfig();
            flowLayoutPanel.Invoke(RefreshPanel);
        }

        private void RunGame(LauncherConfig config)
        {
            using (ConfirmationForm confirmationForm = new (
                string.Format(GetConfigValue("GameLaunchConfirmationMessage"), config.GameName),
                ScreenWidth,
                ScreenHeight))
            {
                confirmationForm.ShowDialog(this);

                if (confirmationForm.UserConfirmed)
                {
                    string LauncherPath = Path.GetDirectoryName(config.LauncherPath) ?? string.Empty;
                    Directory.SetCurrentDirectory(LauncherPath);
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
                            try
                            {
                                Process.Start(startInfo);
                                Close();
                            }
                            catch (Win32Exception e)
                            {
                                if (e.ErrorCode == -2147467259)
                                {
                                    MessageBox.Show($"Permission Denied For Opening {config.GameName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Error opening EXE path: {ex.ErrorCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(GetConfigValue("RootDirectory"));
                    }
                }
            }
        }
    }
}