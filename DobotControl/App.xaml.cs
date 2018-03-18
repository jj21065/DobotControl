using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DobotControl
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    /// 
    public partial class App : Application
    {
        protected void App_Startup(object sender, StartupEventArgs e)
        {
            //base.OnStartup(e);

            //for(int i = 0;i != e.Args.Length;++i)
            //{
            //    MessageBox.Show(e.Args[i]);
            //}
          //  MessageBox.Show(e.Args.Length.ToString());
            
            
            MainWindow mainWindow = new MainWindow(e.Args);
            //if (startMinimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.Show();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
        //void App_Startup(object sender, StartupEventArgs e)
        //{
        //    // Application is running
        //    // Process command line args
        //    //bool startMinimized = false;
        //    //for (int i = 0; i != e.Args.Length; ++i)
        //    //{
        //    //    if (e.Args[i] == "/StartMinimized")
        //    //    {
        //    //        startMinimized = true;
        //    //    }
        //    //}

        //    // Create main application window, starting minimized if specified
        //    MainWindow mainWindow = new MainWindow();
        //   //if (startMinimized)
        //    {
        //        mainWindow.WindowState = WindowState.Minimized;
        //    }
        //    mainWindow.Show();
        //}
    }

}
