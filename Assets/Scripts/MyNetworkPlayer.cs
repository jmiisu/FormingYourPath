using Mirror;
using System;
using TMPro;
using UnityEngine;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SerializeField] 
    private TMP_Text displayNameText = null;

    [SyncVar(hook = nameof(HandleDisplayNameUpdated))]
    [SerializeField] 
    private string displayName = "Missing Name";

    [Server] 
    public void SetDisplayName(string newDisplayName)
    {
        displayName = newDisplayName;
    }

    [Command]
    private void CmdSetDisplayName(string newDisplayName)
    {

    }

    [Client]
    private void HandleDisplayNameUpdated(string oldName, string newName)
    {
        displayNameText.text = newName;
        Debug.Log($"---{displayName}");
    }
}
