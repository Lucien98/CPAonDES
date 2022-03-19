using System;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
public class TrsWorker
{
	private string fileName;
    private int headerLength = 0;
    private int numberOfTraces;
    private int numberOfSamples;
    private int bytesPerSample;
    private int cryptoDataLength;
    private int singleTraceLength;
    private int blockSize;

    FileStream fs;
    BinaryReader binaryReader;
    public TrsWorker(String fname)
    {
        fileName = fname;
        readHeader();
        fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        binaryReader = new BinaryReader(fs);
    }

    private void readHeader()
    {
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fs);
        Byte fieldTag = 0x00;
        Byte fieldLength;
        int fieldValue = 0;
        while(fieldTag != 0x5f)
        {
            fieldTag =  binaryReader.ReadByte();
            fieldLength = binaryReader.ReadByte();
            headerLength += (2 + fieldLength);
            if (fieldTag == 0x5f) break;
            if (fieldLength == 1) 
                fieldValue = binaryReader.ReadByte();
            else if(fieldLength == 2)
                fieldValue = binaryReader.ReadInt16();
            else if(fieldLength == 4)
                fieldValue = binaryReader.ReadInt32();

            if (fieldTag == 0x41) numberOfTraces = fieldValue;
            if (fieldTag == 0x42) numberOfSamples = fieldValue;
            if (fieldTag == 0x43) bytesPerSample = fieldValue;
            if (fieldTag == 0x44) cryptoDataLength = fieldValue;

            //Console.WriteLine("{0:X2}", fieldTag);
            //Console.WriteLine(fieldLength);
            //Console.WriteLine(fieldValue);
        }
        singleTraceLength = cryptoDataLength + numberOfSamples * bytesPerSample;
        blockSize = cryptoDataLength / 2;
        //Console.WriteLine(headerLength);
        fs.Close();
        //numberOfTraces = 100000;
    }
    public List<String> extractPlainTextsList(int start, int N)
    {
        List<String> plainTextsList = new List<string>();
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        Byte[] buffer = new Byte[blockSize];

        Int64 offset = headerLength + (Int64)start * (Int64)singleTraceLength;

        fs.Seek(offset, SeekOrigin.Begin);
        for (int i = 0; i < N; i++)
        {
            fs.Read(buffer, 0, blockSize);
            fs.Seek(singleTraceLength - blockSize, SeekOrigin.Current);
            plainTextsList.Add(Utils.ByteArrayToHexString(buffer));
        }
        fs.Close();
        return plainTextsList;
    }
    public List<String> extractCipherTextsList()
    {
        List<String> cipherTextsList = new List<string>();
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        Byte[] buffer = new Byte[blockSize];

        long position = fs.Seek(headerLength, SeekOrigin.Begin);
        Console.WriteLine(position);
        for (int i = 0; i < numberOfTraces; i++)
        {
            fs.Read(buffer, 0, blockSize);
            fs.Seek(singleTraceLength - blockSize, SeekOrigin.Current);
            cipherTextsList.Add(Utils.ByteArrayToHexString(buffer));
        }
        fs.Close();
        return cipherTextsList;
    }

    public Vector<Double> extractNthTracePointsVec(int N)
    {
        Vector<Double> nthTracePointsVec = Vector<Double>.Build.Dense(numberOfTraces);
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fs);

        fs.Seek(headerLength + cryptoDataLength + N * bytesPerSample, SeekOrigin.Begin);
        for (int i = 0; i < numberOfTraces; i++)
        {
            switch (bytesPerSample)
            {
                case 1:
                    nthTracePointsVec[i] = binaryReader.ReadByte();
                    break;
                case 2:
                    nthTracePointsVec[i] = binaryReader.ReadInt16();
                    break;
                case 4:
                    nthTracePointsVec[i] = binaryReader.ReadInt32();
                    break;
                default:
                    Console.WriteLine("Invalid sample coding! Sample must be coded in 1 or 2 or 4 bytes. ");
                    Environment.Exit(-1);
                    break;
            }
            fs.Seek(singleTraceLength - bytesPerSample, SeekOrigin.Current);
        }
        fs.Close();
        return nthTracePointsVec;
    }
    /*********************************************
     * Read N traces at the trace of index 'start'
     * 
     *********************************************/
    public Matrix<Double> extractNTracesMatrix(int start, int N)
    {
        Matrix<Double> nTracesMatrix = Matrix<Double>.Build.Dense(N, numberOfSamples);
        
        long offset = headerLength + (Int64)(cryptoDataLength + bytesPerSample * numberOfSamples) * (Int64)start; 
        fs.Seek(offset, SeekOrigin.Begin);
        for (int i = 0; i < N; i++)
        {
            fs.Seek(cryptoDataLength, SeekOrigin.Current);

            for (int j = 0; j < numberOfSamples; j++)
            {
                switch (bytesPerSample)
                {
                    case 1:
                        nTracesMatrix[i, j] = binaryReader.ReadByte();
                        break;
                    case 2:
                        nTracesMatrix[i, j] = binaryReader.ReadInt16();
                        break;
                    case 4:
                        nTracesMatrix[i, j] = binaryReader.ReadInt32();
                        break;
                    default:
                        Console.WriteLine("Invalid sample coding! Sample must be coded in 1 or 2 or 4 bytes. ");
                        Environment.Exit(-1);
                        break;
                }
            }
            
        }
        //fs.Close();
        return nTracesMatrix;
    }

    public int getSampleNumber()
    {
        return numberOfSamples;
    }

    public int getTracesNumber()
    {
        return numberOfTraces;
    }

    public void died()
    {
        fs.Close();
    }
}

