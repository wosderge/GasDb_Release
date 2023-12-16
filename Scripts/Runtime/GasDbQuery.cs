using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GasDb
{
    /// <summary>GasDbクエリ用クラス</summary>
    public sealed class GasDbQuery
    {
        [Serializable]
        private struct QueryData
        {
            [SerializeField] private string key;
            [SerializeField] private string value;
            [SerializeField] private string[] any;
            [SerializeField] private string condition;
            internal QueryData(string key, object value, string condition)
            {
                this.key = key;
                this.value = value.ToString();
                any = null;
                this.condition = condition;
            }
            
            internal QueryData(string key, object[] any, string condition )
            {
                this.key = key;
                value = null;
                this.any = any.Select(x => x.ToString()).ToArray();
                this.condition = condition;
            }
        }

        [Serializable]
        private struct OrderData
        {
            [SerializeField] private string key;
            [SerializeField] private bool anti;
            internal OrderData(string key, bool anti)
            {
                this.key = key;
                this.anti = anti;
            }
        }
        
        private readonly List<QueryData> queries = new();
        private OrderData order;

        /// <summary>クエリをサーバ用の情報に代入</summary>
        internal void AddToForm(List<IMultipartFormSection> list)
        {
            list.Add(new MultipartFormDataSection("query", GasDbUtility.ArrayToJson(queries.ToArray())));
            list.Add(new MultipartFormDataSection("order", JsonUtility.ToJson(order)));
        }

        /// <summary>ソート：昇順</summary>
        public GasDbQuery OrderByAscending(string key)
        {
            order = new OrderData(key, false);
            return this;
        }

        /// <summary>ソート：降順</summary>
        public GasDbQuery OrderByDescending(string key)
        {
            order = new OrderData(key, true);
            return this;
        }

        /// <summary>クエリ：含む</summary>
        public GasDbQuery WhereContainedIn(string key, object value)
        {
            queries.Add(new QueryData(key, value,"in"));
            return this;
        }

        /// <summary>クエリ：含まない</summary>
        public GasDbQuery WhereNotContainedIn(string key, object value)
        {
            queries.Add(new QueryData(key, value, "nin"));
            return this;
        }

        /// <summary>クエリ：いずれかと一致</summary>
        public GasDbQuery WhereAnyEqualTo(string key, object[] values)
        {
            queries.Add(new QueryData(key,values,"anyeq"));
            return this;
        }

        /// <summary>クエリ：一致</summary>
        public GasDbQuery WhereEqualTo(string key, object value)
        {
            queries.Add(new QueryData(key, value, "eq"));
            return this;
        }

        /// <summary>クエリ：一致しない</summary>
        public GasDbQuery WhereNotEqualTo(string key, object value)
        {
            queries.Add(new QueryData(key, value, "ne"));
            return this;
        }

        /// <summary>クエリ：より多い</summary>
        public GasDbQuery WhereGreaterThan(string key, object value)
        {
            queries.Add(new QueryData(key,value,"gt"));
            return this;
        }

        /// <summary>クエリ：以上</summary>
        public GasDbQuery WhereGreaterThanOrEqualTo(string key, object value)
        {
            queries.Add(new QueryData(key, value, "gte"));
            return this;
        }

        /// <summary>クエリ：未満</summary>
        public GasDbQuery WhereLessThan(string key, object value)
        {
            queries.Add(new QueryData(key, value, "lt"));
            return this;
        }

        /// <summary>クエリ：以下</summary>
        public GasDbQuery WhereLessThanOrEqualTo(string key, object value)
        {
            queries.Add(new QueryData(key, value, "lte"));
            return this;
        }
    }
}