using System;
using System.Text;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

/// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
/// <param name="s"> The string containing the hex digits (with or without spaces). </param>
/// <returns> Returns an array of bytes. </returns>
byte[] HexStringToByteArray(string s)
{
    s = s.Replace(" ", "");
    byte[] buffer = new byte[s.Length / 2];
    for (int i = 0; i < s.Length; i += 2)
    {
        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
    }

    return buffer;
}

/// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
/// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
/// <returns> Returns a well formatted string of hex digits with spacing. </returns>
string ByteArrayToHexString(byte[] data)
{
    Array.Reverse(data);
    StringBuilder sb = new StringBuilder(data.Length * 3);
    foreach (byte b in data)
    {
        sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
    }

    return sb.ToString().ToUpper();
}

// 2d byte array
void processTrs(ref byte[][] plainTexts, ref byte[][] cipherTexts)
{
    Console.WriteLine("hello world!");
}

// 2d byte array
void printhello()
{
    Console.WriteLine("hello world!");
}

void test_matrix_operation()
{
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 create a dense matrix with 3 rows and 4 columns                  -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // create a dense matrix with 3 rows and 4 columns
    // filled with random numbers sampled from the standard distribution
    Matrix<double> m = Matrix<double>.Build.Random(3, 4);
    Console.WriteLine(m);
    
    
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 create a dense zero-vector of length 10                          -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // create a dense zero-vector of length 10
    Vector<double> v = Vector<double>.Build.Dense(10);
    Console.WriteLine(v);
    
    var M = Matrix<double>.Build;
    var V = Vector<double>.Build;

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 create matrix builder and vector builder                         -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine(M);
    Console.WriteLine(V);
    

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 3x4 dense matrix filled with zeros                               -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // 3x4 dense matrix filled with zeros
    Console.WriteLine(M.Dense(3, 4));
    
    
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 3x4 dense matrix filled with 1.0.                                -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // 3x4 dense matrix filled with 1.0.
    M.Dense(3, 4, 1.0);
    Console.WriteLine(M.Dense(3, 4, 1.0));
   
    
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 3x4 dense matrix where each field is initialized using a function-");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // 3x4 dense matrix where each field is initialized using a function
    Console.WriteLine(M.Dense(3, 4, (i, j) => 100 * i + j));
    
    
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 3x4 square dense matrix with each diagonal value set to 2.0      -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // 3x4 square dense matrix with each diagonal value set to 2.0
    Console.WriteLine(M.DenseDiagonal(3, 4, 2.0));
    
    
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 3x3 dense identity matrix                                        -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // 3x3 dense identity matrix
    Console.WriteLine(M.DenseIdentity(3));
    
    Console.WriteLine();
    Console.WriteLine();
}

void readTrsHead()
{

}

void printBuffer(byte [] data)
{
    String hexStr = ByteArrayToHexString(data);
    Console.WriteLine(hexStr);
}

void readTrs(String fileName, ref byte[][] plainTexts, ref byte[][] cipherTexts, ref Double[,] dataTraces)
{
    //FileStream fileStream = new FileStream(@"E:\2021Fall\work\DES\CorrelationPowerAnalysis-master\traceset.trs", FileMode.Open);
    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    {
        try
        {
            //byte[] buffer = new byte[fs.Length];
            byte [] bytebuffer = new byte[1];
            byte [] shortbuffer = new byte[2];
            byte [] intbuffer = new byte[4];
            
            // get the number of traces
            fs.Seek(2, SeekOrigin.Begin);
            fs.Read(intbuffer, 0, 4);
            //Array.Reverse(intbuffer);
            int NT = BitConverter.ToInt32(intbuffer, 0);
            Console.WriteLine("Number of traces: {0}", NT);
            //printBuffer(intbuffer);
   
            // get the number of sample points          
            fs.Seek(2, SeekOrigin.Current);
            fs.Read(intbuffer, 0, 4);
            int NS = BitConverter.ToInt32(intbuffer, 0);
            Console.WriteLine("Number of Sample points: {0}", NS);
            //printBuffer(intbuffer);

            fs.Seek(2, SeekOrigin.Current);
            fs.Read(bytebuffer, 0, 1);
            //int sampleCoding = BitConverter.ToInt8(bytebuffer, 0);
            if (bytebuffer[0] == 1)
            {
                Console.WriteLine("every sample is coded in 1 byte");
            }
            else if(bytebuffer[0] == 2)
            {
                Console.WriteLine("every sample is coded in 2 byte");
            }
            else if(bytebuffer[0] == 4)
            {
                Console.WriteLine("every sample is coded in 4 byte");
            }
            else
            {
                //exit(1);
            }
            byte [] samplebuffer = new byte[bytebuffer[0]];

            fs.Seek(2, SeekOrigin.Current);
            fs.Read(shortbuffer, 0, 2);
            int DS = BitConverter.ToInt16(shortbuffer, 0);

            int sampleValue, j;
            fs.Seek(2, SeekOrigin.Current);
            for (int i = 0; i < NT; i++)
            {
                fs.Read(plainTexts[i], 0, 8);
                fs.Read(cipherTexts[i], 0, 8);
                //printBuffer(plainTexts[i]);
                //printBuffer(cipherTexts[i]);
                //if (i == 1) break;
                Array.Reverse(plainTexts[i]);
                Array.Reverse(cipherTexts[i]);
                for(j = 0; j < NS; j++)
                {
                    fs.Read(samplebuffer, 0, 2);
                    sampleValue = BitConverter.ToInt16(samplebuffer, 0);
                    dataTraces[i,j] = sampleValue;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}

void main()
{
    //test_matrix_operation();
    //byte[][] plainTexts = new byte[20000][];
    //byte[][] cipherTexts = new byte[20000][];
    //String fileName = @"E:\2021Fall\work\DES\CorrelationPowerAnalysis-master\traceset.trs";

    //for (int i = 0; i < plainTexts.Length; i++)
    //{
    //    plainTexts[i] = new byte[8];
    //    cipherTexts[i] = new byte[8];
    //}

    //var dataTraces = Matrix<Double>.Build;
    ////dataTraces.Dense(20000, 3500, 0);

    //Double[,] dataTracesArray = new Double[20000, 3500];
    //readTrs(fileName, ref plainTexts, ref cipherTexts, ref dataTracesArray);

    //for (int i = 0;i < 10; i++)
    //    //for (int i = 0;i < plainTexts.Length; i++)
    //{
    //    //plainTexts[i].Reverse();
    //    printBuffer(plainTexts[i]);
    //    printBuffer(cipherTexts[i]);
    //}
    //var dt = dataTraces.DenseOfArray(dataTracesArray);

    //Console.WriteLine(dt.ToString());

    //DES.DES.main();
    //DES.
    DateTime dt1, dt2;
    String fileName = @"E:\2021Fall\work\DES\CorrelationPowerAnalysis-master\traceset.trs";
    DateTime beforeDT;
    DateTime afterDT;

    beforeDT = DateTime.Now;
    CPA.init(fileName);
    afterDT = DateTime.Now;
    TimeSpan ts; //= afterDT - beforeDT;

    for (var i= 1; i < 9; i++)
    {
        
        dt1 = DateTime.Now;
        CPA.initDesHypothesis(i);
        dt2 = DateTime.Now;
        ts = dt1 - dt2;
        Console.WriteLine("init hpythesi Eclapsed {0} s\n", ts);
        
        dt1 = DateTime.Now;
        CPA.corRelate();
        dt2 = DateTime.Now;
        ts = dt1 - dt2;
        Console.WriteLine("correlate Eclapsed {0} s\n\n\n",ts);

    }
}

main();
