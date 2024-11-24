using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_Text buttonText;

    public void OnClickConnect()
    {
        if (usernameInput.text.Length > 1)
        {
            PhotonNetwork.NickName = usernameInput.text;
            PlayerPrefs.SetString("PlayerName", usernameInput.text);
            buttonText.text = "Connecting...";
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}
