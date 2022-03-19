using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

using System.Collections;
public class CPAv2
{
    private int blockSize = 8;
    private int attackTracesNumber = 0;

    private List<String> plainTextsList;
    private List<String> cipherTextsList;

    private Matrix<Double> dataTraces;
    private Matrix<Double> hypothesis; //numberOfTraces * 64
    private Matrix<Double> correlationMatrix;
    TrsWorker trsWorker;
    private Vector<Double> nthTracePointsVec;
    private byte[] key;
    private BitArray Round1Key = new BitArray(48);
    Vector<Double> ResA2ColumnSumVec;
    Vector<Double> ResB2ColumnSumVec;
    Vector<Double> ResAaverageVec;
    Vector<Double> ResBaverageVec;
    Vector<Double> ResAColumnSumsVec;
    Vector<Double> ResBColumnSumsVec;
    Vector<Double> ResAvarianceVec;
    Vector<Double> ResBvarianceVec;
    Matrix<Double> X;
    //Vector<Double> A2;
    //Vector<Double> B2;
    public CPAv2(String fileName)
    {
        trsWorker = new TrsWorker(fileName);
        int NT = trsWorker.getTracesNumber();
        hypothesis = Matrix<double>.Build.Dense(NT, 8 * blockSize);
        
        int NS = trsWorker.getSampleNumber();
        ResA2ColumnSumVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResB2ColumnSumVec = Vector<Double>.Build.Dense(NS);
        
        ResAaverageVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBaverageVec = Vector<Double>.Build.Dense(NS);
        ResAColumnSumsVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBColumnSumsVec = Vector<Double>.Build.Dense(NS);

        ResAvarianceVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBvarianceVec = Vector<Double>.Build.Dense(NS);
        X = Matrix<Double>.Build.Dense(8*blockSize, NS);
        //A2 = Vector<Double>.Build.Dense(8 * blockSize);
        //B2 = Vector<Double>.Build.Dense(NS);
    }


    private void initMatrix()
    {
        int NS = trsWorker.getSampleNumber();

        ResA2ColumnSumVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResB2ColumnSumVec = Vector<Double>.Build.Dense(NS);

        ResAaverageVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBaverageVec = Vector<Double>.Build.Dense(NS);
        ResAColumnSumsVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBColumnSumsVec = Vector<Double>.Build.Dense(NS);

        ResAvarianceVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBvarianceVec = Vector<Double>.Build.Dense(NS);
        X = Matrix<Double>.Build.Dense(8 * blockSize, NS);

    }
    private int bitCount(int i)
    {
        int num = 0;
        while (i != 0)
        {
            i &= (i - 1);
            num++;
        }
        return num;
    }

    private int bitArrayToInt(BitArray bit)
    {
        int[] res = new int[1];
        for (int i = 0; i < bit.Count; i++)
        {
            bit.CopyTo(res, 0);
        }
        return res[0];
    }
    private void computeDES1stRoundResult()
    {
        for(int i = 0; i < plainTextsList.Count; i++)
        {
            //DES.DES.printBitArray(Round1Key);
            plainTextsList[i] = DES.DES.get1stRoundResWithSubKey1(Round1Key, Utils.HexStringToByteArray(plainTextsList[i]));
        }
    }

