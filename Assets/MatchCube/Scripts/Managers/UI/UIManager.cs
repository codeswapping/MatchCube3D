using System.Collections;
using System.Collections.Generic;
using MatchCube.Scripts.Managers;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject continueGame;
    public GameObject startGame;

    

    public void OnContinueGameButtonClick()
    {
        GameManager.Instance.ContinueGame();
    }

    public void OnStartGameButtonClick()
    {
        GameManager.Instance.StartGame();
    }
}
