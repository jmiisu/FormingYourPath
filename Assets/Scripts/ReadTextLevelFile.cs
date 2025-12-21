using UnityEngine;
using System;
public class ReadTextLevelFile : MonoBehaviour
{
    private string[] ReadLevelText()
    {
        TextAsset bindData = Resources.Load("FirstLevel") as TextAsset;

        string data = bindData.text.Replace(Environment.NewLine, string.Empty);

        return data.Split('-');
    }
}
