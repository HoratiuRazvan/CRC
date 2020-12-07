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
            for ( i = inputFileName.Length; i >= 0; i--)
                if (inputFileName[i] == '\\' || inputFileName[i] == '/')
                    break;
            String sourceFolder = inputFileName.Substring(inputFileName.Length - i);
            if (sourceFolder != inputFileName && !File.Exists(Path.Combine(sourceFolder, inputFileName)))
                    return "Fisierul nu a fost gasit";
            if (key == "")
                return "Cheia de criptare nu a fost specificata";
            if (!(key[0] >= '0' && key[0] <= '9' || key[0] >= 'A' && key[0] <= 'F' || key[0] >= 'a' && key[0] <= 'f') ||
                !(key[1] >= '0' && key[1] <= '9' || key[1] >= 'A' && key[1] <= 'F' || key[1] >= 'a' && key[1] <= 'f'))
                return "Cheia nu e specificata in Hexa";
            byte[] text = File.ReadAllBytes(inputFileName);
            String stringKey = Convert.ToString(Convert.ToInt32(key, 16), 2);

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
                if(v == true)
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
                    binWriter.Write(crc);                   //CRC - null daca nu a fost cerut
                    
                }
                return "Operation completed successfully!";
            }
            else //decriptare
            {
                String fullStringKey = null;
                for (int j = 0; j < text.Length / 8; j++)
                    fullStringKey = fullStringKey + stringKey;
                fullStringKey = fullStringKey + stringKey;
                byte[] binaryKey = Encoding.ASCII.GetBytes(fullStringKey);
                for (int j = 0; j < text.Length; j++)
                    text[j] ^= binaryKey[j];
            }
            return "Operation completed successfully!";

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
