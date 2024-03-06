using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Click : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIBgBlur blur = transform.parent.GetComponent<UIBgBlur>();
        Image image = transform.parent.Find("Image").GetComponent<Image>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => 
        {
            blur.SetEnanble(true);
            image.gameObject.SetActive(false);
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
