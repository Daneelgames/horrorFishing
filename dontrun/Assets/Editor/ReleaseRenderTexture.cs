using UnityEngine;
 
#if UNITY_EDITOR
using UnityEditor;
#endif
 
// occasionally releases TempBuffers, which are created through a bug, beloning to Unity.
// Without it, unity can consume a lot of memory, have bad fps in editor & even crash.
//
// Authors:  OneManBandGames,  Igor Aherne
// https://forum.unity.com/threads/profiler-memory-snapshot-determine-who-references-the-rendertexture.479704/#post-3363255
[InitializeOnLoad]
public static class ReleaseRenderTexture{
 
#if UNITY_EDITOR
   
    static double _flushIntervals = 90;//every 90 seconds, or tweak to do less often (involves garbage collection)
    static double _lastTime = 0;
 
    //ran via [InitializeOnLoad]
    static ReleaseRenderTexture() {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }
 
 
 
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void OnCompiled() {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }
 
 
    static void OnEditorUpdate() {
       
        if(EditorApplication.timeSinceStartup < _lastTime + _flushIntervals) {
            return;
        }
        _lastTime = EditorApplication.timeSinceStartup;
 
        //release the bugged textures:
        var rendTex = (RenderTexture[])Resources.FindObjectsOfTypeAll(typeof(RenderTexture));
        for (int i = 0; i < rendTex.Length; i++)
        {
            if (rendTex[i].name.StartsWith("TempBuffer"))
            {
                RenderTexture.ReleaseTemporary(rendTex[i]);
            }
        }
        System.GC.Collect();
    }
 
#endif
}