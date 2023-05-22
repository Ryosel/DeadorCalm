using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Managers.Game.FollowCam = GameObject.Find("Follow Cam");
        Managers.Game.ReconCam = GameObject.Find("Recon Cam");
        Managers.Game.ChangeGameMode(Define.GameMode.Infil);

        Managers.UI.ShowSceneUI<UI_HUD>();

        Managers.Sound.Play("Music", Define.Sound.Bgm);
    }

    public override void Clear()
    {

    }
}
