using System;
using GasDb;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GasDbSampleData : GasDbObjectBase
{
    [Serializable]
    public class TestObject
    {
        public float x;
        public float y;
        public float z;
    }
    
    public string stringValue;
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public TestObject objectValue;
    public string[] sts;
    
    public void ShowLog()
    {
        Debug.Log($"stringValue: {stringValue} intValue: {intValue} floatValue: {floatValue}");
    }
}

public static class GasDbSampleDataUtil
{
    public static GasDbSampleData GenerateRandomTestData()
    {
        return new GasDbSampleData
        {
            intValue = Random.Range(0, 1000),
            stringValue = $"Test_{Random.Range(0, 1000)}",
            floatValue = Random.Range(0f, 1000f),
            boolValue = Random.Range(0, 2) == 0,
            objectValue = new GasDbSampleData.TestObject
            {
                x = Random.Range(0f, 1000f),
                y = Random.Range(0f, 1000f),
                z = Random.Range(0f, 1000f),
            },
            sts = new []
            {
                Guid.NewGuid().ToString(),Guid.NewGuid().ToString(),Guid.NewGuid().ToString(),
            },
        };
    }
}