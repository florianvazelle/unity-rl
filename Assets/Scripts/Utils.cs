using System;
using System.IO; 

namespace Utils {

    public class RandomGenerator 
    {  
        // Instantiate random number generator.  
        // It is better to keep a single Random instance 
        // and keep using Next on the same instance.  
        static private readonly Random _random = new Random();  
    
        // Generates a random number within a range.      
        static public int RandomNumber(int min, int max)
        {  
            return _random.Next(min, max);  
        }  

        static public float RandomFloat(float min, float max)
        {
            return min + ((float)_random.NextDouble()) * (max - min);
        }
    }  

    public class Logger 
    {
        public Logger(string environnement, string logFilePath = "MyLog.log")
        {
            if(!logFilePath.EndsWith(".log"))
                logFilePath += ".log";
            
            LogFilePath = logFilePath;
            Environnement = environnement;

            if(!File.Exists(LogFilePath))
                File.Create(LogFilePath).Close();
            
            WriteLine("--- New Session Started");
        }

        public string LogFilePath { get; private set; }
        public string Environnement { get; private set; }

        public void WriteLine(object message)
        {
            using(StreamWriter writer = new StreamWriter(LogFilePath, true))
                writer.WriteLine(DateTime.Now.ToString() + " [" + Environnement + "]: " + message.ToString());
        }
    }
}