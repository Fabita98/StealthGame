using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class copyText : MonoBehaviour
{
    TMPro.TextMeshProUGUI text;
    public GameObject TextHRValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = TextHRValue.GetComponent<Text>().text;
    }
}
