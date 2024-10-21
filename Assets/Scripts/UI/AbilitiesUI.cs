using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesUI : MonoBehaviour
{
    // [SerializeField] private TMP_Text whiteLotusCountText;
    [SerializeField] private Slider whiteLotusSlider;
    [SerializeField] private TMP_Text pinkLotusCountText;
    [SerializeField] private TMP_Text blueLotusCountText;

    private void Start()
    {
        SetAbilitiesCount();
    }

    public void SetAbilitiesCount()
    {
        // whiteLotusCountText.text = PlayerPrefsManager.GetInt(PlayerPrefsKeys.WhiteLotus, 0).ToString();
        whiteLotusSlider.value = PlayerPrefsManager.GetFloat(PlayerPrefsKeys.WhiteLotus, 0);
        pinkLotusCountText.text = PlayerPrefsManager.GetInt(PlayerPrefsKeys.PinkLotus, 0).ToString();
        blueLotusCountText.text = PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0).ToString();
    }
}
