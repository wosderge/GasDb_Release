using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDb読み込み用クラス</summary>
    public static class GasDbReader
    {
        /// <summary>objectIdに一致するデータを1つ読み込む </summary>
        /// <returns>読み込み失敗時はnullが返ります</returns>
        public async static UniTask<T> LoadDataByObjectIdAsync<T>(string sheetName, string objectId) where T : GasDbObjectBase
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "GetData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("key", objectId),
            };
            var json = await GasDbUtility.SendToServer(formData);
            return json == null ? null : GasDbUtility.ParseJsonObject<T>(json);
        }

        /// <summary>シートのデータを読み込む（クエリ指定可能）</summary>
        /// <returns>読み込み失敗時はnullが返ります</returns>
        public async static UniTask<T[]> LoadDataAsync<T>(string sheetName, GasDbQuery queries = null) where T : GasDbObjectBase
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "GetData"),
                new MultipartFormDataSection("sheetName", sheetName),
                new MultipartFormDataSection("firstone", false.ToString()),
            };
            queries?.AddToForm(formData);
            var json = await GasDbUtility.SendToServer(formData);
            return json == null ? null : GasDbUtility.ParseJsonArray<T>(json);
        }
    }
}