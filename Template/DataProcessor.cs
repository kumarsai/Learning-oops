using System;
using System.Collections.Generic;
using System.Text;

namespace Template
{
    internal abstract class DataProcessor
    {
        public void ReadProcessAndSave() {
            Read();
            Process();
            Save();
        }

        public abstract void Read();
        public abstract void Process();
        public void Save()
        {
            Console.WriteLine("Saving data file...");
        }
    }

    internal class TextFile : DataProcessor
    {
        //public void ReadProcessAndSave() {
        //    Read();
        //    Process();
        //    Save();
        //}
        public override void Read() { 
        Console.WriteLine("Reading text file...");
        }


        public override void Process() { 
        Console.WriteLine("Processing text file...");
        }

        //public void Save()
        //{
        //    Console.WriteLine("Saving data file...");
        //}

    }

    internal class ExcelFile : DataProcessor
    {
        //public override void Read()
        //{
        //    Read();
        //    Process();
        //    Save();
        //}

        public override void Read()
        {
            Console.WriteLine("Reading Excel file...");
        }


        public override void Process()
        {
            Console.WriteLine("Processing Excel file...");
        }

        //public void Save()
        //{
        //    Console.WriteLine("Saving data file...");
        //}
    }
}
