# About this project
This c# project is aimed at launching  Correlational Power Analysis(CPA) and Differential Power Analysis on DES algorithm. 
The power traces is acquired on a SASEBO-G board.

# Implementation Details
## Reference code
### CPA Code
The implementation is based on a python implementation of CPA attacks on AES,  whose source code is available at [CorrelationPowerAnalysis](https://github.com/laiqinghui/CorrelationPowerAnalysis).

This project uses the Math.NET package to compute the correlation coefficient in a matrix-based way, but it seems it is not efficient as the original implementation by python. 
### DES intermediate value Computation Code
To launch CPA attacks, we should compute the intermediate values of the cryptographic algorithms. The project reference [DES implementation in C# ](https://github.com/killerart/DES) to compute the intermediate values.
## About Math.NET
I use Math.NET to operate on Matrix. So how to use Math.NET? The answer is to use NuGet to install the package. 
What is NuGet? If you are primer in c# and visual studio, I will tell you that NuGet to .Net framework is like pip to python. It is a package manager. 
### Install Math.NET
To run this project, you should install Math.NET.

You can reference [使用 dotnet CLI 安装和管理包](https://docs.microsoft.com/zh-cn/nuget/consume-packages/install-use-packages-dotnet-cli) to install Math.NET in the project.
For me, the steps are

1. In command line, switch the directory to the project's root directory which contain the `.csproj` file.

2. execute command `dotnet add package MathNet.Numerics --version 4.15.0`. This command is from https://www.nuget.org/packages/MathNet.Numerics/4.15.0. 

Maybe there are other ways to install Math.NET, such as using Package Manager Console in Visual Studio >Tools>NuGet Package Manager, ref. https://blog.csdn.net/heray1990/article/details/72467304, but I failed in this way.

Another link that may be useful is https://www.hossambarakat.net/2020/06/24/fix-error-NU1101/.
## Power trace file format
The trace file in this project is in `.trx` format. The`trs` file format is a binary file format defined by Riscure. You can reference [python-trsfile](https://github.com/Riscure/python-trsfile) to get information about how to read this kind of file.

Details about the trace set file in this project may be provided later.

