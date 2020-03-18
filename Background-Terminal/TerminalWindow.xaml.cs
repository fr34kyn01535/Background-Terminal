using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Background_Terminal
{
    public partial class TerminalWindow : Window
    {
        public delegate void ReadLine(string text);
        public event ReadLine OnReadLine = null;

        Regex ansiColorStripper = new Regex(@"\x1b\[*\?*\S*");
    
        private ObservableCollection<string> terminalData = new ObservableCollection<string>();
        private static BrushConverter brushConverter = new BrushConverter();

        public void WriteLine(string data)
        {
            terminalData.Add(ansiColorStripper.Replace(data, ""));
        }

        public IntPtr Handle { get; private set; }

        public TerminalWindow()
        {
            InitializeComponent();
            Handle = new WindowInteropHelper(this).Handle;
            terminalData_TextBox.FontSize = Int32.Parse(ConfigurationManager.AppSettings["FontSize"]);
            terminalData_TextBox.Foreground = (Brush)brushConverter.ConvertFromString(ConfigurationManager.AppSettings["FontColor"]);
            input_TextBox.Foreground = (Brush)brushConverter.ConvertFromString(ConfigurationManager.AppSettings["FontColor"]);
            Left = Int32.Parse(ConfigurationManager.AppSettings["PosX"]);
            Top = Int32.Parse(ConfigurationManager.AppSettings["PosY"]);
            Width = Int32.Parse(ConfigurationManager.AppSettings["Width"]);
            Height = Int32.Parse(ConfigurationManager.AppSettings["Height"]);

            Activated += (object sender, EventArgs e) =>
            {
                FocusManager.SetFocusedElement(this, input_TextBox);
                Keyboard.Focus(input_TextBox);
            };
            GotFocus += (object sender, RoutedEventArgs e) =>
            {
                FocusManager.SetFocusedElement(this, input_TextBox);
                Keyboard.Focus(input_TextBox);
            };

            LostFocus += (object sender, RoutedEventArgs e) =>
            {
                Keyboard.ClearFocus();
            };

            terminalData.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
                (object o, System.Collections.Specialized.NotifyCollectionChangedEventArgs target) =>
                {
                    string retStr = "";

                    if (terminalData.Count > 1)
                    {
                        for (int i = 0; i < terminalData.Count - 1; i++)
                        {
                            retStr += terminalData[i];
                            retStr += Environment.NewLine;
                        }

                        retStr += terminalData[terminalData.Count - 1];
                    }
                    else if (terminalData.Count == 1)
                    {
                        retStr += terminalData[0];
                    }

                    Dispatcher.Invoke(() =>
                    {
                        terminalData_TextBox.Text = retStr;
                        terminalData_TextBox.ScrollToEnd();
                    });
                });
            Show();
        }

        public void UpdateTerminalWindowDataMargin()
        {
            terminalData_TextBox.Margin = new Thickness(0, 0, 0, input_TextBox.ActualHeight);
        }

        public void TerminalDataTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            terminalData_TextBox.ScrollToEnd();
        }

        public void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Return) || e.Key.Equals(Key.Enter))
            {
                OnReadLine?.Invoke(input_TextBox.Text);
                terminalData.Add(input_TextBox.Text);
                input_TextBox.Text = "";
            }
        }

        public void TerminalWindow_Loaded(object sender, EventArgs e)
        {
            UpdateTerminalWindowDataMargin();
        }
    }
}