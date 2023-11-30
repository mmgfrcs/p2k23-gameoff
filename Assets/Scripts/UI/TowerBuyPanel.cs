﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerBuyPanel : MonoBehaviour
{
    [Header("UI"), SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TowerBuyListItem[] towerList;
    [SerializeField] private TowerBuyStats[] statList;
    [SerializeField] private Button buyBtn;
    
    private Sprite _damageStatIcon;
    private Sprite _attackSpeedStatIcon;
    private Sprite _rangeStatIcon;
    private Sprite _bulletSpeedStatIcon;
    private Sprite _rotationSpeedStatIcon;

    public event Action<Vector3, Tower> TowerBuy; 
    public event Action<Vector3, Tower> TowerSell;

    private Vector3 _position;
    private int _selectedIdx = -1;
    private GameObject _sBox;

    private void Awake()
    {
        _damageStatIcon = Resources.Load<Sprite>("UI/DamageIcon");
        _attackSpeedStatIcon = Resources.Load<Sprite>("UI/AttackSpeedIcon");
        _rangeStatIcon = Resources.Load<Sprite>("UI/RangeIcon");
        _bulletSpeedStatIcon = Resources.Load<Sprite>("UI/BulletSpeedIcon");
        _rotationSpeedStatIcon = Resources.Load<Sprite>("UI/RotationSpeedIcon");
    }

    private void Start()
    {
        CreateSelectionBox();
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

    private void CreateSelectionBox()
    {
        if (_sBox != null) return;
        _sBox = new GameObject("SelectionBox");
        var sr = _sBox.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("UI/SelectionBox");
        sr.sortingOrder = 999;
        _sBox.transform.localScale = Vector3.one * 1.2f;
        _sBox.gameObject.SetActive(false);
    }

    public void BuyTower()
    {
        var tower = GameManager.Instance.TowerList[_selectedIdx];
        var instTower = Instantiate(tower.gameObject, _position, Quaternion.identity).GetComponent<Tower>();
        GameManager.Instance.Purchase(tower.Price);
        TowerBuy?.Invoke(_position, instTower);
        ClosePanel();
    }

    public void ResetPanel()
    {
        _selectedIdx = -1;
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
        _position = pos;
        CreateSelectionBox();
        _sBox.gameObject.SetActive(true);
        _sBox.transform.position = pos;
        gameObject.SetActive(true);
    }
    
    public void ClosePanel()
    {
        CreateSelectionBox();
        _sBox.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Select(int idx)
    {
        _selectedIdx = idx;
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
                    statList[i].SetStat("Damage", tower.Damage, "", false, _damageStatIcon);
                    break;
                case 1:
                    statList[i].SetStat("Bullet Speed", tower.ProjectileSpeed, "m/s", false, _bulletSpeedStatIcon);
                    break;
                case 2:
                    statList[i].SetStat("Rotation Speed", tower.RotationSpeed, "deg/s", false, _rotationSpeedStatIcon);
                    break;
                case 3:
                    statList[i].SetStat("Attack Speed", tower.AttackSpeed, "p/s", true, _attackSpeedStatIcon);
                    break;
                case 4:
                    statList[i].SetStat("Range", tower.Range, "m", true, _rangeStatIcon);
                    break;
                default:
                    if (i-5 >= tower.OtherStatistics.Length) statList[i].gameObject.SetActive(false);
                    else statList[i].SetStat(tower.OtherStatistics[i-5].name, tower.OtherStatistics[i-5].value, tower.OtherStatistics[i-5].unitString, tower.OtherStatistics[i-5].isDecimal, tower.OtherStatistics[i-5].icon);
                    break;
            }
        }
    }
}