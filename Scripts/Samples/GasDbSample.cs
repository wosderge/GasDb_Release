using Cysharp.Threading.Tasks;
using GasDb;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>GasDbサンプルコード</summary>
public sealed class GasDbSample : MonoBehaviour
{
    [SerializeField] public string appURL;
    [SerializeField] public string sheetName;

    private async void Start()
    {
        // 最初にサーバー側のURLを設定する
        GasDbSettings.AppUrl = appURL;
        await SampleSaveAsync();
        await SampleLoadAsync();
        await SampleQueryAsync();
        await SampleUpdateAsync();
    }

    /// <summary>データを保存するサンプル</summary>
    private async UniTask SampleSaveAsync()
    {
        // 新規データを作成
        var data = GasDbSampleDataUtil.GenerateRandomTestData();

        // 新規データの保存
        var objectId1 = await GasDbWriter.SaveAsync(data, sheetName);
        if (objectId1 == null) // 保存失敗時はnullが返ります
        {
            // 保存失敗時の処理
            return;
        }

        // ObjectIdの保存 -> 戻り値のObjectIdを保存しておくことで、特定のデータを取得することができます
        PlayerPrefs.SetString("Key", objectId1);

        // 既存データの上書き保存
        data.intValue *= 10;
        await GasDbWriter.SaveAsync(data, sheetName);
    }

    /// <summary>データを読み込むするサンプル</summary>
    private async UniTask SampleLoadAsync()
    {
        // データ一覧を読み込む（クエリなし）
        var data1 = await GasDbReader.LoadDataAsync<GasDbSampleData>(sheetName);
        if (data1 != null) // 読み込み失敗時はnullが返ります
        {
            foreach (var record in data1)
            {
                record.ShowLog();
            }
        }

        // データ一覧を読み込む（クエリあり）
        var query = new GasDbQuery().WhereLessThan("intValue", 5000).OrderByAscending("intValue");
        var data2 = await GasDbReader.LoadDataAsync<GasDbSampleData>(sheetName, query);
        if (data2 != null) // 読み込み失敗時はnullが返ります
        {
            foreach (var record in data2)
            {
                record.ShowLog();
            }
        }

        // ObjectIdを指定して読み込む
        var objectId = PlayerPrefs.GetString("Key");
        var data3 = await GasDbReader.LoadDataByObjectIdAsync<GasDbSampleData>(sheetName, objectId);
        if (data3 != null) // 読み込み失敗時はnullが返ります
        {
            data3.ShowLog();
        }
    }

    /// <summary>クエリ指定でデータを読み込むサンプル</summary>
    private async UniTask SampleQueryAsync()
    {
        //intValueが5000より大きいデータを、intValueの降順で取得
        var query1 = new GasDbQuery().WhereLessThan("intValue", 5000).OrderByAscending("intValue");
        var data1 = await GasDbReader.LoadDataAsync<GasDbSampleData>(sheetName, query1);
        if (data1 != null) // 読み込み失敗時はnullが返ります
        {
            foreach (var record in data1)
            {
                record.ShowLog();
            }
        }

        //降順
        new GasDbQuery().OrderByDescending("intValue");

        //昇順
        new GasDbQuery().OrderByAscending("intValue");

        //特定の文字列を含むか
        new GasDbQuery().WhereContainedIn("stringValue", "Hoge");

        //特定の文字列を含まないか
        new GasDbQuery().WhereNotContainedIn("stringValue", "Hoge");

        //特定の文字列のいずれかと一致するか
        new GasDbQuery().WhereAnyEqualTo(
            "stringValue",
            new[]
            {
                "Hoge",
                "Fuga",
            });

        //特定の値と一致するか
        new GasDbQuery().WhereEqualTo("intValue", 5000);

        //特定の値と一致しないか
        new GasDbQuery().WhereNotEqualTo("intValue", 5000);

        //特定の値より大きいか
        new GasDbQuery().WhereGreaterThan("intValue", 5000);

        //特定の値以上か
        new GasDbQuery().WhereGreaterThanOrEqualTo("intValue", 5000);

        //特定の値より小さいか
        new GasDbQuery().WhereLessThan("intValue", 5000);

        //特定の値以下か
        new GasDbQuery().WhereLessThanOrEqualTo("intValue", 5000);
    }

    /// <summary>データを一括更新するサンプル</summary>
    private async UniTask SampleUpdateAsync()
    {
        // UpdateSetAsync - データの上書き
        // intValueが5000より大きいデータのboolValueをtrueに更新
        var query1 = new GasDbQuery().WhereGreaterThan("intValue", 5000);
        await GasDbUpdater.UpdateSetAsync(sheetName, "boolValue", true, query1);

        // UpdateAddAsync - データの加算
        // intValueが5000より大きいデータに対して、intValueを100加算
        var query2 = new GasDbQuery().WhereGreaterThan("intValue", 5000);
        await GasDbUpdater.UpdateAddAsync(sheetName, "intValue", 100, query2);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GasDbSample))]
public class GasDbSampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Add Random Sample Data"))
        {
            var sample = (GasDbSample)target;
            GasDbSettings.AppUrl = sample.appURL;
            var data = GasDbSampleDataUtil.GenerateRandomTestData();
            GasDbWriter.SaveAsync(data, sample.sheetName).Forget();
        }
    }
}
#endif