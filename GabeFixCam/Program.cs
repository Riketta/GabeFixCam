using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GabeFixCam
{
    class Program
    {
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        static IntPtr processHandle;
        static string gameBuild = "7.2.0.23877";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("GabeFixCam for WoW Legion " + gameBuild + " by Riketta (github.com/riketta).");

                Process process = Process.GetProcessesByName("Wow-64")[0];
                processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, process.Id);

                float cameraRange = 60f;
                if (args.Length == 1)
                    cameraRange = float.Parse(args[0]);
                Console.WriteLine("Expected range: " + cameraRange + ". You can pass new value as argument.");

                int bytesWritten = 0;
                byte[] buffer = BitConverter.GetBytes(cameraRange);

                while (true)
                {
                    IntPtr addr = process.MainModule.BaseAddress + 0x1921EB8;
                    addr = (IntPtr)ReadInt64(addr) + 0x3328;
                    addr = (IntPtr)ReadInt64(addr) + 0x23C;

                    float currentRange = ReadFloat(addr);
                    Console.Write("Current range: " + currentRange + ". Updating range... ");
                    if (currentRange != 0)
                    {
                        WriteProcessMemory((int)processHandle, addr, buffer, buffer.Length, ref bytesWritten);
                        Console.WriteLine("Success! Range is " + cameraRange);
                    }
                    else Console.WriteLine("Fail! Skipping...");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }

        static int ReadInt32(IntPtr address)
        {
            return BitConverter.ToInt32(ReadMemory(address, 4), 0);
        }

        static Int64 ReadInt64(IntPtr address)
        {
            return BitConverter.ToInt64(ReadMemory(address, 8), 0);
        }

        static float ReadFloat(IntPtr address)
        {
            return BitConverter.ToSingle(ReadMemory(address, 4), 0);
        }

        static byte[] ReadMemory(IntPtr address, int size) //, ref int lpNumberOfBytesRead)
        {
            int bytesRead = 0;
            byte[] outBuffer = new byte[size];
            ReadProcessMemory((int)processHandle, address, outBuffer, outBuffer.Length, ref bytesRead);
            return outBuffer;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
    }
}
