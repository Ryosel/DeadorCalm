using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    PlayerController _player;

    public GameObject FollowCam { get; set; }
    public GameObject ReconCam { get; set; }

    int _score = 0; // 현재 게임 점수
    public bool IsGameover { get; private set; } // 게임 오버 상태

    public Define.GameMode Mode { get; private set; } // 게임모드: 잠입, 정찰

    public void Init()
    {
        // 플레이어 캐릭터의 사망 이벤트 발생시 게임 오버
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

    // 점수를 추가하고 UI 갱신
    public void AddScore(int newScore)
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (!IsGameover)
        {
            // 점수 추가
            _score += newScore;
            // 점수 UI 텍스트 갱신
            UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
            if (ui != null)
                ui.UpdateScoreText(_score);
        }
    }

    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        IsGameover = true;
        // 게임 오버 UI를 활성화
        Managers.UI.ShowPopupUI<UI_Gameover>();
    }

    public void RestartGame()
    {
        Managers.Scene.LoadScene(Define.Scene.Game);
        IsGameover = false;
    }
}
