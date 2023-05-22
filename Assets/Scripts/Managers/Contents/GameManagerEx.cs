using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    PlayerController _player;

    public GameObject FollowCam { get; set; }
    public GameObject ReconCam { get; set; }

    int _score = 0; // ���� ���� ����
    public bool IsGameover { get; private set; } // ���� ���� ����

    public Define.GameMode Mode { get; private set; } // ���Ӹ��: ����, ����

    public void Init()
    {
        // �÷��̾� ĳ������ ��� �̺�Ʈ �߻��� ���� ����
        //_player = GameObject.FindObjectOfType<PlayerController>();
        //_player.OnDeath += EndGame;
    }

    public void ChangeGameMode(Define.GameMode mode)
    {
        Mode = mode;
        if (Mode == Define.GameMode.Infil)
        {
            FollowCam.SetActive(true);
            ReconCam.SetActive(false);
        }
        else if(Mode == Define.GameMode.Recon)
        {
            ReconCam.SetActive(true);
            FollowCam.SetActive(false);
        }
    }

    // ������ �߰��ϰ� UI ����
    public void AddScore(int newScore)
    {
        // ���� ������ �ƴ� ���¿����� ���� ���� ����
        if (!IsGameover)
        {
            // ���� �߰�
            _score += newScore;
            // ���� UI �ؽ�Ʈ ����
            UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
            if (ui != null)
                ui.UpdateScoreText(_score);
        }
    }

    // ���� ���� ó��
    public void EndGame()
    {
        // ���� ���� ���¸� ������ ����
        IsGameover = true;
        // ���� ���� UI�� Ȱ��ȭ
        Managers.UI.ShowPopupUI<UI_Gameover>();
    }

    public void RestartGame()
    {
        Managers.Scene.LoadScene(Define.Scene.Game);
        IsGameover = false;
    }
}
