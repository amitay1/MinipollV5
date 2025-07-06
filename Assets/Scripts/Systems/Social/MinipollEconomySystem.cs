/***************************************************************
 *  MinipollEconomySystem.cs
 *
 *  תיאור כללי:
 *    מערכת "כלכלה" בסיסית עבור המיניפול:
 *      - מחזיק currency (coins/food tokens)
 *      - רכישת/מכירת משאבים (אם מנוהלים ע"י Tribe או EconomyManager גלובלי)
 *      - סחר בין מיניפולים (החלפת משאבים תמורת מטבע)
 *
 *  דרישות קדם:
 *    - לשים על כל Minipoll (יחד עם Brain וכו’).
 *    - EconomyManager גלובלי שמגדיר מחירי משאבים (אם נרצה).
 ***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using MinipollGame.Systems.Core;
using MinipollGame.Core;

public class MinipollEconomySystem : MonoBehaviour
{
    [Header("Currency")]
    public int coins = 0;

    [Header("Inventory")]
    public Dictionary<string,int> items = new Dictionary<string,int>();
    // יכול להחזיק סוגי משאבים כמו “Food”=3, “Wood”=5...

    [Header("Settings")]
    public bool selfUpdate = false;
    private MinipollBrain brain;

    private void Awake()
    {
        brain = GetComponent<MinipollBrain>();
    }

    private void Update()
    {
        if (selfUpdate)
        {
            UpdateEconomy(Time.deltaTime);
        }
    }

    public void UpdateEconomy(float deltaTime)
    {
        // כרגע לא נעשה הרבה. אפשר להוסיף לוגיקה של בלאי, מסים וכו’.
    }

    /// <summary>
    /// הוספת פריט למלאי
    /// </summary>
    public void AddItem(string itemName, int quantity)
    {
        if (!items.ContainsKey(itemName))
        {
            items[itemName] = 0;
        }
        items[itemName] += quantity;
    }

    /// <summary>
    /// הסרת פריט מהמלאי
    /// </summary>
    public bool RemoveItem(string itemName, int quantity)
    {
        if (!items.ContainsKey(itemName)) return false;
        if (items[itemName] < quantity) return false;

        items[itemName] -= quantity;
        if (items[itemName] <= 0) 
            items.Remove(itemName);

        return true;
    }

    /// <summary>
    /// סחר ישיר בין שני מיניפולים
    /// למשל: traderA מוכר X "Food" ל-traderB תמורת Y מטבעות
    /// </summary>
    public static bool TradeItems(MinipollEconomySystem seller, MinipollEconomySystem buyer,
                                  string itemName, int quantity, int price)
    {
        // בודקים אם למוכר יש את הפריט ולbuyer יש מטבעות
        if (!seller.items.ContainsKey(itemName) || seller.items[itemName] < quantity) 
            return false;
        if (buyer.coins < price) 
            return false;

        // מעבירים פריט
        seller.RemoveItem(itemName, quantity);
        buyer.AddItem(itemName, quantity);

        // מעבירים מטבעות
        seller.coins += price;
        buyer.coins -= price;
        Debug.Log($"Trade success: {seller.name} sold {quantity} of {itemName} to {buyer.name} for {price} coins.");

        return true;
    }

    internal void EarnCurrency(float price, string v)
    {
        throw new NotImplementedException();
    }

    internal void SpendCurrency(float price, string v)
    {
        throw new NotImplementedException();
    }

    internal void AddItem(EconomicItem economicItem)
    {
        throw new NotImplementedException();
    }

}
