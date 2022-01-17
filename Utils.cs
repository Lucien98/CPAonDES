using System;
using System.Text;

public class Utils
{
    /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
    /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
    /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
    public static string ByteArrayToHexString(byte[] data)
    {
        //Array.Reverse(data);
        StringBuilder sb = new StringBuilder(data.Length * 3);
        foreach (byte b in data.Reverse())
        {
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }

        return sb.ToString().ToUpper();
    }

    public static void printBuffer(byte[] data)
    {
        String hexStr = ByteArrayToHexString(data);
        Console.WriteLine(hexStr);
    }

}
