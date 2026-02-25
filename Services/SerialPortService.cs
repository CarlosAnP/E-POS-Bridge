using System.IO.Ports;

namespace EposBridge.Services;

public class SerialPortService
{
    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool OpenDrawer(string portName)
    {
        try
        {
            using var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            port.Open();
            // ESC p m t1 t2
            byte[] cmd = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA };
            port.Write(cmd, 0, cmd.Length);
            
            byte[] cmd2 = new byte[] { 0x1B, 0x70, 0x01, 0x19, 0xFA };
            port.Write(cmd2, 0, cmd2.Length);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
