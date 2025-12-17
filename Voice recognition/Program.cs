using Vosk;
using NAudio.Wave;
using System;
using System.Text.Json;
using System.Runtime.InteropServices; // Diperlukan untuk P/Invoke (simulasi tombol)

// --- FUNGSI UNTUK SIMULASI KEYBOARD ---
public class Win32
{
    // Konstanta untuk menandakan pelepasan tombol (Key Up)
    public const int KEYEVENTF_KEYUP = 0x0002;

    // Virtual Key Code untuk tombol yang ingin kita tekan.
    public const byte VK_L = 0x4C;
    public const byte VK_ADOWN = 0x28;
    public const byte VK_AUP = 0x26;

    // Deklarasi fungsi WinAPI keybd_event
    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

    public static void SimulateKeyPress(byte keyCode)
    {
        // 1. Tekan tombol ke bawah (Key Down)
        keybd_event(keyCode, 0x00, 0, IntPtr.Zero);

        // Jeda kecil (opsional, tapi disarankan)
        System.Threading.Thread.Sleep(50);

        // 2. Lepaskan tombol (Key Up)
        keybd_event(keyCode, 0x00, KEYEVENTF_KEYUP, IntPtr.Zero);
    }
}
// ----------------------------------------


public class SpeechRecognizer
{
    public static void Main(string[] args)
    {
        // Pastikan model sudah diinstal di lokasi ini
        Model folder_vosk = new Model("D:\\vosk-model-en-us-0.22");

        var merekam = new VoskRecognizer(folder_vosk, 16000.0f);

        Console.WriteLine("Memulai inisialisasi perekaman...");

        // Peringatan penting untuk fungsi simulasi keyboard
        //Console.ForegroundColor = ConsoleColor.Yellow;
        //Console.WriteLine("PERINGATAN: Program ini akan menyimulasikan penekanan tombol L saat Anda mengucapkan 'like'.");
        //Console.WriteLine("Pastikan Anda berada di aplikasi yang ingin Anda kontrol (misal: Notepad).");
        //Console.ResetColor();

        using (var waveIn = new WaveInEvent())
        {
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.BufferMilliseconds = 1000;

            waveIn.DataAvailable += (sender, e) =>
            {
                if (merekam.AcceptWaveform(e.Buffer, e.BytesRecorded))
                {
                    string jsonResult = merekam.Result();

                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(jsonResult))
                        {
                            if (document.RootElement.TryGetProperty("text", out JsonElement textElement))
                            {
                                string recognizedText = textElement.GetString().Trim().ToLower(); // Dibuat lowercase dan trim untuk perbandingan

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[HASIL AKHIR]: {recognizedText}");
                                Console.ResetColor();

                                // --- LOGIKA SIMULASI TOMBOL KEYBOARD ---
                                if (recognizedText == "like" || recognizedText == "the like" || recognizedText == "light" || recognizedText == "the light")
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine(">>> Kata like terdeteksi Menekan tombol L");
                                    Console.ResetColor();

                                    Win32.SimulateKeyPress(Win32.VK_L);
                                }

                                else if (recognizedText == "down" || recognizedText == "the down" || recognizedText == "dowd" || recognizedText == "dowded")
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine("kata down terdeteksi menekan tombol Arrow bawah");
                                    Console.ResetColor();

                                    Win32.SimulateKeyPress(Win32.VK_ADOWN);
                                }

                                else if (recognizedText == "up" )
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("kata up terdeteksi menekan tombol arrow atas");
                                    Console.ResetColor();

                                    Win32.SimulateKeyPress(Win32.VK_AUP);
                                }
                                // ---------------------------------------
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                }
            };

            waveIn.StartRecording();
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Mulai berbicara. Tekan tombol apa saja untuk berhenti.");
            Console.WriteLine("---------------------------------------------");

            Console.ReadKey();

            waveIn.StopRecording();
            Console.WriteLine("\nPerekaman dihentikan.");
        }
    }
}