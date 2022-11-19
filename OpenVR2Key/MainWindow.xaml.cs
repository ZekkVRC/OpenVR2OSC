using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace OpenVR2Key
{
    public partial class MainWindow : Window
    {
        private static Mutex _mutex = null;
        private readonly static string DEFAULT_KEY_LABEL = "Replace me with an OSC endpoint! (ie: \"end\" would hit /avatar/parameters/end)";
        private MainController _controller;
        private List<BindingItem> _items = new List<BindingItem>();
        //private object _activeElement;
        private string _currentlyRunningAppId = MainModel.CONFIG_DEFAULT;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private HashSet<string> _activeKeys = new HashSet<string>();
        private bool _initDone = false;
        private bool _dashboardIsVisible = false;
        public MainWindow()
        {
            InitWindow();
            InitializeComponent();
            Title = Properties.Resources.AppName;

            // Prevent multiple instances running at once
            _mutex = new Mutex(true, Properties.Resources.AppName, out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                    Application.Current.MainWindow,
                    "This application is already running!",
                    Properties.Resources.AppName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Application.Current.Shutdown();
            }


            _controller = new MainController
            {
                // Reports on the status of OpenVR
                StatusUpdateAction = (connected) =>
                {
                    Debug.WriteLine($"Status Update Action: connected={connected}");
                    var message = connected ? "Connected" : "Disconnected";
                    var color = connected ? Brushes.OliveDrab : Brushes.Tomato;
                    Dispatcher.Invoke(() =>
                    {
                        Label_OpenVR.Content = message;
                        Label_OpenVR.Background = color;
                        if (!connected && _initDone && MainModel.LoadSetting(MainModel.Setting.ExitWithSteam))
                        {
                            if (_notifyIcon != null) _notifyIcon.Dispose();
                            System.Windows.Application.Current.Shutdown();
                        }
                    });
                },

                // Triggered when a new scene app is detected
                AppUpdateAction = (appId) =>
                {
                    Debug.WriteLine($"App Update Action: appId={appId}");
                    _currentlyRunningAppId = appId;
                    var color = Brushes.OliveDrab;
                    if (appId == MainModel.CONFIG_DEFAULT)
                    {
                        color = Brushes.Gray;
                    }
                    var appIdFixed = appId.Replace("_", "__"); // Single underscores makes underlined chars
                    Dispatcher.Invoke(() =>
                    {
                        Debug.WriteLine($"Setting AppID to: {appId}");
                        Label_Application.Content = appIdFixed;
                        Label_Application.Background = color;
                    });
                },

                // We should update the text on the current binding we are recording
                //KeyTextUpdateAction = (keyText, cancel) =>
                //{
                //    Debug.WriteLine($"Key Text Update Action: keyText={keyText}");
                //    Dispatcher.Invoke(() =>
                //    {
                //        if (_activeElement != null)
                //        {
                //           (_activeElement as Label).Content = keyText;
                //            if (cancel) UpdateLabel(_activeElement as Label, false);
                //        }
                //    });
                //},

                // We have loaded a config
                ConfigRetrievedAction = (config, forceButtonOff) =>
                {
                    var loaded = config != null;
                    //if (loaded) Debug.WriteLine($"Config Retrieved Action: count()={config.Count}");
                    Dispatcher.Invoke(() =>
                    {
                        if (loaded) InitList(config);
                        UpdateConfigButton(loaded, forceButtonOff);
                    });
                },

                KeyActivatedAction = (key, on) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!_dashboardIsVisible)
                        {
                            if (on) _activeKeys.Add(key);
                            else _activeKeys.Remove(key);
                            if (_activeKeys.Count > 0) Label_Keys.Content = string.Join(", ", _activeKeys);
                            else Label_Keys.Content = "None";
                            Label_Keys.ToolTip = "";
                            Label_Keys.Background = Brushes.Gray;
                        }
                    });
                },

                // Dashboard Visible
                DashboardVisibleAction = (visible) => {
                    Dispatcher.Invoke(() =>
                    {
                        _dashboardIsVisible = visible;
                        if (visible)
                        {
                            Label_Keys.Content = "Blocked";
                            Label_Keys.ToolTip = "The SteamVR Dashboard is visible which will block input from this application.";
                            Label_Keys.Background = Brushes.Tomato;
                        }
                        else
                        {
                            Label_Keys.Content = "Unblocked";
                            Label_Keys.ToolTip = "";
                            Label_Keys.Background = Brushes.Gray;
                        }
                    });
                }
            };

            // Receives error messages from OpenVR
            _controller.SetDebugLogAction((message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    WriteToLog(message);
                });
            });

            // Init the things
            var actionKeys = InitList();
            _controller.Init(actionKeys);
            InitSettings();
            InitTrayIcon();
            _initDone = true;
        }

        private void WriteToLog(String message)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var oldLog = TextBox_Log.Text;
            var lines = oldLog.Split('\n');
            Array.Resize(ref lines, 3);
            var newLog = string.Join("\n", lines);
            TextBox_Log.Text = $"{time}: {message}\n{newLog}";
        }

        private void InitWindow()
        {
            var shouldMinimize = MainModel.LoadSetting(MainModel.Setting.Minimize);
            var onlyInTray = MainModel.LoadSetting(MainModel.Setting.Tray);

            if (shouldMinimize)
            {
                Hide();
                WindowState = WindowState.Minimized;
                ShowInTaskbar = !onlyInTray;
            }
        }

        private void InitTrayIcon()
        {
            var icon = Properties.Resources.icon.Clone() as System.Drawing.Icon;
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Click += NotifyIcon_Click;
            _notifyIcon.Text = $"Click to show the {Properties.Resources.AppName} window";
            _notifyIcon.Icon = icon;
            _notifyIcon.Visible = true;
        }

        #region bindings

        // Fill list with entries
        private string[] InitList(List<BindingItem> config = null)
        {
            if (MainController.porterror)
            {
                WriteToLog("Error: Port 9001 was bound on application start. Please close all other OSC programs, then launch this again. Once launched you can restart other programs.");
            }
            var actionKeys = new List<string>();
            actionKeys.AddRange(GenerateActionKeyRange(16, 'L')); // Left
            actionKeys.AddRange(GenerateActionKeyRange(16, 'R')); // Right
            actionKeys.AddRange(GenerateActionKeyRange(16, 'C')); // Chord
            actionKeys.AddRange(GenerateActionKeyRange(16, 'T')); // Tracker
            string[] GenerateActionKeyRange(int count, char type)
            {
                var keys = new List<string>();
                for (var i = 1; i <= count; i++) keys.Add($"{type}{i}");
                return keys.ToArray();
            }

            if (config == null) config = new List<BindingItem>();
            _items.Clear();
            foreach (var actionKey in actionKeys)
            {
                var text = actionKey;//config.Contains(actionKey) ? actionKey : string.Empty;
                if (actionKey == text) text = DEFAULT_KEY_LABEL;
                //Debug.WriteLine("TextBeforeTheFall: " + text);
                if (text.Contains("System.Windows.Controls.TextBox:")) text = text.Split(' ').GetValue(1).ToString(); _items.Add(new BindingItem()
                {
                    Key = actionKey,
                    Label = $"Key {actionKey}",
                    Text = text
                });
            }
            ItemsControl_Bindings.ItemsSource = null;
            ItemsControl_Bindings.ItemsSource = _items;

            MainModel._bindings = _items;
            MainModel.StoreConfig();

            return actionKeys.ToArray();
        }



        // Binding data class
        public class BindingItem
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string Text { get; set; }
        }
        #endregion

        #region events
        private void Update_Textbox(object sender, TextChangedEventArgs e)
        {
            TextBox b = (TextBox)sender;
            BindingItem identity = (BindingItem)b.DataContext;
            string key = identity.Key;
            //string letter = key.Substring(0, 1);
            //int number = int.Parse(key.Substring(1, key.Length));
            //if (letter == "L")
            //{

            //}
            //else if (letter == "R")
           // {
             //   number += 16;
            //}
            //else if (letter == "T")
           // {
            //    number += 32;
            //}
            //Debug.WriteLine(key);
            var config = MainModel.RetrieveConfig();
            //Debug.WriteLine("config in: " + config.ToString());
            var actionKeys = new List<string>();
            actionKeys.AddRange(GenerateActionKeyRange(16, 'L')); // Left
            actionKeys.AddRange(GenerateActionKeyRange(16, 'R')); // Right
            actionKeys.AddRange(GenerateActionKeyRange(16, 'C')); // Chord
            actionKeys.AddRange(GenerateActionKeyRange(16, 'T')); // Tracker
            string[] GenerateActionKeyRange(int count, char type)
            {
                var keys = new List<string>();
                for (var i = 1; i <= count; i++) keys.Add($"{type}{i}");
                return keys.ToArray();
            }

            if (config == null) config = new List<BindingItem>();//Dictionary<string, Key[]>();
            _items.Clear();
            foreach (var actionKey in actionKeys)
            {
                string text = actionKey;//config.Contains(actionKey) ? actionKey: string.Empty;
                if(actionKey == key)
                {
                    text = sender.ToString();
                }
                if (actionKey == text) text = DEFAULT_KEY_LABEL;
                //Debug.WriteLine("TextBeforeTheFall: "+text);
                if (text.Contains("System.Windows.Controls.TextBox:")) text = text.Split(' ').GetValue(1).ToString();
                _items.Add(new BindingItem()
                {
                    Key = actionKey,
                    Label = $"Key {actionKey}",
                    Text = text
                }) ;
                
            }
            MainModel._bindings = _items;
            MainModel.StoreConfig();

        }
        // All key down events in the app
        //private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //   e.Handled = true;
        //   var key = e.Key == Key.System ? e.SystemKey : e.Key;
        //  if (!_controller.OnKeyDown(key))
        //  {
        //       string message = $"Key not mapped: " + key.ToString();
        //      WriteToLog(message);
        //  }

        //}

        // All key up events in the app
        //private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    e.Handled = true;
        //    var key = e.Key == Key.System ? e.SystemKey : e.Key;
        //    _controller.OnKeyUp(key);
        //}

        protected override void OnClosing(CancelEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var onlyInTray = MainModel.LoadSetting(MainModel.Setting.Tray);
            switch (WindowState)
            {
                case WindowState.Minimized:
                    ShowInTaskbar = !onlyInTray;
                    break;
                default:
                    ShowInTaskbar = true;
                    Show();
                    break;
            }
        }

        #endregion

        #region actions
        private void UpdateConfigButton(bool hasConfig, bool forceButtonOff = false)
        {
            
        }

        // Click to either create new config for current app or remove the existing config.
        private void Button_AppBinding_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

                    var result = MessageBox.Show(
                        Application.Current.MainWindow,
                        "Save Config?",
                        "OpenVR2Key",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );
                    if (result == MessageBoxResult.Yes)
                    {
                        MainModel.StoreConfig();
                        
                    }
           }
 

        // This should clear all bindings from the current config
        private void Button_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                "Refresh",
                "OpenVR2Key",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            if (result == MessageBoxResult.Yes)
            {
                //MainModel.ClearBindings();
                
                MainModel.StoreConfig(MainModel.RetrieveConfig());
                _controller.LoadConfig(true);
                //InitList();
            }
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e)
        {
            _controller.OpenConfigFolder();
        }


        #endregion

        #region bindings

        // Main action that is clicked from the list to start and end registration of keys
        //private void Label_RecordSave_Click(object sender, MouseButtonEventArgs e)
        //{
        //var element = sender as Label;
        //var dataItem = element.DataContext as BindingItem;
        //var active = _controller.ToggleRegisteringKey(dataItem.Key, element, out object activeElement);
        //UpdateLabel(activeElement as Label, active);
        //if (active) _activeElement = activeElement;
        //else _activeElement = null;
        //}

        //private void UpdateLabel(Label label, bool active)
        //{
        //   {
        //       label.Foreground = active ? Brushes.DarkRed : Brushes.Black;
        //      label.BorderBrush = active ? Brushes.Tomato : Brushes.DarkGray;
        //      label.Background = active ? Brushes.LightPink : Brushes.LightGray;
        //  }
        //}

        //private void Label_HighlightOn(object sender, RoutedEventArgs e)
        //{
        //    if (_activeElement != sender) (sender as Label).Background = Brushes.WhiteSmoke;
        //}

        //private void Label_HighlightOff(object sender, RoutedEventArgs e)
        //{
        //    if (_activeElement != sender) (sender as Label).Background = Brushes.LightGray;
        //}

        // Clear the current binding
        //private void Button_ClearCancel_Click(object sender, RoutedEventArgs e)
        //{
         //   var button = sender as Button;
         //   var dataItem = button.DataContext as BindingItem;
         //   MainModel.RemoveBinding(dataItem.Key);
         //   DockPanel sp = VisualTreeHelper.GetParent(button) as DockPanel;
         //   var element = sp.Children[2] as Label;
         //   element.Content = DEFAULT_KEY_LABEL;
       // }
        #endregion

        #region settings
        // Load settings and apply them to the checkboxes
        private void InitSettings()
        {
            CheckBox_Minimize.IsChecked = MainModel.LoadSetting(MainModel.Setting.Minimize);
            CheckBox_Tray.IsChecked = MainModel.LoadSetting(MainModel.Setting.Tray);
            CheckBox_ExitWithSteamVR.IsChecked = MainModel.LoadSetting(MainModel.Setting.ExitWithSteam);
            CheckBox_DebugNotifications.IsChecked = MainModel.LoadSetting(MainModel.Setting.Notification);
            CheckBox_HapticFeedback.IsChecked = MainModel.LoadSetting(MainModel.Setting.Haptic);
#if DEBUG
            Label_Version.Content = $"{MainModel.GetVersion()}d";
#else
            Label_Version.Content = MainModel.GetVersion();
#endif
        }
        private bool CheckboxValue(RoutedEventArgs e)
        {
            var name = e.RoutedEvent.Name;
            return name == "Checked";
        }
        private void CheckBox_Minimize_Checked(object sender, RoutedEventArgs e)
        {
            MainModel.UpdateSetting(MainModel.Setting.Minimize, CheckboxValue(e));
        }

        private void CheckBox_Tray_Checked(object sender, RoutedEventArgs e)
        {
            MainModel.UpdateSetting(MainModel.Setting.Tray, CheckboxValue(e));
        }

        private void CheckBox_DebugNotifications_Checked(object sender, RoutedEventArgs e)
        {
            MainModel.UpdateSetting(MainModel.Setting.Notification, CheckboxValue(e));
        }

        private void CheckBox_HapticFeedback_Checked(object sender, RoutedEventArgs e)
        {
            MainModel.UpdateSetting(MainModel.Setting.Haptic, CheckboxValue(e));
        }

        private void ClickedURL(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            Process.Start(link.NavigateUri.ToString());
        }
        #endregion

        #region trayicon
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            Show();
            Activate();
        }
        #endregion

        private void CheckBox_ExitWithSteamVR_Checked(object sender, RoutedEventArgs e)
        {
            MainModel.UpdateSetting(MainModel.Setting.ExitWithSteam, CheckboxValue(e));
        }
    }
}