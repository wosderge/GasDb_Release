using System;
using UnityEngine;

namespace GasDb
{
    /// <summary>GasDbObjectの基底クラス GasDbで読み書きしたいデータはこれを継承してください</summary>
    [Serializable]
    public abstract class GasDbObjectBase
    {
        [SerializeField]internal string sheetName;
        [SerializeField]internal string objectId;
        [SerializeField]internal string updateDate;
        [SerializeField]internal string createDate;
    }
}