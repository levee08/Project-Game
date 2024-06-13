using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using TMPro;

public class PlayerInfoInput : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField customInput;

    public void SavePlayerInfo()
    {
        DataManager.Instance.playerData.PlayerName = nameInput.text;
        DataManager.Instance.playerData.PlayerAge = int.Parse(ageInput.text);
        DataManager.Instance.playerData.CustomID = int.Parse(customInput.text);

        Debug.Log("Player Info Saved: " + DataManager.Instance.playerData.PlayerName + ", " + DataManager.Instance.playerData.PlayerAge + ", " + DataManager.Instance.playerData.CustomID);
    }

}
