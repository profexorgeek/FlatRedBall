﻿using MasterInstaller;
using MasterInstaller.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterInstallerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ComponentBase _currentComponent;

        MainFlow mainFlow;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Show();

                mainFlow = new MainFlow();
                mainFlow.MainForm = this;

                mainFlow.StartFlow();
            }
            catch(Exception e)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                System.IO.File.WriteAllText(folder + "/InstallerError.txt", e.ToString());
            }
        }

        public async void SetComponent(ComponentBase component)
        {
            try
            {
                _currentComponent = component;
                this.Content = null;

                var newControl = component.MainControl;

                if (newControl == null)
                {
                    throw new Exception("The component " + component.GetType().Name + " does not define a Control.  Every component needs a control");
                }
                this.Content = newControl;
            }
            catch(Exception e)
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                System.IO.File.WriteAllText(folder + "/InstallerError.txt", e.ToString());
            }
            await component.Show();
        }


    }
}
