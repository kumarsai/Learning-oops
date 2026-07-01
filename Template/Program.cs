using Template;

Console.WriteLine("Hello, World!");
DataProcessor textFile = new TextFile();
textFile.ReadProcessAndSave();

DataProcessor excelFile = new ExcelFile();
excelFile.ReadProcessAndSave();
