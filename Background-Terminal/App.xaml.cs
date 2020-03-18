using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static Background_Terminal.TerminalWindow;

namespace Background_Terminal
{
    public partial class App : Application
    {
        private TerminalWindow terminalWindow;
        private Key key1, key2;
        private ProcessHandler processHandler;


        public App()
        {
            terminalWindow = new TerminalWindow();
            processHandler = new ProcessHandler("cmd.exe");

            terminalWindow.OnReadLine += (string line) =>
            {
                Console.WriteLine("> " + line);
                processHandler.WriteLine(line);
            };

            processHandler.OnOutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Console.WriteLine("< " + e.Data);
                terminalWindow.WriteLine(e.Data);
            };

            key1 = KeyInterop.KeyFromVirtualKey(Int32.Parse(ConfigurationManager.AppSettings["Key1"]));
            key2 = KeyInterop.KeyFromVirtualKey(Int32.Parse(ConfigurationManager.AppSettings["Key2"]));

            Win32Interop.KeyTriggered = OnKeyTriggered;
            Win32Interop.SetKeyhook();

            Exit += OnExit;
            processHandler.Run();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Win32Interop.DestroyKeyhook();
            terminalWindow.Close();
            Environment.Exit(0);
        }

        private void OnKeyTriggered(int keyCode)
        {
            int vKey1 = KeyInterop.VirtualKeyFromKey((Key)key1);
            int vKey2 = KeyInterop.VirtualKeyFromKey((Key)key2);

            if (keyCode == vKey2 && Win32Interop.IsKeyDown(vKey1))
            {
                Win32Interop.ClickSimulateFocus(terminalWindow);
                Win32Interop.SetForegroundWindow(terminalWindow.Handle);
                Win32Interop.SetActiveWindow(terminalWindow.Handle);
                terminalWindow.Focus();
                terminalWindow.Activate();
            }
        }
    }
}
