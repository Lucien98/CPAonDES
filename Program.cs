using System;
using System.Text;
using System.Collections;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using NumSharp;
using NumSharp.Utilities;


void test_matrix_operation()
{
    Console.WriteLine("------------------------------------------------------------------------------------");
    Console.WriteLine("-                 create a dense matrix with 3 rows and 4 columns                  -");
    Console.WriteLine("------------------------------------------------------------------------------------");
    // create a dense matrix with 3 rows and 4 columns
    // filled with random numbers sampled from the standard distribution
    Matrix<double> m = Matrix<double>.Build.Random(3, 4);

    Console.WriteLine(m);
    m = m.PointwisePower(2);

    Console.WriteLine(m);
    Console.WriteLine();
    Console.WriteLine();
}


void testCPAv2()
{
    String fileName = @"E:\2021Fall\work\DES\CorrelationPowerAnalysis-master\traceset.trs";
    //String fileName = @"E:\traceset500k.trs";
    CPAv2 cpa = new CPAv2(fileName);
    DateTime beforeDT = DateTime.Now;
    cpa.analyse("CPA");
    DateTime afterDT = DateTime.Now;
    TimeSpan ts = afterDT.Subtract(beforeDT);
    Console.WriteLine("init correlation matrix done! Elapsed {0} s", ts);

}

void testgetkeyfromSubKey()
{
    byte[] plainText = { 0x5c, 0x91, 0x5e, 0x6c, 0x61, 0x6a, 0xa0, 0xf2 };
    byte[] keyBytes = { 0x41, 0x7a, 0x8f, 0x9f, 0x6a, 0x2b, 0x1c, 0x7d };
    byte[] cipherText = { 0x54, 0x1f, 0x8d, 0x5c, 0x94, 0x47, 0x83, 0x9c };

    var subkeyss = DES.DES.CreateSubKeys(keyBytes);
    DES.DES.getKeyFromSubkey(subkeyss[0], plainText, cipherText);

}

testCPAv2();
//testgetkeyfromSubKey();
//DES.DES.getSubkeys();


/*****************************************************************************************************************/

string _tostring(object obj)
{
    switch (obj)
    {
        case NDArray nd:
            return nd.ToString(false);
        case Array arr:
            if (arr.Rank != 1 || arr.GetType().GetElementType()?.IsArray == true)
                arr = Arrays.Flatten(arr);
            var objs = toObjectArray(arr);
            return $"[{string.Join(", ", objs.Select(_tostring))}]";
        default:
            return obj?.ToString() ?? "null";
    }

    object[] toObjectArray(Array arr)
    {
        var len = arr.LongLength;
        var ret = new object[len];
        for (long i = 0; i < len; i++)
        {
            ret[i] = arr.GetValue(i);
        }

        return ret;
    }
}
void testNumSharp()
{
    var nd = np.full(5, 12); //[5, 5, 5 .. 5]
    //Console.WriteLine(_tostring(nd));
    nd = np.zeros(12); //[0, 0, 0 .. 0]
    nd = np.arange(12).reshape(3, 4); //[0, 1, 2 .. 11]
    Console.WriteLine(_tostring(nd));
    Console.WriteLine();
    nd = nd.mean(0);
    Console.WriteLine(_tostring(nd));
    Console.WriteLine();

    //nd = nd - nd.mean(0);
    // create a matrix
    nd = np.zeros((3, 4)); //[0, 0, 0 .. 0]
    nd = np.arange(12).reshape(3, 4);

    var nd2 = np.zeros(15).reshape(3, 5);

    nd = nd.T;
    // access data by index
    var data = nd[1, 1];
    Console.WriteLine(_tostring(nd));
    Console.WriteLine(_tostring(data));
    // create a tensor
    nd = np.arange(12);

    // reshaping
    data = nd.reshape(2, -1); //returning ndarray shaped (2, 6)
    Console.WriteLine(_tostring(data));

    //nd = [[1,2,3]];

    Shape shape = (2, 3, 2);
    data = nd.reshape(shape); //Tuple implicitly casted to Shape
                              //or:
    nd = nd.reshape(2, 3, 2);

    // slicing tensor
    data = nd[":, 0, :"]; //returning ndarray shaped (2, 1, 2)
    data = nd[Slice.All, 0, Slice.All]; //equivalent to the line above.

    // nd is currently shaped (2, 3, 2)
    // get the 2nd vector in the 1st dimension
    data = nd[1]; //returning ndarray shaped (3, 2)

    // get the 3rd vector in the (axis 1, axis 2) dimension
    data = nd[1, 2]; //returning ndarray shaped (2, )

    // get flat representation of nd
    data = nd.flat; //or nd.flatten() for a copy

    // interate ndarray
    foreach (object val in nd)
    {
        // val can be either boxed value-type or a NDArray.
    }

    var iter = nd.AsIterator<int>(); //a different T can be used to automatically perform cast behind the scenes.
    while (iter.HasNext())
    {
        //read
        int val = iter.MoveNext();

        //write
        iter.MoveNextReference() = 123; //set value to the next val
                                        //note that setting is not supported when calling AsIterator<T>() where T is not the dtype of the ndarray.
    }
}

//testNumSharp();