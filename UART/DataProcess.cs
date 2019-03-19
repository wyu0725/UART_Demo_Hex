using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace UART
{
    class DataProcess
    {
        private static string rx_Command = @"\b[0-9a-fA-F]{1,8}\b";//match  bit Hex
        private static bool CheckLegal(Regex rx, string rxString)
        {
            return rx.IsMatch(rxString);
        }
        private static byte[] _HexStringToByteArray(string s)
        {
            s.Replace(" ", "");
            int ByteLength;
            if(s.Length%2 != 0)
            {
                ByteLength = s.Length / 2 +1;
            }
            else
            {
                ByteLength = s.Length / 2;
            }
            byte[] buffer = new byte[ByteLength];
            if (s.Length % 2 != 0)
            {
                //buffer[ByteLength - 1] = 0;
                for (int i = 0; i < s.Length; i += 2)
                    if ((s.Length - i) == 1)
                        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 1), 16);
                    else
                        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            else
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }
        public static bool HexStringToByteArray(string HexString, out byte[] HexByte)
        {
            Regex rx_ByteHex = new Regex(rx_Command);
            if (!CheckLegal(rx_ByteHex, HexString))
            {
                HexByte = null;
                return false;
            }
            else
            {
                HexByte = _HexStringToByteArray(HexString);
                return true;
            }
        }
    }
}
