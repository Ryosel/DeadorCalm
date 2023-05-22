using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HUD : UI_Scene
{
    enum Texts
    {
        AmmoText,
        ScoreText,
        EnemyWaveText
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
    }

    // 탄약 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        GetText((int)Texts.AmmoText).text = magAmmo + "/" + remainAmmo;
    }

    // 점수 텍스트 갱신
    public void UpdateScoreText(int newScore)
    {
        GetText((int)Texts.ScoreText).text = "Score : " + newScore;
    }

    // 적 웨이브 텍스트 갱신
    public void UpdateWaveText(int waves, int count)
    {
        GetText((int)Texts.EnemyWaveText).text = "Wave : " + waves + "\nEnemy Left : " + count;
    }
}
