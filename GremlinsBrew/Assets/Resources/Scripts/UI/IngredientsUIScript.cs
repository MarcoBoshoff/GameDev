using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientsUIScript : MonoBehaviour
{
    private ResourceType potionType = ResourceType.Empty;

    public RectTransform rect;
    public Image imageRenderer;
    public Text stockText;
    public Vector2 targetPos;

    public Sprite[] ingredentSprites;

    public ResourceType PotionType
    {
        get => potionType;
        set
        {
            //TODO Set sprite based on the potion type
            potionType = value;

            switch (potionType)
            {
                case ResourceType.HealthPotion:
                    imageRenderer.sprite = ingredentSprites[0];
                    break;
                case ResourceType.PoisonPotion:
                    imageRenderer.sprite = ingredentSprites[1];
                    break;
                case ResourceType.LovePotion:
                    imageRenderer.sprite = ingredentSprites[2];
                    break;
                case ResourceType.ManaPotion:
                    imageRenderer.sprite = ingredentSprites[3];
                    break;
                case ResourceType.FearPotion:
                    imageRenderer.sprite = ingredentSprites[4];
                    break;
                case ResourceType.DragonBreathPotion:
                    imageRenderer.sprite = ingredentSprites[5];
                    break;
            }
        }
    }

    float redFlashTimer = 0, flashLength = 0.7f;
    bool isRed = false;
    public void UpdateStockLevel()
    {
        int stock = GameControllerScript.StockLevel(potionType);
        stockText.text = stock.ToString();

        if (stock == 0) {
            redFlashTimer += Time.deltaTime;

            if (redFlashTimer >= flashLength)
            {
                isRed = !isRed;
                redFlashTimer -= flashLength;
            }

            imageRenderer.color = (isRed) ? Color.red : Color.white;
        }
        else if (isRed)
        {
            isRed = false;
            imageRenderer.color = Color.white;
        }
    }

    public void MoveToTarget()
    {
        rect.anchoredPosition = Vector3.MoveTowards(rect.anchoredPosition, targetPos, Time.deltaTime * 40);
    }
}
