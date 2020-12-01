/*using System;
using Bassoon;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered
{
    class Program
    {
        static void Main(string[] args)
        {
            // Doesn't really need to be in `using` block, but it's highly recommended as it handles automatic
            // research cleanup for you.
            using (BassoonEngine be = new BassoonEngine())
            {
                Console.WriteLine("Basson Audio!");

                // Need something to play back
                if (args.Length < 1)
                {
                    Console.WriteLine("Please specifiy a file (as the first argument) to playback");
                    return;
                }

                // Load an audiofile
                string path = args[0];
                using (Sound sound = new Sound(path))
                {
                    // Lower volume first
                    sound.Volume = 0.1f;
                    Console.WriteLine();

                    // Play it!
                    sound.Play();
                    if (sound.IsPlaying)
                        Console.WriteLine($"Playing \"{path}\" @ 10% volume...");

                    // hold until the user quits
                    Console.WriteLine("[Press enter to quit the program]");
                    Console.ReadLine();
                }

                Console.WriteLine("All done!");
            }
        }
    }
}*/
