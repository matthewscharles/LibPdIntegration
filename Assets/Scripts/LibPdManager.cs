using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[RequireComponent(typeof(AudioSource))]
public class LibPdManager : MonoBehaviour
{
    private List<LibPdInstance> instances = new List<LibPdInstance>();
    private float[] tempBuffer;
        
    #region libpd imports

    #if UNITY_IOS
        private const string DLL_NAME="__Internal";
    //The following lines will be needed if we can get externals working on Windows.
    //Just renaming libpd.dll to pd.dll does not seem to be enough though, so
    //they're commented out for now.
    //#elif UNITY_STANDALONE_WIN
    //	private const string DLL_NAME="pd";
    #else
        private const string DLL_NAME="libpd";
    #endif
    
    [DllImport(DLL_NAME)]
    private static extern void libpd_set_instance(IntPtr instance);
    #endregion

    public void RegisterInstance(LibPdInstance instance)
    {
        instances.Add(instance);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // Clear the output buffer
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0f;
        }

        // Process each instance sequentially
        foreach (var instance in instances)
        {
            if (instance.IsLoaded)
            {
                libpd_set_instance(instance.GetInstance());

                // Ensure tempBuffer is properly sized
                if (tempBuffer == null || tempBuffer.Length != data.Length)
                {
                    tempBuffer = new float[data.Length];
                }

                // Clear tempBuffer
                for (int i = 0; i < tempBuffer.Length; i++)
                {
                    tempBuffer[i] = 0f;
                }

                // Process audio
                instance.ProcessAudio(tempBuffer, channels);

                // Mix into the main output buffer
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] += tempBuffer[i];
                }
            }
        }
    }
}