using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance {get; private set;}
    private Image BGM;    
    [SerializeField] private Sprite BGM_On;
    [SerializeField] private Sprite BGM_Off;
    private Image SFX;
    [SerializeField] private Sprite SFX_On;
    [SerializeField] private Sprite SFX_Off;
    private Transform Score;
    private Transform Highscore;
    private Transform Holder;
    private Transform SlotTemplate;    
    private Transform BallUnitTemplate;    

    void Awake()
    {        
        Instance = this;
        var Game = transform.Find("Game");
        Score = Game.Find("Score").Find("Value");
        Highscore = Game.Find("Highscore").Find("Value");

        BGM = Game.Find("BGM").GetComponent<Image>();
        SFX = Game.Find("SFX").GetComponent<Image>();

        Holder = Game.Find("Queue").Find("Holder");
        SlotTemplate = Holder.Find("Slot");
        BallUnitTemplate = SlotTemplate.Find("BallUnitTemplate");
    }

    public void SetScore(int score)
    {
        Score.GetComponent<TextMeshProUGUI>().text = score.ToString();
    }

    public void SetHighscore(int score)
    {
        Highscore.GetComponent<TextMeshProUGUI>().text = score.ToString();
    }

    public void SetQueue(Queue<Tile> queue)
    {
        foreach(Transform child in Holder)
        {
            if (child == SlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Tile tile in queue)
        {
            RectTransform unitSlot = Instantiate(BallUnitTemplate, Holder).GetComponent<RectTransform>();            
            var unit = unitSlot.transform.GetChild(0).GetComponent<Image>();
            var special = unitSlot.transform.GetChild(1).GetComponent<Image>();     
            var queuedBallUnit = tile.queuedBallIndicator;
            var colorBase = queuedBallUnit.GetSpriteColor();
            var maskBase = (queuedBallUnit.specialType == SpecialUnit.None)? null : queuedBallUnit.GetMaskSprite();
            
            unit.color = colorBase;
            
            if (maskBase != null) special.sprite = maskBase;
            else special.gameObject.SetActive(false);            

            unitSlot.gameObject.SetActive(true);
        }
    }

    public void ToggleBGM()
    {
        if (BGM.sprite == BGM_On) BGM.sprite = BGM_Off;
        else BGM.sprite = BGM_On;
    }

    public void ToggleSFX()
    {
        if (SFX.sprite == SFX_On) SFX.sprite = SFX_Off;
        else SFX.sprite = SFX_On;
    }

    public void SetBGM(bool on)
    {
        if (on) BGM.sprite = BGM_On;
        else BGM.sprite = BGM_Off;
    }

    public void SetSFX(bool on)
    {
        if (on) SFX.sprite = SFX_On;
        else SFX.sprite = SFX_Off;
    }
}