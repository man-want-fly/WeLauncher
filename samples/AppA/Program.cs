using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("AppA started");
        Console.WriteLine("Args: " + string.Join(" ", args));
        Console.WriteLine("APP_ENV=" + Environment.GetEnvironmentVariable("APP_ENV"));
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}

