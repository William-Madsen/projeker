using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    // Mac Quartz funktioner
    // fundt fra en tutorial online
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateMouseEvent(IntPtr source, int mouseType, CGPoint mouseCursorPosition, int mouseButton);
    //  Opretter en mouse event (klik)

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventPost(int tap, IntPtr @event);
    // Sender mouse eventen til systemet

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cf);
    // laver et cleanup af event pointerne

    private const int kCGHIDEventTap = 0;
    private const int kCGEventLeftMouseDown = 1;
    private const int kCGEventLeftMouseUp = 2;
    private const int kCGMouseButtonLeft = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double x;
        public double y;
    }

    static void SendClick()
    {
        // Brug position 650,500 i px. Kan udvides til nuværende cursor.
        var pos = new CGPoint { x = 650, y = 500 };

        // Mus ned
        IntPtr mouseDown = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventLeftMouseDown, pos, kCGMouseButtonLeft);
        CGEventPost(kCGHIDEventTap, mouseDown);

        // Mus op
        IntPtr mouseUp = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventLeftMouseUp, pos, kCGMouseButtonLeft);
        CGEventPost(kCGHIDEventTap, mouseUp);

        CFRelease(mouseDown);
        CFRelease(mouseUp);
    }

    static void Main()
    {
        Console.WriteLine("Welcome to Auto Clicker!");

        Console.WriteLine("Hvor lang skal der være mellem klik (ms)?");
        string? input = Console.ReadLine();
        int interval;

        if (!int.TryParse(input, out interval))
        {
            interval = 1000; // default
            Console.WriteLine($"Ugyldigt input. Bruger standard interval: {interval} ms");
        }
        else
        {
            Console.WriteLine($"Interval set til {interval} ms");
        }

        Console.WriteLine("Tryk på Enter for at starte auto clicker...");
        Console.ReadLine();

        bool running = true;

        while (running)
        {
            // Tjek om ESC er trykket
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    running = false;
                    break;
                }
            }

            // Send klik
            SendClick();

            // Vent interval
            Thread.Sleep(interval);
        }

        Console.WriteLine("Auto-clicker stoppet.");
    }
}
