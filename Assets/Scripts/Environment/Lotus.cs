using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lotus : MonoBehaviour
{
    [SerializeField] private int id;

    private List<int> _lotusIds;
    private void Start()
    {
        _lotusIds = PlayerPrefsManager.GetIntList(PlayerPrefsKeys.LotusInventory);
        if (_lotusIds.Contains(id))
        {
            gameObject.SetActive(false);
        }
    }

    public void Pick(int id = -100)
    {
        if (id == -100)
        {
            id = this.id;
        }
        _lotusIds.Add(id);
        PlayerPrefsManager.SetIntList(PlayerPrefsKeys.LotusInventory, _lotusIds);
    }
}
