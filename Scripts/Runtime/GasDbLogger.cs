using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDbのログ管理に特化したクラス</summary>
    /// <remarks>初回読み込みでは最新のinitialLoadMax件分を読み込み、その後は更新されたログを取得します</remarks>
    /// <remarks>読み込み完了したログは、Popで順次取り出せます</remarks>
    public sealed class GasDbLogger<T> where T : GasDbObjectBase
    {
        private readonly string sheetName;
        private readonly int initialLoadMax;
        private readonly HashSet<string> alreadyLoadLog = new();
        private readonly Queue<T> logQueue = new();
        private int latestLoadLineNum;

        public GasDbLogger(string sheetName, int initialLoadMax = 10)
        {
            this.initialLoadMax = initialLoadMax;
            this.sheetName = sheetName;
        }
        
        /// <summary>最新のデータから順に読み込んだデータを取り出す</summary>
        public bool TryPop(out T data)
        {
            if (logQueue.Count == 0)
            {
                data = null;
                return false;
            }

            data = logQueue.Dequeue();
            return true;
        }
        
        /// <summary>データ送信し、更新を確認する</summary>
        public async UniTask SendAndFetchAsync(T target)
        {
            await SendAndFetchAsyncImpl(target);
        }

        /// <summary>更新確認のみを行う</summary>
        public async UniTask FetchAsync()
        {
            await SendAndFetchAsyncImpl(null);
        }

        private async UniTask SendAndFetchAsyncImpl(T target)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("method", "GetLog"),
                new MultipartFormDataSection("sheetName", sheetName),
            };
            if (target != null)
            {
                formData.Add(new MultipartFormDataSection("json", JsonUtility.ToJson(target)));
            }

            if (latestLoadLineNum == 0)
            {
                formData.Add(new MultipartFormDataSection("limit", initialLoadMax.ToString()));
                formData.Add(new MultipartFormDataSection("loadStartLineNum", 1.ToString()));
            }
            else
            {
                formData.Add(new MultipartFormDataSection("loadStartLineNum", latestLoadLineNum.ToString()));
            }

            var json = await GasDbUtility.SendToServer(formData);
            if (json == null)
            {
                return;
            }

            var data = GasDbUtility.ParseJsonArrayWithLine<T>(json, out latestLoadLineNum);
            if (data == null || data.Length == 0)
            {
                return;
            }

            for (var i = data.Length - 1; i >= 0; i--)
            {
                var record = data[i];
                if (!alreadyLoadLog.Add(record.objectId))
                {
                    continue;
                }

                logQueue.Enqueue(data[i]);
            }
        }
    }
}