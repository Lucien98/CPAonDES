using System;
using System.Text;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
}

void main()
{
    //test_matrix_operation();
    
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
