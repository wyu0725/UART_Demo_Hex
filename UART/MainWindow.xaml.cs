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
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
namespace UART
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        //Richtextbox
        FlowDocument mcFlowDoc = new FlowDocument();
        Paragraph para = new Paragraph();
        StringBuilder DisplayString = new StringBuilder();
        //Serial 
        SerialPort UartDevice = new SerialPort();
        //string recieved_data;
        byte receiveByteData;
        string FilePath;
        bool FileSaved;
        byte[] FileSaveByte = new byte[512];
        int DataCount = 0;
        private BinaryWriter bw;
        bool HexOrAscii = true;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            UserInitial();
        }

        private void UserInitial()
        {
            btnConnect.Content = "Connect";
            btnConnect.Background = Brushes.Gray;
            btnSendData.IsEnabled = false;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult key = MessageBox.Show(
               "Are you sure you want to quit",
               "Confirm",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question,
               MessageBoxResult.No);
            e.Cancel = (key == MessageBoxResult.No);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content.ToString() == "Connect")
            {
                UartDevice.PortName = cbxCOMSelect.Text;
                UartDevice.BaudRate = int.Parse(cbxBaudRate.Text);
                UartDevice.Handshake = Handshake.None;
                UartDevice.DataBits = 8;
                UartDevice.Encoding = System.Text.Encoding.UTF8;
                UartDevice.Parity = Parity.None;
                UartDevice.StopBits = StopBits.One;
                UartDevice.ReadTimeout = 200;
                UartDevice.WriteTimeout = 50;
                try
                {
                    UartDevice.Open();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Please Check the uart is not used by other process", "COM Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch(IOException)
                {
                    MessageBox.Show("Please Check the uart parameter", "COM Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                btnConnect.Content = "Disconnect";
                btnConnect.Background = Brushes.Cyan;
                btnSendData.IsEnabled = true; ;
                UartDevice.DataReceived += new SerialDataReceivedEventHandler(Receive);
            }
            else
            {
                UartDevice.Close();
                UartDevice.Dispose();
                btnConnect.Content = "Connect";
                btnConnect.Background = Brushes.Gray;
                UartDevice.DataReceived -= new SerialDataReceivedEventHandler(Receive);
                btnSendData.IsEnabled = false;
            }
        }

        private delegate void UpdateUiTextDelegate(string text);
        private void Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            try
            {
                receiveByteData = (byte)UartDevice.ReadByte();
            }
            catch (TimeoutException)
            {
                MessageBox.Show("COM time out", "Timeout ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FileSaveByte[DataCount] = receiveByteData;
            DataCount += 1;
            byte[] buf = new byte[1] { receiveByteData };
            string displayByte;
            if (HexOrAscii)
            {
                displayByte = string.Format("{0:X2}", receiveByteData);
            }
            else
            {
                displayByte = Encoding.ASCII.GetString(buf);
            }
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(DisplayData), displayByte);
        }
        private void DisplayData(string text)
        {
            // Assign the value of the recieved_data to the RichTextBox.
            DisplayString.Append(text);
            DisplayString.Append(" ");
            tbxReceiveData.Text = DisplayString.ToString();
            tbxDataCount.Text = DataCount.ToString();
        }

        public void SerialCmdSend(string data)
        {
            if (UartDevice.IsOpen)
            {
                try
                {
                    byte[] hexstring;
                    // Send the binary data out the port
                    if(cbxDataFormat.SelectedIndex == 0)
                    {
                        HexOrAscii = true;
                        bool bResult = DataProcess.HexStringToByteArray(data,out hexstring);
                        if (!bResult)
                        {
                            MessageBox.Show("Send HEX vaule failed. Please check the value", "Data format ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        HexOrAscii = false;
                        hexstring = Encoding.ASCII.GetBytes(data);
                    }
                    
                        //There is a intermitant problem that I came across
                        //If I write more than one byte in succesion without a 
                        //delay the PIC i'm communicating with will Crash
                        //I expect this id due to PC timing issues ad they are
                        //not directley connected to the COM port the solution
                        //Is a ver small 1 millisecound delay between chracters
                        foreach (byte hexval in hexstring)
                        {
                        byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
                        UartDevice.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Send data Failed", "System ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("UART device do not initiate!", "Connect ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnSendData_Click(object sender, RoutedEventArgs e)
        {
            if(DataCount > 511)
            {
                if (!(MessageBox.Show("You must save the file or the data would be overwrite. Confirm?", "Confirm Message", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                    return;
                else
                {
                    FileSaveByte = new byte[512];
                    DataCount = 0;
                    tbxDataCount.Text = DataCount.ToString();
                    DisplayString.Clear();
                    tbxReceiveData.Text = "";
                }
            }
            SerialCmdSend(tbxSendData.Text);
            tbxSendData.Text = "";
        }

        private void btnDirCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileDir.Text.Trim()))
            {
                MessageBox.Show("Please fill the file directory path first!", //text
                                "Created failure",   //caption
                                MessageBoxButton.OK,//button
                                MessageBoxImage.Error);//icon
            }
            else
            {
                string FileDir = System.IO.Path.Combine(txtFileDir.Text);
                if (!Directory.Exists(FileDir))//路径不存在
                {
                    Directory.CreateDirectory(FileDir);
                }
                else
                {
                    MessageBox.Show("The File Directory already exits", //text
                                    "Created failure",   //caption
                                    MessageBoxButton.OK,//button
                                    MessageBoxImage.Warning);//icon
                }
            }
        }

        private void btnDirDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileDir.Text.Trim()))
            {
                MessageBox.Show("Please fill the file directory path first!", //text
                                "Delete failure",   //caption
                                MessageBoxButton.OK,//button
                                MessageBoxImage.Error);//icon
            }
            else
            {
                string FileDir = System.IO.Path.Combine(txtFileDir.Text);
                if (!Directory.Exists(FileDir))//路径不存在
                {
                    MessageBox.Show("The File Directory doesn't exits", //text
                                     "Delete failure",   //caption
                                     MessageBoxButton.OK,//button
                                     MessageBoxImage.Error);//icon
                }
                else
                {
                    Directory.Delete(FileDir);
                }
            }
        }

        private void btnFileSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileName.Text.Trim()))
            {
                MessageBox.Show("File name is missing", //text
                                        "save failure", //caption
                                   MessageBoxButton.OK, //button
                                    MessageBoxImage.Error);//icon
            }
            else
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "dat";
                saveDialog.AddExtension = true;
                saveDialog.FileName = txtFileName.Text;
                saveDialog.InitialDirectory = @txtFileDir.Text;
                saveDialog.OverwritePrompt = true;
                saveDialog.Title = "Save Data files";
                saveDialog.ValidateNames = true;
                FilePath = System.IO.Path.Combine(saveDialog.InitialDirectory, saveDialog.FileName);//文件路径
                if (saveDialog.ShowDialog().Value)
                {
                    FileStream fs = null;
                    if (!File.Exists(FilePath))
                    {
                        fs = File.Create(FilePath);
                        FileSaved = true;

                    }
                    else
                    {
                        MessageBox.Show("the file is already exist", //text
                                                "imformation", //caption
                                           MessageBoxButton.OK, //button
                                            MessageBoxImage.Warning);//icon                        
                    }
                    fs.Close();//close the file
                }
            }
        }

        private void btnClearDisplay_Click(object sender, RoutedEventArgs e)
        {
            FileSaveByte = new byte[512];
            DataCount = 0;
            tbxDataCount.Text = DataCount.ToString();
            DisplayString.Clear();
            tbxReceiveData.Text = "";
        }

        private void btnSaveData_Click(object sender, RoutedEventArgs e)
        {
            if(!CheckFileSaved())
            {
                return;
            }
            if(DataCount == 0)
            {
                return;
            }
            bw = new BinaryWriter(File.Open(FilePath, FileMode.Append));
            bw.Write(FileSaveByte);
            bw.Flush();
            bw.Dispose();
            bw.Close();

            FileSaved = false;
        }

        private bool CheckFileSaved()
        {
            if ((FilePath != null) && (!string.IsNullOrEmpty(FilePath.Trim())) && FileSaved)
            {
                return true;
            }
            else
            {
                if (MessageBox.Show("File not saved. Use default name?", "Confirm Message", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (SaveFileDefault())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

        }
        private bool SaveFileDefault()
        {
            string DefaultDicrectory = @txtFileDir.Text;
            string DefaultFileName = DateTime.Now.ToString();
            DefaultFileName = DefaultFileName.Replace("/", "_");
            DefaultFileName = DefaultFileName.Replace(":", "_");
            DefaultFileName = DefaultFileName.Replace(" ", "T");
            DefaultFileName += ".dat";
            FilePath = System.IO.Path.Combine(DefaultDicrectory, DefaultFileName);
            FileStream fs = null;
            if (!File.Exists(FilePath))
            {
                fs = File.Create(FilePath);
                FileSaved = true;
                fs.Close();
                txtFileName.Text = DefaultFileName;
                return true;
            }
            else
            {
                MessageBox.Show("Save file failure. Please save the file manual", "File Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "Help/Help.pdf");
        }

    }
}
