using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

using System.Collections;
public class CPA
{
    private static int blockSize = 8;
    
    private static List<String> plainTextsList = new List<string>();
    private static List<String> cipherTextsList = new List<string>();

    private static Matrix<Double> dataTraces;
    private static Matrix<Double> hypothesis;
    private static Matrix<Double> correlationMatrix;
    private static String fname;
    public static void init(String fileName)
    {
        readTrs(fileName);
        fname = fileName;
    }


    public static void readTrs(String fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            byte[] bytebuffer = new byte[1];
            byte[] shortbuffer = new byte[2];
              byte[] intbuffer = new byte[4];

            // get the number of traces
            fs.Seek(2, SeekOrigin.Begin);
            fs.Read(intbuffer, 0, 4);
          
            int NT = BitConverter.ToInt32(intbuffer, 0);
            Console.WriteLine("Number of traces: {0}", NT);
            
            // get the number of sample points          
            fs.Seek(2, SeekOrigin.Current);
            fs.Read(intbuffer, 0, 4);
            int NS = BitConverter.ToInt32(intbuffer, 0);
            Console.WriteLine("Number of Sample points: {0}", NS);
           
            fs.Seek(2, SeekOrigin.Current);
            fs.Read(bytebuffer, 0, 1);
            
            if (bytebuffer[0] == 1)
            {
                Console.WriteLine("every sample is coded in 1 byte");
            }
            else if (bytebuffer[0] == 2)
            {
                Console.WriteLine("every sample is coded in 2 byte");
            }
            else if (bytebuffer[0] == 4)
            {
                Console.WriteLine("every sample is coded in 4 byte");
            }
            else
            {
                Console.WriteLine("Invalid SampleCoding");
                Environment.Exit(0);
            }
            byte[] samplebuffer = new byte[bytebuffer[0]];

            fs.Seek(2, SeekOrigin.Current);
            fs.Read(shortbuffer, 0, 2);
            int DS = BitConverter.ToInt16(shortbuffer, 0);

            int sampleValue, j;
            fs.Seek(2, SeekOrigin.Current);
            dataTraces = Matrix<double>.Build.Dense(NT, NS);
            hypothesis = Matrix<double>.Build.Dense(NT, 8*blockSize);
            byte[] plainText = new byte[blockSize];
            byte[] cipherText = new byte[blockSize];

            for (int i = 0; i < NT; i++)
            {
                fs.Read(plainText, 0, blockSize);
                fs.Read(cipherText, 0, blockSize);

                plainTextsList.Add(Utils.ByteArrayToHexString(plainText));
                cipherTextsList.Add(Utils.ByteArrayToHexString(cipherText));
                int start = 0;
                int end = 3500;
                fs.Seek(start * bytebuffer[0], SeekOrigin.Current);
                for (j = start; j < end; j++)
                {
                    fs.Read(samplebuffer, 0, 2);
                    sampleValue = BitConverter.ToInt16(samplebuffer, 0);
                    dataTraces[i, j-start] = sampleValue;
                }
                fs.Seek((NS - end) * bytebuffer[0], SeekOrigin.Current);
            }
        }

    }
    public static int bitCount(int i)
    {
        int num = 0;
        while (i != 0)
        {
            i &= (i - 1);
            num++;
        }
        return num;
    }

    public static int bitArrayToInt(BitArray bit)
    {
        int[] res = new int[1];

        for (int i = 0; i < bit.Count; i++)
        {
            bit.CopyTo(res, 0);
        }

        return res[0];
    }
    public static void initDesHypothesis(int sboxNumber)
    {
        
        byte[] LRIndices = DES.DES.getLRIndices(sboxNumber);
        
        for (int i = 0; i <plainTextsList.Count; i++)
        {
            var IPL = DES.DES.getIPL(Utils.HexStringToByteArray(plainTextsList[i]), 0);
            var IPR = DES.DES.getIPR(Utils.HexStringToByteArray(plainTextsList[i]), 0);
            var extended = DES.DES.ETrans(IPR);
            var IPLCharacBits = DES.DES.extract(IPL, DES.DES.getLRIndices(sboxNumber));
            var IPRCharacBIts = DES.DES.extract(IPR, DES.DES.getLRIndices(sboxNumber));

            for (int j = 0; j < 64; j++)
            {
                var sboxInput = DES.DES.extract(extended, DES.DES.getConsecutive6Indices(sboxNumber));
                int sboxInputInt = bitArrayToInt(sboxInput);
                sboxInputInt = sboxInputInt ^ j;
                var sboxOutput = DES.DES.getSBoxContent(sboxNumber, sboxInputInt);
                hypothesis[i, j] = bitCount(sboxOutput ^ bitArrayToInt(IPLCharacBits) ^ bitArrayToInt(IPRCharacBIts));

            }
        }
        CPAv2 cpa = new CPAv2(fname);
        //hypothesis = cpa.initDesHypothesis(sboxNumber, 1, "CPA");
        //hypothesis = cpa.initDesHypothesis(sboxNumber, 2, "CPA");
        //Console.WriteLine(hypothesis);

    }
    public static Matrix<Double> colSDMatrix(Matrix<Double> matrix)
    {
        Matrix<Double> ss = Matrix<Double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
        matrix.PointwisePower(2, ss);

        var Columnmean = ss.ColumnSums() / ss.RowCount;
        Columnmean.PointwiseSqrt();
    
        Matrix<Double> colMean = Matrix<Double>.Build.Dense(ss.RowCount, ss.ColumnCount);

        for (var i = 0; i < ss.RowCount; i++)
        {
            colMean.SetRow(i, Columnmean);
        }
        return colMean;
    }
    public static void corRelate()
    {
        //correlationMatrix = Matrix<Double>.Build.Dense(hypothesis.ColumnCount, dataTraces.ColumnCount);
        //DateTime beforeDT;
        //DateTime afterDT;
        //TimeSpan ts;

        //int i = 0, j = 0;
        //foreach (var hypCol in hypothesis.EnumerateColumns())
        //{
        //    j = 0;
        //    beforeDT = DateTime.Now;
        //    foreach(var dtCol in dataTraces.EnumerateColumns())
        //    {
        //        beforeDT = System.DateTime.Now;

        //        correlationMatrix[i, j] = Correlation.Pearson(hypCol, dtCol);
        //        j++;
        //        afterDT = System.DateTime.Now;
        //        ts = afterDT.Subtract(beforeDT);
        //        if (i == 0 && j == 0) Console.WriteLine("compute solumn mean using {0} s", ts);
        //    }
        //    i++;
        //    afterDT = System.DateTime.Now;

        //    ts = afterDT.Subtract(beforeDT);
        //    if (i == 1) Console.WriteLine("compute solumn mean using {0} s", ts);
        //}
        //var max = Statistics.MaximumAbsolute(correlationMatrix.Enumerate());
        //foreach (var col in correlationMatrix.EnumerateColumns())
        //{
        //    if (col.AbsoluteMaximum() == max)
        //        Console.WriteLine("{0:X2}", col.AbsoluteMaximumIndex());
        //}
        //Console.WriteLine(max);

        //Console.WriteLine("find index using {0} s", ts);
        DateTime beforeDT;
        DateTime afterDT;
        TimeSpan ts;

        beforeDT = System.DateTime.Now;

        var hypColumnmean = hypothesis.ColumnSums() / hypothesis.RowCount;
        Matrix<Double> hypColMean = Matrix<Double>.Build.Dense(hypothesis.RowCount, hypothesis.ColumnCount);
        var dtColumnmean = dataTraces.ColumnSums() / dataTraces.RowCount;
        Matrix<Double> dtColMean = Matrix<Double>.Build.Dense(dataTraces.RowCount, dataTraces.ColumnCount);
        //int index = 0;
        //foreach (var row in hypothesis.EnumerateRows()) 
        //{
        //    hypothesis.SetRow(index++, row - hypColumnmean);
        //}
        //index = 0;
        //foreach(var row in dataTraces.EnumerateRows())
        //{
        //    dataTraces.SetRow(index++, row - dtColumnmean);
        //}
        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("compute solumn mean using {0} s", ts);


        beforeDT = System.DateTime.Now;
        int i, j = 0;
        for (i = 0; i < hypothesis.RowCount; i++)
        {
            hypColMean.SetRow(i, hypColumnmean);
        }



        for (i = 0; i < dataTraces.RowCount; i++)
        {
            dtColMean.SetRow(i, dtColumnmean);
        }
        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("compute A-mA, B-mB:duplicate  using {0} s", ts);

        //Console.WriteLine(hypothesis);
        var A_mA = hypothesis - hypColMean;
        var B_mB = dataTraces - dtColMean;
        //var A_mA = hypothesis;
        //var B_mB = dataTraces;


        beforeDT = System.DateTime.Now;

        var ssA1 = colSDMatrix(A_mA);
        var ssB1 = colSDMatrix(B_mB);

        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("compute ssA, ssB using {0} s", ts);
        beforeDT = System.DateTime.Now;

        var corr = (A_mA.Transpose() * B_mB).PointwiseDivide(ssA1.Transpose() * ssB1);

        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("compute corr using {0} s", ts);

        beforeDT = System.DateTime.Now;
        var max = Statistics.MaximumAbsolute(corr.Enumerate());
        //foreach (var col in corr.EnumerateColumns())
        //{
        //    if (col.AbsoluteMaximum() == max)
        //        Console.WriteLine("{0:X2}", col.AbsoluteMaximumIndex());
        //}
        //Console.WriteLine(max);
        //int [] max_point= new int[64];
        Vector<Double> absMaxOfEveryRow = Vector<Double>.Build.Dense(hypothesis.ColumnCount);
        for (i = 0; i < absMaxOfEveryRow.Count; i++)
        {
            absMaxOfEveryRow[i] = corr[i, corr.Row(i).AbsoluteMaximumIndex()];

            //Console.WriteLine("{0},{1}, {2}", i, absMaxOfEveryRow[i], corr.Row(i).AbsoluteMaximumIndex());
        }
        var key = Convert.ToByte(absMaxOfEveryRow.AbsoluteMaximumIndex());
        //absMaxOfEveryRow[absMaxOfEveryRow.AbsoluteMaximumIndex()] = 0;
        key = Convert.ToByte(absMaxOfEveryRow.AbsoluteMaximumIndex()); 
        Console.WriteLine("{0:X2}, {1}", key, corr.Row(key).AbsoluteMaximumIndex());

        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("find index using {0} s", ts);
    }
    public static void test()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(plainTextsList[i]);
        }
        Console.WriteLine(dataTraces);
    }
}
