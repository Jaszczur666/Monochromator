using System;
using System.Diagnostics;

public class Monochromator
{
    private System.IO.Ports.SerialPort SP;
    public string name;
    private int timeout;
    public void Goto(double lambda)
    {
        string res;
        if (SP.IsOpen)
        {
            Console.WriteLine("DEBUG: " + name + " Attempting step to " + lambda.ToString());
            sendCommand("sl" + lambda.ToString());
            Console.WriteLine("DEBUG: Reading response from " + name);
            readResponse(out res);

        };
    }
    public void ScanTo(double lambda)
    {
        string res;
        Console.WriteLine("DEBUG: " + name + " Attempting step to " + lambda.ToString());
        sendCommand("gt" + lambda.ToString());
        Console.WriteLine("DEBUG: Reading response from " + name);
        readResponse(out res);

    }
    public void InitializePort(string pname)
    {
        if (SP.IsOpen) SP.Close();
        if (pname != "")
        {
            SP.PortName = pname;
            Console.WriteLine(ToString());
            Console.WriteLine(pname);
        }
        if (!SP.IsOpen) SP.Open();
    }
    private void sendCommand(string command)
    {
        if (SP.IsOpen) SP.Write(command + "\r\n");
    }
    private void readResponse(out string res)
    {
        res = "Invalid response";
        if (SP.IsOpen)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (SP.BytesToRead == 0 && sw.ElapsedMilliseconds < timeout) ;
            Console.WriteLine("DEBUG: " + name + " " + sw.ElapsedMilliseconds.ToString() + " ms waited for  start of the response");
            res = "";
            if (SP.BytesToRead > 0)
            {
                bool finish = false;
                while (!finish || (sw.ElapsedMilliseconds < timeout))
                {

                    char znak = (char)SP.ReadByte();
                    if (znak == '*' || znak == '!') finish = true;
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
        }

    }
    public Monochromator()
    {
        SP = new System.IO.Ports.SerialPort();
        SP.StopBits = System.IO.Ports.StopBits.One;
        SP.BaudRate = 9600;
        SP.DataBits = 8;
        SP.Parity = System.IO.Ports.Parity.None;
        SP.PortName = "COM1";
        SP.NewLine = "\r\n";
        name = "unset";
        timeout = 1000;

    }
}
