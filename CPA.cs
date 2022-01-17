using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

using System.Collections;
public class CPA
{
    static int NS = 20000;
    private static byte[][] plainTexts = new byte[NS][];
    private static byte[][] cipherTexts = new byte[NS][];
    private static Matrix<Double> dataTraces;
    private static Matrix<Double> hypothesis;
    public static void init(String fileName)
    {
        readTrs(fileName,  ref plainTexts, ref cipherTexts, ref dataTraces);
    }
     public static void readTrs(String fileName, ref byte[][] plainTexts, ref byte[][] cipherTexts, ref Matrix<Double> dataTraces)
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
            Double[,] dataTracesArray = new Double[20000, 3500];
            for (int i = 0; i < NT; i++)
            {
                plainTexts[i] = new byte[8];
                cipherTexts[i] = new byte[8];
                fs.Read(plainTexts[i], 0, 8);
                fs.Read(cipherTexts[i], 0, 8);
                for (j = 0; j < NS; j++)
                {
                    fs.Read(samplebuffer, 0, 2);
                    sampleValue = BitConverter.ToInt16(samplebuffer, 0);
                    dataTracesArray[i, j] = sampleValue;
                }
            }
            var dt = Matrix<Double>.Build;
            dataTraces= dt.DenseOfArray(dataTracesArray);
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
         
        Double[,] hypothesisArray = new double[20000, 64];
        byte[] LRIndices = DES.DES.getLRIndices(sboxNumber);
        //Utils.printBuffer(plainTexts[0]);
        for (int i = 0; i <cipherTexts.Length; i++)
        {
            var IPL = DES.DES.getIPL(plainTexts[i]);
            var IPR = DES.DES.getIPR(plainTexts[i]);
            var extended = DES.DES.ETrans(IPR);
            var IPLCharacBits = DES.DES.extract(IPL, DES.DES.getLRIndices(sboxNumber));
            var IPRCharacBIts = DES.DES.extract(IPR, DES.DES.getLRIndices(sboxNumber));

            for (int j = 0; j < 64; j++)
            {
                var sboxInput = DES.DES.extract(extended, DES.DES.getConsecutive6Indices(sboxNumber));
                int sboxInputInt = bitArrayToInt(sboxInput);
                sboxInputInt = sboxInputInt ^ j;
                var sboxOutput = DES.DES.getSBoxContent(sboxNumber, sboxInputInt);
                hypothesisArray[i, j] = bitCount(sboxOutput ^ bitArrayToInt(IPLCharacBits) ^ bitArrayToInt(IPRCharacBIts));

            }
        }
        var matrixBuilder = Matrix<Double>.Build;
        hypothesis = matrixBuilder.DenseOfArray(hypothesisArray);
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
        DateTime beforeDT;
        DateTime afterDT;
        TimeSpan ts;
        //var rowSum = hypothesis.RowSums();

        //for(int i = 0; i < rowSum.ToRowMatrix().ColumnCount; i++)
        //    if (rowSum[i] != 128)
        //        Console.WriteLine(rowSum[i]);
        //var columnSum = hypothesis.ColumnSums();

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

        for (var i = 0; i < hypothesis.RowCount; i++)
        {
            hypColMean.SetRow(i, hypColumnmean);
        }



        for (var i = 0; i < dataTraces.RowCount; i++)
        {
            dtColMean.SetRow(i, dtColumnmean);
        }
        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("compute A-mA, B-mB:duplicate  using {0} s", ts);

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
        foreach (var col in corr.EnumerateColumns())
        {
            if(col.AbsoluteMaximum()==max)
                Console.WriteLine("{0:X2}", col.AbsoluteMaximumIndex());
        }
        Console.WriteLine(max);
        afterDT = System.DateTime.Now;
        ts = afterDT.Subtract(beforeDT);
        Console.WriteLine("find index using {0} s", ts);
    }
}
