namespace AndenStemesterEksamensProjekt.Services
{
    //lavet af:
    // Emil
    class Diag_logService
    {
        public static void Log(string message)
        {
            // Simple logging to console for debug purposes
            Console.WriteLine($"[DiagLog] {DateTime.UtcNow}: {message}");
        }

        public static void LogError(string message, Exception ex)
        {
            // Simple error logging to console for debug purposes
            Console.WriteLine($"[DiagLog][ERROR] {DateTime.UtcNow}: {message} - Exception: {ex.Message}");
        }

        public static void LogWarning(string message)
        {
            // Simple warning logging to console for debug purposes
            Console.WriteLine($"[DiagLog][WARNING] {DateTime.UtcNow}: {message}");
        }

        public static void LogInfo(string message)
        {
            // Simple info logging to console for demonstration purposes
            Console.WriteLine($"[DiagLog][INFO] {DateTime.UtcNow}: {message}");
        }
    }


}