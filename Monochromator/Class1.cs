using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
public class Monochromator
{
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
            Console.WriteLine("DEBUG: " + name + " Attempting step to " + lambda);
            sendCommand("sl " + lambda);
            Console.WriteLine("DEBUG: Reading response from " + name);
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
        Console.WriteLine("DEBUG: " + name + " Attempting step to " + wl);
        sendCommand("gt" + wl);
        Console.WriteLine("DEBUG: Reading response from " + name);
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
        Console.WriteLine("scanto "+lambda);
        sendCommand("gt " + lambda);
        Console.WriteLine("DEBUG: Reading response from " + name);
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
            Console.WriteLine(pname);
        }
        if (!SP.IsOpen) SP.Open();
    }
    private void sendCommand(string command)
    {
        if (SP.IsOpen)
        {
            
            SP.WriteLine(command);
            Console.WriteLine("Command: " + command);
        }
    }
    private void readResponse(out string res)
    {
        res = "Invalid response";
        if (SP.IsOpen)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            timeout = 1;
            while (SP.BytesToRead == 0 && sw.ElapsedMilliseconds < timeout) ;
            Console.WriteLine("DEBUG: " + name + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  start of the response");
            res = "";
            if (SP.BytesToRead > 0)
            {
                bool finish = false;
                while (!finish || (sw.ElapsedMilliseconds < timeout))
                {

                    char znak = (char)SP.ReadByte();
                    if (znak == '*' || znak == '!')
                    {
                        finish = true;
                        finishedMove = true;
                        Console.WriteLine("DEBUG: Final sign received");
                    }
                    res += znak.ToString();
                    //Console.Write(znak);
                }
                if (!finish) res += "\r\n Awaria";
                Console.WriteLine("DEBUG: " + name + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  end of the response");
                //Console.WriteLine(" ");
            }

            //res =this.SP.ReadTo("*");
            Console.WriteLine("DEBUG: " + name + " Buffer length is " + SP.BytesToRead);
            Console.WriteLine("DEBUG: " + name + " Response was: " + res);
            Console.WriteLine("race? ");
            Console.WriteLine("Debug: finished move flag is " + finishedMove);
        }

    }
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
        Console.WriteLine(repeatNeeded);
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
                        Console.WriteLine("DEBUG: Final sign received");
                        if (znak == '!')
                        {
                            Console.WriteLine("Defect !!!! ");//repeatNeeded = false;//true;
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
            Console.WriteLine("DEBUG: " + " Buffer length is " + ser.BytesToRead);
            Console.WriteLine("DEBUG: " + " Response was: " + res);
            Console.WriteLine("finishedMove flag is "+finishedMove);
        }
    }

}
