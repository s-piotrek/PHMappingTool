using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PHMappingTool.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace PHMappingTool
{
    class MainWindowsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _xmlPath;
        public string XMLPath
        {
            get
            {
                return _xmlPath;
            }
            set
            {
                _xmlPath = value;
                RaisePropertyChanged(nameof(XMLPath));
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OpenPathDialogExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FileName = "Folder Selection.";
            if (openFileDialog.ShowDialog() == true)
            {
                XMLPath = Path.GetDirectoryName(openFileDialog.FileName);
                // System.IO.FileInfo fInfo = new System.IO.FileInfo(openFileDialog.FileName);
            }
        }
        public void CalculateMappingFileExecute()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            MappingFileViewModel mappingFileViewModel = new MappingFileViewModel();
            try
            {
                Loger.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                mappingFileViewModel.CalculateMappingFile(XMLPath);
                MessageBox.Show("Process finished successfully");
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message + " " + e.InnerException.Message);
                MessageBox.Show("Process failed.");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public ICommand OpenPathDialog
        {
            get
            {
                { return new RelayCommand(OpenPathDialogExecute, () => true); }
            }
        }

        public ICommand CalculateMappingFile
        {
            get
            {
                { return new RelayCommand(CalculateMappingFileExecute, ()=>true); }
            }
        }
    }
}
