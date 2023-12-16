using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDb書き込み用クラス</summary>
    public static class GasDbWriter
    {
        /// <summary>jsonをスプレッドシートに書き込む</summary>
        internal async static UniTask SaveAsync(string json, string sheetName)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "SetData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("isArray", "ARRAY"),
                new MultipartFormDataSection("json", json),
            };
            await GasDbUtility.SendToServer(formData);
        }

        /// <summary>データをスプレッドシートに書き込む</summary>
        /// <returns>書き込み成功時はobjectId、失敗時はnullが返ります</returns>
        public async static UniTask<string> SaveAsync<T>(T target, string sheetName=null) where T : GasDbObjectBase
        {
            if (string.IsNullOrEmpty(sheetName)) {
                sheetName = target.sheetName;
            }

            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "SetData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("json", JsonUtility.ToJson(target)),
            };
            var json = await GasDbUtility.SendToServer(formData);
            if (json == null)
            {
                return null;
            }

            var data = GasDbUtility.ParseJsonObject<T>(json);
            if (data == null)
            {
                return null;
            }

            target.objectId = data.objectId;
            target.updateDate = data.updateDate;
            target.createDate = data.createDate;
            target.sheetName = data.sheetName;
            return data.objectId;
        }
    }
}