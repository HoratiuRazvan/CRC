using System;
using System.IO;
using System.Text;

namespace CRC
{
    class Program
    {
        static String CRC(String inputFileName, String key, String outputFileName, bool d = false, bool v = false)
        {
            if(d == true && v == true)
                return "Invalid parameter {0}";
            if (inputFileName == "")
                return "Fisierul de intrare nu a fost specificat";
            if (inputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 || outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                return "Numele fisierului este invalid";
            int i;
            for ( i = inputFileName.Length - 1; i >= 0; i--)
                if (inputFileName[i] == '\\' || inputFileName[i] == '/')
                    break;
            String sourceFolder = inputFileName.Substring(inputFileName.Length - i);
            if (sourceFolder != inputFileName && !File.Exists(Path.Combine(sourceFolder, inputFileName)))
                    return "Fisierul nu a fost gasit";
            if(sourceFolder == "")
                sourceFolder = Directory.GetCurrentDirectory();
            if (key == "")
                return "Cheia de criptare nu a fost specificata";
            if (!(key[0] >= '0' && key[0] <= '9' || key[0] >= 'A' && key[0] <= 'F' || key[0] >= 'a' && key[0] <= 'f') ||
                !(key[1] >= '0' && key[1] <= '9' || key[1] >= 'A' && key[1] <= 'F' || key[1] >= 'a' && key[1] <= 'f'))
                return "Cheia nu e specificata in Hexa";
            byte[] text = File.ReadAllBytes(inputFileName);
            if (outputFileName == "")
                if (d == true)
                    outputFileName = inputFileName + ".dec";
                else
                    outputFileName = inputFileName + ".enc";
            int len = outputFileName.Length - 4;
            if (len >= 4 && (outputFileName.Substring(len) == ".enc" && (d = true)) || (outputFileName.Substring(len) == ".dec" && d == false) )
                outputFileName = outputFileName.Remove(len);

            String stringKey = Convert.ToString(Convert.ToInt32(key, 16), 2);
            try
            {
                if (d == false)//Criptare
                {
                    String fullStringKey = null;
                    for (int j = 0; j < text.Length / 8; j++)
                        fullStringKey = fullStringKey + stringKey;
                    fullStringKey = fullStringKey + stringKey;
                    byte[] binaryKey = Encoding.ASCII.GetBytes(fullStringKey);
                    for (int j = 0; j < text.Length; j++)
                        text[j] ^= binaryKey[j];
                    byte[] crc = null;
                    if (v == true)
                    {
                        for (int j = 0; j < text.Length / 32; j++)
                            for (int q = 0; q < 32; q++)
                                crc[q] ^= text[j * 32 + q];
                        int start = text.Length / 32;
                        for (int j = start; j < text.Length; j++)
                            crc[j - start] ^= text[j];
                    }
                    using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open(outputFileName, FileMode.Create)))
                    {
                        binWriter.Write(text.Length);           //Lungimea textului criptat
                        binWriter.Write(text);                  //Textul criptat
                        if (crc != null)
                            binWriter.Write(crc);                   //CRC - null daca nu a fost cerut
                    }
                    return "Operation completed successfully!";
                }
                else //decriptare
                {
                    String fullStringKey = null;
                    byte[] byteTextLength = new byte[] { text[0], text[1], text[2], text[3], text[4], text[5], text[6], text[7] };
                    int intTextLength = BitConverter.ToInt32(byteTextLength);

                    for (int j = 8; j < (text.Length - 40) / 8; j++) // 40 = 32 + 8 numarul de elemente pe care nu le folosim
                        fullStringKey = fullStringKey + stringKey;
                    fullStringKey = fullStringKey + stringKey;
                    byte[] binaryKey = Encoding.ASCII.GetBytes(fullStringKey);
                    for (int j = 8; j < (text.Length - 40); j++)
                        text[j] ^= binaryKey[j];
                    byte[] crc = null;
                    for (int j = text.Length - 32; j < text.Length; j++)
                        crc[j - text.Length + 32] = text[j]; // am copiat crc-ul uzual de la final
                    for (int j = 8; j < (text.Length - 40) / 32; j++)       //acum xoram CRC-ul de la final cu fiecare 32 de biti si daca obtinem
                        for (int q = 0; q < 32; q++)                        //doar biti 0 atunci CRC-ul dat e valid si am obtinut un rezultat bun
                            crc[q] ^= text[j * 32 + q];
                    int start = (text.Length - 32) / 32;
                    for (int j = start; j < text.Length; j++)
                        crc[j - start] ^= text[j];
                    for (int j = 0; j < 32; j++)
                        if (crc[j] != 0)
                            return "Decrypted data validation failed";
                    using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open(outputFileName, FileMode.Create)))
                    {
                        binWriter.Write(text);                  //Textul criptat
                    }
                    return "Operation completed succesfully!";
                }
            }
            catch
            {
                return "Operation failed!";
            }
        }
        static void Main(string[] args)
        {
            String key;
            Console.WriteLine("Please select a key!");
            key = Console.ReadLine();
            CRC("E:\\interviu\\date.exe", key, "E:\\interviu\\date.exe.out");
        }
    }
}
