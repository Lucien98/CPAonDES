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
    Vector<Double> ResAColumnSumsVec;
    Vector<Double> ResBColumnSumsVec;
    Vector<Double> ResOneMinusAColumnSumsVec;
    Matrix<Double> X;
    Matrix<Double> W;
    public CPAv2(String fileName)
    {
        trsWorker = new TrsWorker(fileName);
        int NT = trsWorker.getTracesNumber();
        hypothesis = Matrix<double>.Build.Dense(NT, 8 * blockSize);
        initMatrix();
    }

    private void initMatrix()
    {
        int NS = trsWorker.getSampleNumber();

        ResA2ColumnSumVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResB2ColumnSumVec = Vector<Double>.Build.Dense(NS);
        ResAColumnSumsVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResOneMinusAColumnSumsVec = Vector<Double>.Build.Dense(8 * blockSize);
        ResBColumnSumsVec = Vector<Double>.Build.Dense(NS);

        X = Matrix<Double>.Build.Dense(8 * blockSize, NS);
        W = Matrix<Double>.Build.Dense(8 * blockSize, NS);

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
            Matrix<Double> hypothesis1 = Matrix<double>.Build.Dense(N, 8 * blockSize);
            if (round == 1) plainTextsList = trsWorker.extractPlainTextsList(start, N);
            else if (round == 2) computeDES1stRoundResult();
            byte[] LRIndices = { DES.DES.getLRIndices(sboxNumber)[0] };

            for (int i = 0; i < plainTextsList.Count; i++)
            {

                var IPL = DES.DES.getIPL(Utils.HexStringToByteArray(plainTextsList[i]), round);
                var IPR = DES.DES.getIPR(Utils.HexStringToByteArray(plainTextsList[i]), round);


                var extended = DES.DES.ETrans(IPR);
                var IPLCharacBits = DES.DES.extract(IPL, LRIndices);
                var IPRCharacBIts = DES.DES.extract(IPR, LRIndices);


                for (int j = 0; j < 64; j++)
                {
                    var sboxInput = DES.DES.extract(extended, DES.DES.getConsecutive6Indices(sboxNumber));
                    int sboxInputInt = bitArrayToInt(sboxInput);
                    sboxInputInt = sboxInputInt ^ j;
                    var sboxOutput = DES.DES.getSBoxContent(sboxNumber, sboxInputInt);
                    sboxOutput = sboxOutput >> 3;
                    hypothesis1[i, j] = bitCount(sboxOutput ^ bitArrayToInt(IPLCharacBits) ^ bitArrayToInt(IPRCharacBIts));
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

    public void analyse(string method)
    {
        DateTime beforeDT;
        DateTime afterDT;
        TimeSpan ts;

        key = new byte[blockSize];
        
        if (method == "CPA")
        for (int N = 1; N < blockSize + 1; N++)
        {
            initMatrix();
            attackTracesNumber = 0;
            for (int round = 2; round <= 2; round++)
            {
                    int step = 20000;
                for (int i = 0; i < 1; i++)
                {
                    Console.WriteLine(i);
                    beforeDT = DateTime.Now;
                    initDesHypothesis(N, 1, method, step * i, step);
                    var hypothesis2 = initDesHypothesis(N, round, method, step * i, step);
                    afterDT = System.DateTime.Now;
                    ts = afterDT.Subtract(beforeDT);
                    Console.WriteLine("init hypothesis done! Elsped {0} s", ts);

                    beforeDT = DateTime.Now;
                
                        dataTraces = trsWorker.extractNTracesMatrix(step * i, step);
                    correlate2(hypothesis2);
                    //correlationMatrix = differential(hypothesis2, dataTraces);

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
        if (method == "DPA")
        {
            for (int N = 1; N < blockSize + 1; N++)
            {
                initMatrix();
                attackTracesNumber = 0;
                for (int round = 1; round <= 1; round++)
                {
                    int step = 20000;
                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine(i);
                        beforeDT = DateTime.Now;
                        var hypothesis2 = initDesHypothesis(N, round, method, step * i, step);
                        afterDT = System.DateTime.Now;
                        ts = afterDT.Subtract(beforeDT);
                        Console.WriteLine("init hypothesis done! Elsped {0} s", ts);

                        beforeDT = DateTime.Now;

                        dataTraces = trsWorker.extractNTracesMatrix(step * i, step);
                        correlationMatrix = differential(hypothesis2, dataTraces);

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

        }
        DES.DES.printBitArray(Round1Key);
        trsWorker.died();
    }

    public void correlate2(Matrix<Double> hypothesis2)
    {
        int NS = trsWorker.getSampleNumber();
        correlationMatrix = correlate(hypothesis2, dataTraces);
        
    }

    private Matrix<Double> differential(Matrix<Double> A, Matrix<Double> B)
    {
        Vector<Double> AColumnSumsVec = A.ColumnSums();
        Vector<Double> OneMinusAColumnSumsVec = (1-A).ColumnSums();

        ResAColumnSumsVec += AColumnSumsVec;
        ResOneMinusAColumnSumsVec += OneMinusAColumnSumsVec;

        Matrix<Double> ResAColumnSumsMatrix = Matrix<Double>.Build.Dense(A.ColumnCount, B.ColumnCount);
        Matrix<Double> ResOneMinusAColumnSumsMatrix = Matrix<Double>.Build.Dense(A.ColumnCount, B.ColumnCount);

        for(int i = 0; i < B.ColumnCount; i++)
        {
            ResAColumnSumsMatrix.SetColumn(i, ResAColumnSumsVec);
            ResOneMinusAColumnSumsMatrix.SetColumn(i, ResOneMinusAColumnSumsVec);
        }
        X += A.Transpose() * B;
        W += (1 - A).Transpose() * B;
        return X.PointwiseDivide(ResAColumnSumsMatrix) - W.PointwiseDivide(ResOneMinusAColumnSumsMatrix); 


    }

    private Matrix<Double> correlate(Matrix<Double> A, Matrix<Double> B)
    {
        attackTracesNumber += A.RowCount;
        Vector<Double> A2ColumnSumVec = A.PointwisePower(2).ColumnSums();
        Vector<Double> B2ColumnSumVec = B.PointwisePower(2).ColumnSums();

        ResA2ColumnSumVec += A2ColumnSumVec;
        ResB2ColumnSumVec += B2ColumnSumVec;

        Vector<Double> AColumnSumsVec = A.ColumnSums();
        Vector<Double> BColumnSumsVec = B.ColumnSums();

        ResAColumnSumsVec += AColumnSumsVec;
        ResBColumnSumsVec += BColumnSumsVec;

        Vector<Double> AvarianceVec = ResA2ColumnSumVec - (AColumnSumsVec.PointwisePower(2)) / attackTracesNumber;
        Vector<Double> BvarianceVec = ResB2ColumnSumVec - (BColumnSumsVec.PointwisePower(2)) / attackTracesNumber;



        X += A.Transpose()*B;
        Matrix<Double> Y = (ResAColumnSumsVec.ToColumnMatrix() * ResBColumnSumsVec.ToRowMatrix()) / attackTracesNumber;

        Matrix<Double> Z = AvarianceVec.ToColumnMatrix() * BvarianceVec.ToRowMatrix();
        Z = Z.PointwiseSqrt();
        return (X-Y).PointwiseDivide(Z);
    }
}
