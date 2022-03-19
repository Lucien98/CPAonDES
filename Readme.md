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

# Specification of DES implementation

## Test vectors and intermediate values for DES algorithm

When you are implementing a cryptographic algorithm, if you do not have a test vector, you will struggle hard to get a right implementation in a short time. To be honest, I struggled a lot and waste a lot of time.

So here I will give a test vector and the intermediate values of DES algorithm. 

When the plaintext `P` is `0x5C915E6C616AA0F2`, and the key `K` is `0x417A8F9F6A2B1C7D`.( Note that the key are randomly generated 64 bits, so the parity check bits may be wrong, it will nevertheless affect the encryption and decryption results, so we ignore it. )

This table is the intermediate values using `K` to encrypt `P`, obtaining the ciphertext `C=0x541F8D5C9447839C`.

|                     |    `L`     |    `R`     |
| :-----------------: | :--------: | :--------: |
| Initial permutation | `BD870D12` | `C2F82DA4` |
|   Round 1 result    | `C2F82DA4` | `299B0B3D` |
|   Round 2 result    | `299B0B3D` | `59754660` |
|   Round 3 result    | `59754660` | `3E65D911` |
|   Round 4 result    | `3E65D911` | `90AD2A3C` |
|   Round 5 result    | `90AD2A3C` | `46228B10` |
|   Round 6 result    | `46228B10` | `F818FABF` |
|   Round 7 result    | `F818FABF` | `DFC172F5` |
|   Round 8 result    | `DFC172F5` | `488420BD` |
|   Round 9 result    | `488420BD` | `B7AB2AEF` |
|   Round 10 result   | `B7AB2AEF` | `E6497CB9` |
|   Round 11 result   | `E6497CB9` | `6D0D73B8` |
|   Round 12 result   | `6D0D73B8` | `705D5270` |
|   Round 13 result   | `705D5270` | `45C8CBF0` |
|   Round 14 result   | `45C8CBF0` | `688FD0D2` |
|   Round 15 result   | `688FD0D2` | `D4008E62` |
|   Round 16 result   | `D4008E62` | `299BBF66` |
|      Exchange       | `299BBF66` | `D4008E62` |
|  Final permutation  | `541F8D5C` | `9447839C` |



Key expansion intermediates are listed as follows:

| Round    | value          |
| -------- | -------------- |
| round 1  | `B48560FB7CAD` |
| round 2  | `D2B9122BFE67` |
| round 3  | `2CA663BEEDB2` |
| round 4  | `E3540EAD4F57` |
| round 5  | `6883D0DFE2D2` |
| round 6  | `14D83BF5C74D` |
| round 7  | `A721529AB6CE` |
| round 8  | `2E4E85FCF7A5` |
| round 9  | `2D15412FF8AD` |
| round 10 | `4348ED627DF7` |
| round 11 | `99E190AF89BF` |
| round 12 | `140FABC75FD3` |
| round 13 | `F330055F837D` |
| round 14 | `098EC4D3DDCC` |
| round 15 | `5070BE48B7BD` |
| round 16 | `44CEA4BCF4F9` |

