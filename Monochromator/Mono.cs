using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
public class Monochromator : IDisposable
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly SerialPort SP;
    public string name;
    public string cWavLen;
    private readonly int timeout;
    public bool responseObtained = false;
    public bool bCommFailed = false;
    private string lastCom;
    private string lastresponse;
    private bool moving;
    public void Goto(double lambda)
    {
        Goto(lambda.ToString().Replace(',', '.'));
    }
    public void Goto(string lambda)
    {
        int temptimeout  = timeout;

        if (SP.IsOpen)
        {
            lambda = lambda.Replace(',', '.');
            Stopwatch scansw = new Stopwatch();
            scansw.Start();
            log.Debug(name + " Attempting step to " + lambda);
            SendCommand("sl " + lambda);
            log.Debug("Reading response from " + name);
            responseObtained = false;
            moving = true;
            while (!responseObtained) Thread.Sleep(10);
            cWavLen = lambda;
            log.Debug("Step took " + (scansw.ElapsedMilliseconds / 1000.0).ToString());
        };
    }
    public void ScanTo(double lambda)
    {
        string wl;
        wl = lambda.ToString();
        wl = wl.Replace(',', '.');
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
        SendCommand("gs" + number);
        moving = true;
        while (!responseObtained) Thread.Sleep(1);
        log.Info("Gratinng change took " + (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds");
    }
    public void ScanTo(string lambda)
    {
        Stopwatch scansw = new Stopwatch();
        scansw.Start();
        lambda = lambda.Replace(',', '.');
        log.Info("scanto " + lambda);
        SendCommand("gt " + lambda);
        log.Debug("Reading response from " + name);
        responseObtained = false;
        moving = true;
        while (!responseObtained) Thread.Sleep(10);
        cWavLen = lambda;
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
    private void SendCommand(string command)
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
        SendCommand("cw");
        bCommFailed = false;
        Thread.Sleep(100);
        SendCommand("cw");
        bCommFailed = false;
        Thread.Sleep(100);
        SendCommand("cw");
        Thread.Sleep(100);
        log.Debug("Fix procedure, repeat needed value is "+bCommFailed);
        moving = true;
    }
    public void Parse(string command)
    {
        log.Debug("Parsing in mono class ");
        string[] comarg;
        if (command.Contains(" ")) comarg = command.Split(' ');
        else
        {
            comarg = new string[1];
            comarg[0] = command;
        }
    switch (comarg[0])
        {
            case "scan":
                {
                    Goto(comarg[1]);
                    break;
                }
            case "goto":{
                    ScanTo(comarg[1]);
                    break;
                }

        }
        }
    public Monochromator()
    {
        SP = new SerialPort("Com1", 9600, Parity.None, 8, StopBits.One)
        {
             NewLine = "\r"
        };
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
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // dispose managed resources
            SP.Dispose();
        }
        // free native resources
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
