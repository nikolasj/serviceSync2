using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    [RunInstaller(true)]
    public partial class KudaGoServiceInstaller : System.Configuration.Install.Installer
    {
        public KudaGoServiceInstaller()
        {
            InitializeComponent();
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            // Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            // Service Information
            serviceInstaller.ServiceName = "KudaGoService";
            serviceInstaller.DisplayName = "KudaGoService";
            serviceInstaller.Description = "Служба синхронизации с KudaGo";
            serviceInstaller.StartType = ServiceStartMode.Automatic; // or automatic

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
            this.AfterInstall += new InstallEventHandler(Installer_AfterInstall);
        }

        private void Installer_AfterInstall(object sender, InstallEventArgs e)
        {
            //стартуем службу в новом потоке
            var thread = new Thread(() =>
            {
                Thread.Sleep(1000); //Даем немного времени на завршение консоли
                using (System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController("KudaGoService"))
                {
                    sc.Start();
                }
            });
            thread.IsBackground = false; //не даем погасить поток при завершении
            thread.Start();
        }

    }
}
