using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDb更新用クラス</summary>
    /// <remarks>各々がローカルに保持しているデータは変化しないため不整合が起こります</remarks>
    /// <remarks>ローカルでは更新しないデータ向け</remarks>
    public static class GasDbUpdater
    {
        /// <summary>条件に一致するデータのkey列をvalueで一括更新します</summary>
        public async static UniTask<string> UpdateSetAsync(string sheetName, string key, object value, GasDbQuery query = null)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "UpdateData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("key", key),
                new MultipartFormDataSection("value", value.ToString()),
                new MultipartFormDataSection("mode", "set"),
            };
            query?.AddToForm(formData);
            return await GasDbUtility.SendToServer(formData);
        }

        /// <summary>条件に一致するデータのkey列に対してvalueを一括加算します</summary>
        /// <remarks>数字でない列では値は変化しない</remarks>
        public async static UniTask<string> UpdateAddAsync(string sheetName, string key, object value, GasDbQuery query = null)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "UpdateData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("key", key),
                new MultipartFormDataSection("value", value.ToString()),
                new MultipartFormDataSection("mode", "add"),
            };
            query?.AddToForm(formData);
            return await GasDbUtility.SendToServer(formData);
        }
    }
}