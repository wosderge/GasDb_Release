using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GasDb
{
    public class GasDbJsonWriterEditorWindow : EditorWindow
    {
        [SerializeField] private string appUrl;
        [SerializeField] private string sheetName;
        [SerializeField] private TextAsset jsonFile;

        [MenuItem("GasDb/JsonImporter")]
        private static void Open()
        {
            GetWindow<GasDbJsonWriterEditorWindow>("JsonImporter");
        }

        private void OnGUI()
        {
            appUrl = EditorGUILayout.TextField("AppUrl", appUrl);
            sheetName = EditorGUILayout.TextField("SheetName", sheetName);
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JsonFile", jsonFile, typeof(TextAsset), false);
            if (GUILayout.Button("Write"))
            {
                if (EditorUtility.DisplayDialog("警告", "インポートしますか？\n既存のデータは上書きされます。", "Yes", "No"))
                {
                    GasDbSettings.AppUrl = appUrl;
                    GasDbWriter.SaveAsync(jsonFile.text, sheetName).Forget();
                }
            }
        }
    }
}