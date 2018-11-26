using UnityEngine;
using Sirenix.OdinInspector;

public struct SMBEventInfo
{
    [SerializeField, BoxGroup]
    private string _functionName;
    [SerializeField, BoxGroup]
    private IEventParameter _parameter;

    public string FunctionName => _functionName;
    public IEventParameter Parameter => _parameter;
}