    public Matrix<Double> initDesHypothesis(int sboxNumber, int round, string method, int start, int N)
    {
        if (method == "CPA")
        {
            Matrix<Double> hypothesis1 = Matrix<double>.Build.Dense(N, 8 * blockSize);
            if (round == 2)
            {
                //byte[] keyBytes = { 0x5c, 0x91, 0x5e, 0x6c, 0x61, 0x6a, 0xa0, 0xf2 };
                byte[] keyBytes = { 0x41, 0x7a, 0x8f, 0x9f, 0x6a, 0x2b, 0x1c, 0x7d };
                var subkeys = DES.DES.CreateSubKeys(keyBytes);
                Round1Key = subkeys[0];
                DES.DES.printBitArray(Round1Key);
            }


            if (round == 1) plainTextsList = trsWorker.extractPlainTextsList(start, N);
            else if (round == 2) computeDES1stRoundResult();
            byte[] LRIndices = DES.DES.getLRIndices(sboxNumber);

            for (int i = 0; i < plainTextsList.Count ; i++)
            {

                var IPL = DES.DES.getIPL(Utils.HexStringToByteArray(plainTextsList[i]), round);
                var IPR = DES.DES.getIPR(Utils.HexStringToByteArray(plainTextsList[i]), round);


                var extended = DES.DES.ETrans(IPR);
                var IPLCharacBits = DES.DES.extract(IPL, DES.DES.getLRIndices(sboxNumber));
                var IPRCharacBIts = DES.DES.extract(IPR, DES.DES.getLRIndices(sboxNumber));

                for (int j = 0; j < 64; j++)
                {
                    var sboxInput = DES.DES.extract(extended, DES.DES.getConsecutive6Indices(sboxNumber));
                    int sboxInputInt = bitArrayToInt(sboxInput);
                    sboxInputInt = sboxInputInt ^ j;
                    var sboxOutput = DES.DES.getSBoxContent(sboxNumber, sboxInputInt);
                    hypothesis1[i, j] = bitCount(sboxOutput ^ bitArrayToInt(IPLCharacBits) ^ bitArrayToInt(IPRCharacBIts));
                }
            }
            return hypothesis1;

        }
        else
        {
            //need to change with start and N staffs
            Matrix<Double> hypothesis1 = Matrix<double>.Build.Dense(N, 8 * blockSize);
            if (round == 1) plainTextsList = trsWorker.extractPlainTextsList(start, N);
            else if (round == 2) computeDES1stRoundResult();
            byte[] LRIndices = { DES.DES.getLRIndices(sboxNumber)[0] };

            for (int i = start; i < plainTextsList.Count && i < start + N; i++)
            {

                var IPL = DES.DES.getIPL(Utils.HexStringToByteArray(plainTextsList[i]), round);
                var IPR = DES.DES.getIPR(Utils.HexStringToByteArray(plainTextsList[i]), round);


                var extended = DES.DES.ETrans(IPR);
                var IPLCharacBits = DES.DES.extract(IPL, DES.DES.getLRIndices(sboxNumber));
                var IPRCharacBIts = DES.DES.extract(IPR, DES.DES.getLRIndices(sboxNumber));

                for (int j = 0; j < 64; j++)
                {
                    var sboxInput = DES.DES.extract(extended, DES.DES.getConsecutive6Indices(sboxNumber));
                    int sboxInputInt = bitArrayToInt(sboxInput);
                    sboxInputInt = sboxInputInt ^ j;
                    var sboxOutput = DES.DES.getSBoxContent(sboxNumber, sboxInputInt);
                    hypothesis1[i-start, j] = bitCount(sboxOutput ^ bitArrayToInt(IPLCharacBits) ^ bitArrayToInt(IPRCharacBIts));
                }
            }
            return hypothesis1;

        }

    }
    public void computeSinglePointCorrelation(int N, string method)
    {
        if(method == "CPA")
        {
            var nthTracePointsVec = trsWorker.extractNthTracePointsVec(N);

            for (int i = 0; i < hypothesis.ColumnCount; i++)
            {
                correlationMatrix[i, N] = Correlation.Pearson(hypothesis.Column(i), nthTracePointsVec);
            }

        }
        else
        {
            var nthTracePointsVec = trsWorker.extractNthTracePointsVec(N);


            for (int i = 0; i < hypothesis.ColumnCount; i++)
            {
                var average1 = nthTracePointsVec.PointwiseMultiply(hypothesis.Column(i))/hypothesis.Column(i).Sum();
                var average0 = nthTracePointsVec.PointwiseMultiply(1-hypothesis.Column(i))/(1- hypothesis.Column(i)).Sum();
                correlationMatrix[i, N] = (average0 - average1).Sum();
                //correlationMatrix[i, N] = Correlation.Pearson(hypothesis.Column(i), nthTracePointsVec);
            }

        }
    }

    public void computeCorrelationMatrix(string method)
    {
        int NS = trsWorker.getSampleNumber();
        // size of correlationMatrix: 64 * numberOfSamples
        //var tasks = new Task[NS];
        correlationMatrix = Matrix<Double>.Build.Dense(hypothesis.ColumnCount, NS);
        ////Console.WriteLine(hypothesis);
        for (int i = 0; i < NS; i++)
        {
            //    var k = i;
            //    //if (i % 100 == 0) Console.WriteLine("computing {0}th point...", i);
            //    tasks[i] = Task.Run(() =>
            //    {
            computeSinglePointCorrelation(i, method);

            //    });
        }
        //Task.WaitAll(tasks);
    }
    public void analyse(string method)
    {
        DateTime beforeDT;
        DateTime afterDT;
        TimeSpan ts;

        key = new byte[blockSize];
        //plainTextsList = trsWorker.extractPlainTextsList();

        for (int N = 1; N < blockSize + 1; N++)
        //for (int N = 1; N < blockSize + 1; N++)
        {
            //byte[] keyBytes = { 0x41, 0x7a, 0x8f, 0x9f, 0x6a, 0x2b, 0x1c, 0x7d };

            //var subkeys = DES.DES.CreateSubKeys(keyBytes);
            //Round1Key = subkeys[0];
            //DES.DES.printBitArray(Round1Key);
            initMatrix();
            attackTracesNumber = 0;
            for (int round = 2; round <= 2; round++)
            {
                //for (int i = 0; i < 1; i++)
                //{
                //    if (round == 1)
                //    {
                //        initDesHypothesis(N, round, method, 10000 * i, 10000);
                //        continue;
                //    }
                //}
                for (int i = 0; i < 1; i++)
                {
                    Console.WriteLine(i);
                    beforeDT = DateTime.Now;
                    initDesHypothesis(N, 1, method, 10000 * i, 10000);
                    var hypothesis2 = initDesHypothesis(N, round, method, 10000 * i, 10000);
                    afterDT = System.DateTime.Now;
                    ts = afterDT.Subtract(beforeDT);
                    Console.WriteLine("init hypothesis done! Elsped {0} s", ts);

                    beforeDT = DateTime.Now;
                
                        dataTraces = trsWorker.extractNTracesMatrix(10000*i, 10000);
                        correlate2(hypothesis2);

                
                    //dataTraces = trsWorker.extractNTracesMatrix(0, 20000);
                    //correlate2();
                    //computeCorrelationMatrix(method);
                    afterDT = System.DateTime.Now;
                    ts = afterDT.Subtract(beforeDT);
                    Console.WriteLine("init correlation matrix done! Elapsed {0} s", ts);
                }
                Vector<Double> absMaxOfEveryRow = Vector<Double>.Build.Dense(hypothesis.ColumnCount);
                for (int i = 0; i < absMaxOfEveryRow.Count; i++)
                {
                    absMaxOfEveryRow[i] = correlationMatrix[i, correlationMatrix.Row(i).AbsoluteMaximumIndex()];
                }
                key[N - 1] = Convert.ToByte(absMaxOfEveryRow.AbsoluteMaximumIndex());
                Console.WriteLine("{0:X2}", key[N - 1]);
                for (var i = 0; i < 6; i++)
                {
                    Round1Key[6 * (8 - N) + i] = Convert.ToBoolean(key[N - 1] >> i & 1);
                }
            }
        }
        DES.DES.printBitArray(Round1Key);
        trsWorker.died();
    }

