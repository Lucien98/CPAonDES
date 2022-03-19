using System;
using System.Text;
using System.Collections;
public class Utils
{
    /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
    /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
    /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
    public static string ByteArrayToHexString(byte[] data)
    {
        //Array.Reverse(data);
        StringBuilder sb = new StringBuilder(data.Length * 3);
        foreach (byte b in data)
        {
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }

        return sb.ToString().ToUpper();
    }

    public static void printBuffer(byte[] data)
    {
        foreach(byte b in data.Reverse())
        {
            Console.WriteLine(b);
        }
    }
    /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
    /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
    /// <returns> Returns an array of bytes. </returns>
    public static byte[] HexStringToByteArray(string s)
    {
        s = s.Replace(" ", "");
        byte[] buffer = new byte[s.Length / 2];
        for (int i = 0; i<s.Length; i += 2)
        {
            buffer[i / 2] = (byte) Convert.ToByte(s.Substring(i, 2), 16);
        }

        return buffer;
    }

}
