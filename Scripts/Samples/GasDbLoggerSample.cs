using Cysharp.Threading.Tasks;
using GasDb;
using UnityEngine;

/// <summary>GasDbLoggerサンプルコード</summary>
public sealed class GasDbLoggerSample : MonoBehaviour
{
    [SerializeField] private string appURL;
    [SerializeField] private string sheetName;
    private GasDbLogger<GasDbSampleData> logger;

    private void Start()
    {
        // 最初にサーバー側のURLを設定する
        GasDbSettings.AppUrl = appURL;

        // ロガーの初期化
        logger = new GasDbLogger<GasDbSampleData>(sheetName);

        // 最初に読み取るデータ数の指定も可能です
        // logger = new GasDbLogger<GasDbSampleData>(sheetName, 100);

        // 初回読み込み
        logger.FetchAsync().Forget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ランダムなデータを作成して送信 & 最新データの取得
            var data = GasDbSampleDataUtil.GenerateRandomTestData();
            logger.SendAndFetchAsync(data).Forget();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // 最新データの取得
            logger.FetchAsync().Forget();
        }

        // 最新のデータがあれば取得して表示
        if (logger.TryPop(out var record))
        {
            record.ShowLog();
        }
    }
}