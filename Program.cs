using System;
using System.Diagnostics;

namespace Explorer
{
    class ArrowEditor
    {
        private int currentRow;
        private int minRow;
        private int maxRow;

        public ArrowEditor(int currentRow, int minRow, int maxRow)
        {
            this.currentRow = currentRow;
            this.maxRow = maxRow;
            this.minRow = minRow;
        }

        public void ClearArrow()
        {
            Console.SetCursorPosition(0, currentRow);
            Console.Write("  ");
        }

        private void InsertArrow()
        {
            Console.SetCursorPosition(0, currentRow);
            Console.Write("->");
        }

        public int GetCurrentRow()
        {
            return currentRow;
        }

        public void CheckKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                {
                    if (currentRow - 1 >= minRow)
                    {
                        ClearArrow();
                        currentRow--;
                        InsertArrow();
                    }

                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    if (currentRow + 1 <= maxRow)
                    {
                        ClearArrow();
                        currentRow++;
                        InsertArrow();
                    }

                    break;
                }
                default:
                    break;
            }
        }
    }

    class Explorer
    {
        private static int ChooseDrive()
        {
            var drives = DriveInfo.GetDrives();
            var rowCounter = 0;
            Console.WriteLine("Выберите Диск:");

            foreach (var drive in drives)
            {
                rowCounter++;
                if (rowCounter == 1)
                    Console.WriteLine(
                        $"-> {drive.Name} Свободного места: {(float)drive.TotalFreeSpace / 1024 / 1024 / 1024}");
                else
                {
                    Console.WriteLine(
                        $"   {drive.Name} Свободного места: {(float)drive.TotalFreeSpace / 1024 / 1024 / 1024}");
                }
            }

            var arrowEditor = new ArrowEditor(1, 1, rowCounter);
            var flag = true;

            Console.SetCursorPosition(2, 1);
            while (flag)
            {
                Console.SetCursorPosition(2, Console.CursorTop);
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    flag = false;
                }
                else
                {
                    arrowEditor.CheckKey(key);
                }
            }

            return Console.CursorTop - 1;
        }

        private static List<string> DirectoryPrinter(DirectoryInfo directoryInfo)
        {
            var paths = new List<string>();

            Console.WriteLine("Выберите файл или директорию:");

            if (directoryInfo.Parent != null)
            {
                Console.WriteLine("-> ..");
                paths.Add(directoryInfo.Parent.FullName);
            }

            foreach (var file in directoryInfo.GetDirectories())
            {
                if (file.Attributes.HasFlag(FileAttributes.Hidden))
                    continue;
                if (paths.Count == 0)
                    Console.WriteLine($"-> {file.Name} /// {file.CreationTime}");
                else
                    Console.WriteLine($"   {file.Name} /// {file.CreationTime}");
                paths.Add(file.FullName);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Attributes.HasFlag(FileAttributes.Hidden))
                    continue;
                if (paths.Count == 0)
                    Console.WriteLine($"-> {file.Name} /// {file.LastWriteTime}");
                else
                    Console.WriteLine($"   {file.Name} /// {file.LastWriteTime}");
                paths.Add(file.FullName);
            }

            return paths;
        }

        private static bool DirectoryController(DriveInfo drive)
        {
            var paths = DirectoryPrinter(drive.RootDirectory);
            var currentPath = drive.RootDirectory.FullName;
            var arrowEditor = new ArrowEditor(1, 1, paths.Count);

            Console.SetCursorPosition(2, 1);
            while (true)
            {
                Console.SetCursorPosition(2, Console.CursorTop);
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Backspace)
                {
                    return false;
                }
                else if (key == ConsoleKey.Enter)
                {
                    if (File.Exists(paths[arrowEditor.GetCurrentRow() - 1]))
                    {
                        var process = new System.Diagnostics.Process();
                        process.StartInfo = new ProcessStartInfo(paths[arrowEditor.GetCurrentRow() - 1])
                        {
                            UseShellExecute = true
                        };
                        process.Start();
                    }
                    else
                    {
                        Console.Clear();
                        currentPath = paths[arrowEditor.GetCurrentRow() - 1];
                        paths = DirectoryPrinter(new DirectoryInfo(paths[arrowEditor.GetCurrentRow() - 1]));
                        arrowEditor = new ArrowEditor(1, 1, paths.Count);
                        Console.SetCursorPosition(2, 1);
                    }
                }
                else if (key == ConsoleKey.Escape && new DirectoryInfo(currentPath).Parent != null)
                {
                    Console.Clear();
                    currentPath = paths[0];
                    paths = DirectoryPrinter(new DirectoryInfo(paths[0]));
                    arrowEditor = new ArrowEditor(1, 1, paths.Count);
                    Console.SetCursorPosition(2, 1);
                }
                else if (key == ConsoleKey.Escape)
                {
                    return true;
                }
                else
                {
                    arrowEditor.CheckKey(key);
                }
            }
        }

        private static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                var driveNumber = ChooseDrive();
                Console.Clear();
                if (!DirectoryController(DriveInfo.GetDrives()[driveNumber]))
                {
                    break;
                }
            }
        }
    }
}