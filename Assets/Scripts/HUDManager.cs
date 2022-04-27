using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance {get; private set;}
    private Transform Score;    
    private Transform Holder;    
    private Transform SlotTemplate;    
    private Transform BallUnitTemplate;    

    void Awake()
    {        
        Instance = this;
        Score = transform.Find("Score").Find("Value");
        Holder = transform.Find("Queue").Find("Holder");
        SlotTemplate = Holder.Find("Slot");
        BallUnitTemplate = SlotTemplate.Find("BallUnitTemplate");
    }

    public void SetScore(int score)
    {
        Score.GetComponent<TextMeshProUGUI>().text = score.ToString();
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
            var spriteBase = queuedBallUnit.GetSprite();
            var maskBase = queuedBallUnit.GetMask();
            
            unit.sprite = spriteBase;
            if (maskBase != null) special.sprite = maskBase;
            else special.gameObject.SetActive(false);            

            unitSlot.gameObject.SetActive(true);
        }
    }
}
