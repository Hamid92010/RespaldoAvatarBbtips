using UnityEngine;
using System.Collections;
using System.IO;
using ReadyPlayerMe.Core;
using UnityEngine.Networking;

public class AudioAddSet : MonoBehaviour
{
    private const string AUDIO_FOLDER_PATH = "Assets/Audio";
    private AudioSource audioSource;
    private VoiceHandler voiceHandler;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        voiceHandler = GetComponent<VoiceHandler>();

        if (voiceHandler == null)
        {
            Debug.LogError("VoiceHandler component not found on the same GameObject.");
            enabled = false;
            return;
        }

        StartCoroutine(PlayAudioFiles());
    }

    IEnumerator PlayAudioFiles()
    {
        DirectoryInfo dir = new DirectoryInfo(AUDIO_FOLDER_PATH);
        FileInfo[] files = dir.GetFiles("*.mp3");

        foreach (FileInfo file in files)
        {
            yield return StartCoroutine(GetAudioClipFromFile(file.FullName, audioClip =>
            {
                // Reproducir el AudioClip
                voiceHandler.PlayAudioClip(audioClip);
            }));

            // Esperar hasta que termine de reproducirse
            yield return new WaitForSeconds(audioSource.clip.length);

            // Borrar el archivo del disco
            File.Delete(file.FullName);
        }
    }

    IEnumerator GetAudioClipFromFile(string filePath, System.Action<AudioClip> callback)
    {
        // Cargar el archivo de audio desde la ruta del archivo usando UnityWebRequestMultimedia
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error al cargar el audio desde {filePath}: {www.error}");
            }
            else
            {
                // Devolver el AudioClip a través del callback
                var audioClipHandler = (DownloadHandlerAudioClip)www.downloadHandler;
                callback?.Invoke(audioClipHandler.audioClip);
            }
        }
    }
}
