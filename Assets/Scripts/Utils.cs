using System;
using System.IO; 
using UnityEngine;

namespace Utils {

    public class RandomGenerator {

        // Instantiate random number generator.  
        // It is better to keep a single Random instance 
        // and keep using Next on the same instance.  
        static private readonly System.Random _random = new System.Random();  
    
        // Generates a random number within a range.      
        static public int RandomNumber(int min, int max) {  
            return _random.Next(min, max);  
        }  

        static public float RandomFloat(float min, float max) {
            return min + ((float)_random.NextDouble()) * (max - min);
        }
    }  

    public class Logger {

        public Logger(string environnement, string logFilePath = "MyLog.log") {
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

        public void WriteLine(object message) {
            using(StreamWriter writer = new StreamWriter(LogFilePath, true))
                writer.WriteLine(DateTime.Now.ToString() + " [" + Environnement + "]: " + message.ToString());
        }
    }

    public class Render {
        
        public static void SpawnTile(int x, int y, Sprite sprite, Color color) {
            string name = x + ":" + y;

            GameObject g = GameObject.Find(name);
            if (g == null) {
                g = new GameObject (name);
            }

            g.tag = "Tile";
            g.transform.position = new Vector3(x, y);

            var tile = g.GetComponent<SpriteRenderer>();
            if (tile == null) {
                tile = g.AddComponent<SpriteRenderer>();
            }

            tile.sprite = sprite;
            tile.color = color;
        }
    }
}