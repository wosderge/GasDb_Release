using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDbのUtility用クラス</summary>
    internal static class GasDbUtility
    {
        /// <summary>送信共通処理</summary>
        internal async static UniTask<string> SendToServer(List<IMultipartFormSection> formData)
        {
            var www = UnityWebRequest.Post(GasDbSettings.AppUrl, formData);
            Log("Sending...");
            await www.SendWebRequest();
            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.DataProcessingError or UnityWebRequest.Result.ProtocolError)
            {
                LogError(www.error);
            }
            else
            {
                Log("Succeeded to send.");
                Log(www.downloadHandler.text);
                return www.downloadHandler.text;
            }

            return null;
        }

        internal static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=yellow>[GasDb]</color> {message}");
#endif
        }

        internal static void LogError(string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"<color=yellow>[GasDb]</color> {message}");
#else
            Debug.LogWarning($"<color=yellow>[GasDb]</color> Some error has occurred.");
#endif
        }

        [Serializable]
        private class ArrayWrapper<T>
        {
            public T[] data;
        }

        [Serializable]
        private class ArrayWrapperWithLine<T>
        {
            public T[] data;
            public int line;
        }

        internal static string ArrayToJson<T>(T[] obj)
        {
            var wrapper = new ArrayWrapper<T>
            {
                data = obj,
            };
            return JsonUtility.ToJson(wrapper);
        }

        internal static T ParseJsonObject<T>(string json) where T : GasDbObjectBase
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (InvalidCastException e)
            {
                LogError($"Parsing result from Google Sheets failed. Error: {e.Message}");
            }

            return null;
        }

        internal static T[] ParseJsonArray<T>(string json) where T : GasDbObjectBase
        {
            try
            {
                return JsonUtility.FromJson<ArrayWrapper<T>>($"{{\"data\":{json}}}").data;
            }
            catch (InvalidCastException e)
            {
                LogError($"Parsing result from Google Sheets failed. Error: {e.Message}");
            }

            return null;
        }

        internal static T[] ParseJsonArrayWithLine<T>(string json, out int line) where T : GasDbObjectBase
        {
            try
            {
                var wrapper = JsonUtility.FromJson<ArrayWrapperWithLine<T>>(json);
                line = wrapper.line;
                return wrapper.data;
            }
            catch (InvalidCastException e)
            {
                LogError($"Parsing result from Google Sheets failed. Error: {e.Message}");
            }

            line = -1;
            return null;
        }
    }
}