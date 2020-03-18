using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Background_Terminal
{
    public class ProcessHandler
    {
        public event DataReceivedEventHandler OnOutputDataReceived = null;
        public event DataReceivedEventHandler OnErrorDataReceived = null;

        private string fileName;
        private Process process;
        public ProcessHandler(string fileName)
        {
            this.fileName = fileName;
        }

        public void WriteLine(string text)
        {
            process.StandardInput.Write(text + "\n");
        }

        public async Task<int> Run()
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

            process = new Process();
            
            process.StartInfo.FileName = fileName;
            process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            process.EnableRaisingEvents = true;
            if(OnOutputDataReceived != null) process.OutputDataReceived += OnOutputDataReceived;
            if(OnErrorDataReceived != null) process.ErrorDataReceived += OnErrorDataReceived;
            
            process.Exited += new EventHandler((sender, args) =>
            {
                taskCompletionSource.SetResult(process.ExitCode);
                process.Dispose();
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return await taskCompletionSource.Task;
        }
    }
}
