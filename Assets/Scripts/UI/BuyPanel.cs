﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TowerBuyListItem[] towerList;
    [SerializeField] private TowerBuyStats[] statList;
    [SerializeField] private Button buyBtn;

    [Header("Default Icons"), SerializeField]
    private Sprite damageStatIcon;

    [SerializeField] private Sprite attackSpeedStatIcon;
    [SerializeField] private Sprite rangeStatIcon;
    [SerializeField] private Sprite bulletSpeedStatIcon;
    [SerializeField] private Sprite rotationSpeedStatIcon;

    public event Action<Vector3, Tower> TowerBuy; 
    public event Action<Vector3, Tower> TowerSell;

    private Vector3 position;
    private int selectedIdx = -1;

    private void Start()
    {
        for (int i = 0; i < towerList.Length; i++)
        {
            if (i >= GameManager.Instance.TowerList.Count)
            {
                towerList[i].gameObject.SetActive(false);
                continue;
            }
            var tower = GameManager.Instance.TowerList[i];
            towerList[i].SetIcon(tower.Icon);
            towerList[i].SetIndex(this, i);
            
        }
        ResetPanel();
    }

    public void BuyTower()
    {
        var tower = GameManager.Instance.TowerList[selectedIdx];
        var instTower = Instantiate(tower.gameObject, position, Quaternion.identity).GetComponent<Tower>();
        GameManager.Instance.Purchase(tower.Price);
        TowerBuy?.Invoke(position, instTower);
        ClosePanel();
    }

    public void ResetPanel()
    {
        selectedIdx = -1;
        buyBtn.gameObject.SetActive(false);
        towerIcon.gameObject.SetActive(false);
        towerNameText.text = "Buy Tower";
        for (int i = 0; i < statList.Length; i++)
        {
            statList[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel(Vector3 pos)
    {
        ResetPanel();
        position = pos;
        gameObject.SetActive(true);
    }
    
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void Select(int idx)
    {
        selectedIdx = idx;
        buyBtn.gameObject.SetActive(true);
        towerIcon.gameObject.SetActive(true);

        var tower = GameManager.Instance.TowerList[idx];
        towerIcon.sprite = tower.Icon;
        towerNameText.text = tower.Type.ToString();
        priceText.text = tower.Price.ToString("N0");

        buyBtn.interactable = GameManager.Instance.Money >= tower.Price;

        for (int i = 0; i < statList.Length; i++)
        {
            statList[i].gameObject.SetActive(true);
            switch (i)
            {
                case 0:
                    statList[i].SetStat("Damage", tower.Damage, "", damageStatIcon);
                    break;
                case 1:
                    statList[i].SetStat("Bullet Speed", tower.ProjectileSpeed, "m/s", bulletSpeedStatIcon);
                    break;
                case 2:
                    statList[i].SetStat("Rotation Speed", tower.RotationSpeed, "deg/s", rotationSpeedStatIcon);
                    break;
                case 3:
                    statList[i].SetStat("Attack Speed", tower.AttackSpeed, "p/s", attackSpeedStatIcon);
                    break;
                case 4:
                    statList[i].SetStat("Range", tower.Range, "m", rangeStatIcon);
                    break;
                default:
                    if (i-5 >= tower.OtherStatistics.Length) statList[i].gameObject.SetActive(false);
                    else statList[i].SetStat(tower.OtherStatistics[i-5].name, tower.OtherStatistics[i-5].value, tower.OtherStatistics[i-5].unitString, tower.OtherStatistics[i-5].icon);
                    break;
            }
        }
    }
}
