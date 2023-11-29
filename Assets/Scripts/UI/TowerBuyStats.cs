﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerBuyStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI statValue;
    [SerializeField] private Image statIcon;

    public void SetStat(string name, float value, string unit, Sprite icon)
    {
        statName.text = name;
        statValue.text = $"{value:N0} {unit}";
        statIcon.sprite = icon;
    }
}