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
    public  bool finishedMove = false;
    public bool repeatNeeded = false;
    private bool moving;
    public void Goto(double lambda)
    {
        //int temptimeout;
        //temptimeout = timeout;
        //if (SP.IsOpen)
        //{
        //    Console.WriteLine("DEBUG: " + name + " Attempting step to " + lambda.ToString());
        //    sendCommand("sl" + lambda.ToString());
        //    Console.WriteLine("DEBUG: Reading response from " + name);
        //    timeout=300000; // Could take a while.
        //    timeout = temptimeout;
        //    finishedMove = false;
        //    repeatNeeded = false;
        //};
        Goto(lambda.ToString().Replace(',','.'));
    }
    public void Goto(string lambda)
    {
        int temptimeout;
        temptimeout = timeout;
        
        if (SP.IsOpen)
        {
            lambda = lambda.Replace(',', '.');
            log.Debug(name + " Attempting step to " + lambda);
            sendCommand("sl " + lambda);
            log.Debug("Reading response from " + name);
            finishedMove = false;
            moving = true;
        };
    }
    public void ScanTo(double lambda)
    {
        //string res;
        string wl;
        wl = lambda.ToString();
        wl=wl.Replace(',', '.');
        log.Debug(name + " Attempting step to " + wl);
        sendCommand("gt" + wl);
        log.Debug("Reading response from " + name);
        finishedMove = false;
        moving = true;
        //readResponse(out res);

    }
    public void ScanTo(string lambda)
    {
        //string res;
        //Console.WriteLine("DEBUG: " + name + " Attempting step to " + lambda);
        //Console.WriteLine("Command: gt " + lambda);
        lambda=lambda.Replace(',', '.');
        log.Info("scanto "+lambda);
        sendCommand("gt " + lambda);
        log.Debug("Reading response from " + name);
        finishedMove = false;
        moving = true;
        //readResponse(out res);

    }
    public void MarkMoveFinished() {
        finishedMove = true;
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
            //Console.WriteLine("Command: " + command);
            log.Debug("Command: " + command);
        }
    }
    //private void readResponse(out string res)
    //{
    //    res = "Invalid response";
    //    if (SP.IsOpen)
    //    {
    //        Stopwatch sw = new Stopwatch();
    //        sw.Start();
    //        timeout = 1;
    //        while (SP.BytesToRead == 0 && sw.ElapsedMilliseconds < timeout) ;
    //        Console.WriteLine("DEBUG: " + name + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  start of the response");
    //        res = "";
    //        if (SP.BytesToRead > 0)
    //        {
    //            bool finish = false;
    //            while (!finish || (sw.ElapsedMilliseconds < timeout))
    //            {

    //                char znak = (char)SP.ReadByte();
    //                if (znak == '*' || znak == '!')
    //                {
    //                    finish = true;
    //                    finishedMove = true;
    //                    log.Debug("Final sign received, it was "+znak);
    //                }
    //                res += znak.ToString();
    //                //Console.Write(znak);
    //            }
    //            if (!finish) res += "\r\n Awaria";
    //            log.Debug( name + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  end of the response");
    //            //Console.WriteLine(" ");
    //        }

    //        //res =this.SP.ReadTo("*");
    //        log.Debug(name + " Buffer length is " + SP.BytesToRead);
    //        log.Debug(name + " Response was: " + res);
    //        //Console.WriteLine("race? ");
    //        Console.WriteLine("Debug: finished move flag is " + finishedMove);
    //    }

    //}
    public void Fix()
    {
        moving = false;
        repeatNeeded = false;
        sendCommand("cw");
        repeatNeeded = false;
        Thread.Sleep(100);
        sendCommand("cw");
        repeatNeeded = false;
        Thread.Sleep(100);
        sendCommand("cw");
        Thread.Sleep(100);
        log.Debug("Fix procedure, repeat needed value is "+repeatNeeded);
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
            //Console.WriteLine("DEBUG: " + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  start of the response");
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
                        //finishedMove = true;
                        if (moving)
                        {
                            MarkMoveFinished();
                            moving = false;
                        }
                        log.Debug("Final sign received, it was "+znak);
                        if (znak == '!')
                        {
                            //Console.WriteLine("Defect !!!! ");//repeatNeeded = false;//true;
                            log.Error("Communication with mono "+name+" failed");
                            repeatNeeded = true;
                        }
                    }
                    res += znak.ToString();
                    //Console.Write(znak);
                }
                //if (!finish) res += "\r\n Awaria";
                //Console.WriteLine("DEBUG: " + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  end of the response");
                //Console.WriteLine(" ");
            }

            //res =this.SP.ReadTo("*");
            log.Debug(" Buffer length is " + ser.BytesToRead);
            log.Debug(" Response was: " + res);
            log.Debug("finishedMove flag is "+finishedMove);
        }
    }

}
