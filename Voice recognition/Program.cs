using Vosk;
using NAudio.Wave;
using System;
using System.Text.Json;
using System.Runtime.InteropServices; 
using System.Threading.Tasks;

public class Win32
{
    
    public const int KEYEVENTF_KEYUP = 0x0002;

   
    public const byte VK_L = 0x4C;
    public const byte VK_ADOWN = 0x28;
    public const byte VK_AUP = 0x26;
    public const byte VK_MUTE = 0x4D;
    public const byte VK_STOP = 0x20;


    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

    public static void SimulateKeyPress(byte keyCode)
    {
       
        keybd_event(keyCode, 0x00, 0, IntPtr.Zero);

       
        System.Threading.Thread.Sleep(50);

        
        keybd_event(keyCode, 0x00, KEYEVENTF_KEYUP, IntPtr.Zero);
    }
}

public class SpeechService
{
    private WaveInEvent? waveIn;
    private VoskRecognizer? merekam;
    private Model? folder_vosk;

    public void StartListening()
    {
        
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
           
            if (document.RootElement.TryGetProperty("partial", out JsonElement partialElement))
            {
                string partialText = partialElement.GetString() .Trim().ToLower();

                if (!string.IsNullOrEmpty(partialText))
                {
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
                   
                    OnLogReceived?.Invoke($"Terdeteksi: {recognizedText}");


                    if (recognizedText == "like" || recognizedText == "the light" || recognizedText == "like like" || recognizedText == "light" || recognizedText == "the like")
                    {
                        Win32.SimulateKeyPress(Win32.VK_L);
                    }
                    else if (recognizedText == "down" || recognizedText == "dowd" || recognizedText == "the down" || recognizedText == "the dowd")
                    {
                        Win32.SimulateKeyPress(Win32.VK_ADOWN);
                    }
                    else if (recognizedText == "up" || recognizedText == "the up" || recognizedText == "the up up" || recognizedText == "the up the up")
                    {
                        Win32.SimulateKeyPress(Win32.VK_AUP);
                    }
                    else if (recognizedText == "mute" || recognizedText == "the mute" || recognizedText == "mute mute" || recognizedText == "the mute the mute")
                    {
                        Win32.SimulateKeyPress(Win32.VK_MUTE);
                    }
                    else if (recognizedText == "stop" || recognizedText == "the stop" || recognizedText == "the stop stop" || recognizedText == "the stop the stop")
                    {
                        Win32.SimulateKeyPress(Win32.VK_STOP);
                    }
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