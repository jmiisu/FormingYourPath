//using Mirror;
//using UnityEngine;

//public class MyNetworkManager : NetworkManager
//{
//    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
//    {
//        base.OnServerAddPlayer(conn);

//        MyNetworkPlayer player = conn.identity.GetComponent<MyNetworkPlayer>();
//        Debug.Log($"Player Connected.{player.name} {numPlayers}");

//        player.SetDisplayName($"Player: {numPlayers}");
//    }
//}
