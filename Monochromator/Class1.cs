using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
public class Monochromator
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private SerialPort SP;
    public string name;
    private int timeout;
    public  bool responseObtained = false;
    public bool bCommFailed = false;
    private string lastCom;
    private string lastresponse;    
    private bool moving;
    public void Goto(double lambda)
    {
        Goto(lambda.ToString().Replace(',','.'));
    }
    public void Goto(string lambda)
    {
        int temptimeout;
        temptimeout = timeout;
        
        if (SP.IsOpen)
        {
            lambda = lambda.Replace(',', '.');
            Stopwatch scansw = new Stopwatch();
            scansw.Start();
            log.Debug(name + " Attempting step to " + lambda);
            sendCommand("sl " + lambda);
            log.Debug("Reading response from " + name);
            responseObtained = false;
            moving = true;
            while (!responseObtained) Thread.Sleep(10);
            log.Debug("Step took " + (scansw.ElapsedMilliseconds / 1000.0).ToString());
        };
    }
    public void ScanTo(double lambda)
    {
        string wl;
        wl = lambda.ToString();
        wl=wl.Replace(',', '.');
        ScanTo(wl);
        /*Stopwatch scansw = new Stopwatch();
        scansw.Start();
        log.Debug(name + " Attempting step to " + wl);
        sendCommand("gt" + wl);
        log.Debug("Reading response from " + name);
        responseObtained = false;
        moving = true;
        while (!responseObtained) ;
        log.Debug("Step took "+(scansw.ElapsedMilliseconds / 1000).ToString());*/
    }
    public void SelectGrating(string number) {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        sendCommand("gs" + number);
        moving = true;
        while (!responseObtained) Thread.Sleep(1);
        log.Info("Gratinng change took " + (sw.ElapsedMilliseconds / 1000.0).ToString()+" seconds");
    }
    public void ScanTo(string lambda)
    {
        Stopwatch scansw = new Stopwatch();
        scansw.Start();
        lambda =lambda.Replace(',', '.');
        log.Info("scanto "+lambda);
        sendCommand("gt " + lambda);
        log.Debug("Reading response from " + name);
        responseObtained = false;
        moving = true;
        while (!responseObtained) Thread.Sleep(10);
        log.Debug("Step took " + (scansw.ElapsedMilliseconds / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + " seconds");
    }
    public void MarkMoveFinished() {
        responseObtained = true;
    }
    public void InitializePort(string pname)
    {
        if (SP.IsOpen) SP.Close();
        if (pname != "")
        {
            SP.PortName = pname;
            log.Info("pname "+pname);
        }
        if (!SP.IsOpen) SP.Open();
    }
    private void sendCommand(string command)
    {
        if (SP.IsOpen)
        {
            
            SP.WriteLine(command);
            lastCom = command;
            log.Debug("Command: " + command);
        }
    }
    
    public void Fix()
    {
        moving = false;
        bCommFailed = false;
        sendCommand("cw");
        bCommFailed = false;
        Thread.Sleep(100);
        sendCommand("cw");
        bCommFailed = false;
        Thread.Sleep(100);
        sendCommand("cw");
        Thread.Sleep(100);
        log.Debug("Fix procedure, repeat needed value is "+bCommFailed);
        moving = true;
    }
    public Monochromator()
    {
        SP = new SerialPort();
        SP.StopBits = System.IO.Ports.StopBits.One;
        SP.BaudRate = 9600;
        SP.DataBits = 8;
        SP.Parity = System.IO.Ports.Parity.None;
        SP.PortName = "COM1";
        SP.NewLine = "\r";
        SP.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        name = "unset";
        timeout = 1000;
        
    }
    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        string res;
        SerialPort ser = (SerialPort)sender;
        res = "Invalid response";
        if (ser.IsOpen)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (ser.BytesToRead == 0) ;// && sw.ElapsedMilliseconds < timeout) ;
            res = "";
            if (ser.BytesToRead > 0)
            {
                bool finish = false;
                while (!finish /*|| (sw.ElapsedMilliseconds < timeout)*/)
                {

                    char znak = (char)ser.ReadByte();
                    if (znak == '*' || znak == '!')
                    {
                        finish = true;
                        if (moving)
                        {
                            MarkMoveFinished();
                            moving = false;
                        }
                        log.Debug("Final sign received, it was "+znak);
                        if (znak == '!')
                        {
                            log.Error("Communication with mono "+name+" failed");
                            bCommFailed = true;
                        }
                    }
                    res += znak.ToString();
                }
            }
            log.Debug(" Buffer length is " + ser.BytesToRead);
            log.Debug(" Response was: " + res);
            lastresponse = res;
            log.Debug("responseObtained flag is "+responseObtained);
        }
    }

}