    public void correlate2(Matrix<Double> hypothesis2)
    {
        int NS = trsWorker.getSampleNumber();
        correlationMatrix = correlate(hypothesis2, dataTraces);
        
    }

    private Matrix<Double> correlate(Matrix<Double> A, Matrix<Double> B)
    {
        attackTracesNumber += A.RowCount;
        //Console.WriteLine(attackTracesNumber);
        Vector<Double> A2ColumnSumVec = A.PointwisePower(2).ColumnSums();
        Vector<Double> B2ColumnSumVec = B.PointwisePower(2).ColumnSums();

        ResA2ColumnSumVec += A2ColumnSumVec;
        ResB2ColumnSumVec += B2ColumnSumVec;

        Vector<Double> AaverageVec = A.ColumnSums() / A.RowCount;
        Vector<Double> BaverageVec = B.ColumnSums() / B.RowCount;
        Vector<Double> AColumnSumsVec = A.ColumnSums();
        Vector<Double> BColumnSumsVec = B.ColumnSums();

        ResAColumnSumsVec += AColumnSumsVec;
        ResBColumnSumsVec += BColumnSumsVec;

        //Vector<Double> AvarianceVec = A2ColumnSumVec - A.RowCount * (AaverageVec.PointwisePower(2));
        //Vector<Double> BvarianceVec = B2ColumnSumVec - B.RowCount * (BaverageVec.PointwisePower(2));
        Vector<Double> AvarianceVec = ResA2ColumnSumVec - (AColumnSumsVec.PointwisePower(2)) / attackTracesNumber;
        Vector<Double> BvarianceVec = ResB2ColumnSumVec - (BColumnSumsVec.PointwisePower(2)) / attackTracesNumber;



        X += A.Transpose()*B;
        //Matrix<Double> Y = A.RowCount*(AaverageVec.ToColumnMatrix()*BaverageVec.ToRowMatrix());
        Matrix<Double> Y = (ResAColumnSumsVec.ToColumnMatrix() * ResBColumnSumsVec.ToRowMatrix()) / attackTracesNumber;

        Matrix<Double> Z = AvarianceVec.ToColumnMatrix() * BvarianceVec.ToRowMatrix();
        Z = Z.PointwiseSqrt();
        return (X-Y).PointwiseDivide(Z);
    }
    public void corRelate()
    {
        correlationMatrix = Matrix<Double>.Build.Dense(hypothesis.ColumnCount, dataTraces.ColumnCount);
        DateTime beforeDT;
        DateTime afterDT;
        TimeSpan ts;

        int i = 0, j = 0;
        foreach (var hypCol in hypothesis.EnumerateColumns())
        {
            j = 0;
            beforeDT = DateTime.Now;
            foreach (var dtCol in dataTraces.EnumerateColumns())
            {
                beforeDT = System.DateTime.Now;

                correlationMatrix[i, j] = Correlation.Pearson(hypCol, dtCol);
                j++;
                afterDT = System.DateTime.Now;
                ts = afterDT.Subtract(beforeDT);
                if (i == 0 && j == 0) Console.WriteLine("compute solumn mean using {0} s", ts);
            }
            i++;
            afterDT = System.DateTime.Now;

            ts = afterDT.Subtract(beforeDT);
            if (i == 1) Console.WriteLine("compute solumn mean using {0} s", ts);
        }
        var max = Statistics.MaximumAbsolute(correlationMatrix.Enumerate());
        foreach (var col in correlationMatrix.EnumerateColumns())
        {
            if (col.AbsoluteMaximum() == max)
                Console.WriteLine("{0:X2}", col.AbsoluteMaximumIndex());
        }
        Console.WriteLine(max);
    }
}
