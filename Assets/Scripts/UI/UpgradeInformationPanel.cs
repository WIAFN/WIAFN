using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeInformationPanel : MonoBehaviour
{
    public TextMeshPro header;
    public TextMeshPro description;

    public void SetTexts(string header, string description)
    {
        this.header.text = header;
        this.description.text = description;
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
}
