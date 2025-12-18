using Vosk;
using NAudio.Wave;
using System;
using System.Text.Json;
using System.Runtime.InteropServices; // Diperlukan untuk P/Invoke (simulasi tombol)
using System.Threading.Tasks;

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


public class SpeechService
{
    private WaveInEvent? waveIn;
    private VoskRecognizer? merekam;
    private Model? folder_vosk;

    public void StartListening()
    {
        // 1. Inisialisasi Model
        folder_vosk = new Model(@"D:\vosk-model-en-us-0.22");
        merekam = new VoskRecognizer(folder_vosk, 16000.0f);

        waveIn = new WaveInEvent();
        waveIn.DeviceNumber = 0;
        waveIn.WaveFormat = new WaveFormat(16000, 1);
        waveIn.BufferMilliseconds = 500;

        waveIn.DataAvailable += (sender, e) =>
        {
            if (merekam.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                string jsonResult = merekam.Result();
                ProcessResult(jsonResult);
            }
            else
            {
                // PartialResult() dipanggil saat sedang berbicara (Real-time)
                // Gunakan ini jika ingin respon yang sangat cepat
                string partialJson = merekam.PartialResult();
                ProcessPartial(partialJson);
            }
        };

        waveIn.StartRecording();
    }

    public event Action<string>?  OnLogReceived;

    private void ProcessPartial(string jsonResult)
    {
        using (JsonDocument document = JsonDocument.Parse(jsonResult))
        {
            // Untuk Partial Result, Vosk menggunakan properti "partial"
            if (document.RootElement.TryGetProperty("partial", out JsonElement partialElement))
            {
                string partialText = partialElement.GetString() .Trim().ToLower();

                if (!string.IsNullOrEmpty(partialText))
                {
                    // Kirim ke UI agar user bisa melihat teks yang sedang diproses
                    //OnLogReceived?.Invoke($"Mendengarkan: {partialText}...");
                }
            }
        }
    }
    private void ProcessResult(string jsonResult)
    {
        using (JsonDocument document = JsonDocument.Parse(jsonResult))
        {
            if (document.RootElement.TryGetProperty("text", out JsonElement textElement))
            {
                string recognizedText = textElement.GetString().Trim().ToLower();

                if (!string.IsNullOrEmpty(recognizedText) || recognizedText != "the")
                {
                    // Kirim teks yang dideteksi ke event log
                    OnLogReceived?.Invoke($"Terdeteksi: {recognizedText}");

                    // Logika simulasi tombol tetap sama
                    if (recognizedText == "like" || recognizedText == "the light" || recognizedText == "like like" || recognizedText == "light" || recognizedText == "the like")
                    {
                        Win32.SimulateKeyPress(Win32.VK_L);
                    }
                    else if (recognizedText == "down" || recognizedText == "dowd" || recognizedText == "the down" || recognizedText == "the dowd")
                    {
                        Win32.SimulateKeyPress(Win32.VK_ADOWN); 
                    }
                    else if (recognizedText.Contains("up")) Win32.SimulateKeyPress(Win32.VK_AUP);
                }
            }
        }
    }

    public void StopListening()
    {
        waveIn?.StopRecording();
        waveIn?.Dispose();
    }
}