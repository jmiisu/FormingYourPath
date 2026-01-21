using UnityEngine;
using System;
public class ReadTextFile : MonoBehaviour
{
    public static string[] ReadText(string resourcePath)
    {
        TextAsset bindData = Resources.Load(resourcePath) as TextAsset;

        if (bindData == null) return Array.Empty<string>();

        string data = bindData.text.Replace(Environment.NewLine, string.Empty);

        return data.Split('-');
    }
}
