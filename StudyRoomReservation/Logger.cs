namespace StudyRoomReservation;

/// <summary>
/// Reusable logger class that writes into console and file.
/// </summary>
public static class Logger
{
    public static readonly object _lock = new();
    private static string _fileDirectory;
    private static string _fileName;
    private static bool _enableConsole;
    private static bool _enableFile;
    private static bool _enableColors;

    /// <summary>
    /// Configure logger before first use.
    /// </summary>
    public static void Configure(string fileDirectory = "logs",
        string fileName = "app.log",
        bool enableConsole = true,
        bool enableFile = true,
        bool enableColors = true)
    {
        _fileDirectory = fileDirectory;
        _fileName = fileName;
        _enableConsole = enableConsole;
        _enableFile = enableFile;
        _enableColors = enableColors;
        
        if (_enableFile && !Directory.Exists(_fileDirectory))
        {
            Directory.CreateDirectory(_fileDirectory);
        }
    }

    /// <summary>
    /// Write output to console.
    /// </summary>
    /// <param name="level">Level of log(INFO, DEBUG, WARNING, ERROR)</param>
    /// <param name="message">Message describing the log</param>
    public static void WriteToConsole(LogLevel level, string message)
    {
        if (_enableColors)
            Console.ForegroundColor = LevelColor(level);

        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Write output to file.
    /// </summary>
    /// <param name="message">Message describing the log</param>
    private static void WriteToFile(string message)
    {
        var fullPath = Path.Combine(_fileDirectory, _fileName);
        File.AppendAllText(fullPath, message + Environment.NewLine);
    }
    /// <summary>
    /// Get console color for log level.
    /// </summary>
    /// <param name="level">Level of log(INFO, DEBUG, WARNING, ERROR)</param>
    /// <returns>Color of the text</returns>
    private static ConsoleColor LevelColor(LogLevel level) =>
        level switch
        {
            LogLevel.DEBUG => ConsoleColor.DarkMagenta,
            LogLevel.INFO => ConsoleColor.Green,
            LogLevel.WARNING => ConsoleColor.Yellow,
            LogLevel.ERROR => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    
    /// <summary>
    /// Main logging method.
    /// </summary>
    /// <param name="level">Level of log(INFO, DEBUG, WARNING, ERROR)</param>
    /// <param name="message">Message describing the log</param>
    public static void Log(LogLevel level, string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_lock)
        {
            if (_enableConsole)
                WriteToConsole(level, logMessage);

            if (_enableFile)
                WriteToFile(logMessage);
        }
    }

    /// <summary>
    /// Log error message.
    /// </summary>
    /// <param name="message">Message describing the log</param>
    public static void Error(string message)
    {
        Log(LogLevel.ERROR, message);
    }

    /// <summary>
    /// Log warning message.
    /// </summary>
    /// <param name="message">Message describing the log</param>
    public static void Warning(string message)
    {
        Log(LogLevel.WARNING, message);
    }

    /// <summary>
    /// Log informational message.
    /// </summary>
    /// <param name="message">Message describing the log</param>
    public static void Info(string message)
    {
        Log(LogLevel.INFO, message);
    }

    /// <summary>
    /// Log debugging message.
    /// </summary>
    /// <param name="message">Message describing the log</param>
    public static void Debug(string message)
    {
        Log(LogLevel.DEBUG, message);
    }

}